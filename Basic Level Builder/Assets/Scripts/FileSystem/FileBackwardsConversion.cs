/***************************************************
Authors:        Brenden Epp
Last Updated:   8/24/2026

Copyright 2018-2026, DigiPen Institute of Technology
***************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using NewestFileData;
using FileInfo = NewestFileData.FileInfo;

public class FileBackwardsConversion
{
  // NOTE: calling Path.GetExtension on a file with the old extension will return only the part of the extension after the last dot.
  readonly static private string s_ConvertedFileExtension = ".old" + FileDirUtilities.s_FilenameExtension;

  static public void ConvertAllOldFiles()
  {
    string documentsPath = FileDirUtilities.GetDocumentsPath();
    string rootPath = Path.Combine(documentsPath, FileDirUtilities.s_RootDirectoryName);
    List<string> invalidFiles = FindAllInvalidFiles(rootPath);
    List<string> corruptedFiles = new();
    List<string> convertedFiles = new();

    foreach (string filePath in invalidFiles)
    {
      string fileName = Path.GetFileName(filePath);

      // If the file name has the old extension, it was already converted
      if (fileName.EndsWith(s_ConvertedFileExtension))
        continue;

      Version fileVersion = FileDirUtilities.GetFileVersion(filePath);

      // Rename the file to indicate it's been converted
      // We rename before conversion so we don't overwrite the original, as the converted file would have the same name
      string oldFilePath = AddOldExtension(filePath);
      File.Move(filePath, oldFilePath);

      if (fileVersion < new Version(1, 0, 0, 0))
      {
        if (TryConvertV0FileToV1_2File(oldFilePath, fileName, out string newFilePath))
        {
          convertedFiles.Add(newFilePath);
        }
        else
        {
          corruptedFiles.Add(fileName);
        }
      }
    }

    if (convertedFiles.Count + corruptedFiles.Count > 0)
    {
      FileSystem.Instance.WaitForSavingToFinish();
      DialogManager.ShowConvertedFilesDialog(convertedFiles, corruptedFiles);
    }
  }

  static public bool IsFileConverted(string fullFilePath)
  {
    return File.Exists(fullFilePath) && fullFilePath.EndsWith(s_ConvertedFileExtension);
  }

  static private string AddOldExtension(string filePath)
  {
    return Path.ChangeExtension(filePath, s_ConvertedFileExtension);
  }

  static private List<string> FindAllInvalidFiles(string currentDirectoryPath)
  {
    return new List<string>(GetInvalidFilesFromDirectory(currentDirectoryPath));
  }

  // Gets old files or files that can't be read
  private static IEnumerable<string> GetInvalidFilesFromDirectory(string directoryPath)
  {
    // Recursively get all invalid files from the directory and its subdirectories
    return Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories)
      .Where(path =>
        FileDirUtilities.IsValidExtension(path) &&
        !path.EndsWith(s_ConvertedFileExtension) &&
        (FileDirUtilities.GetFileVersion(path) < new Version(1, 0, 0, 0)));
  }

  // Returns true if the conversion was sucessful
  private static bool TryConvertV0FileToV1_2File(string filePathToConvert, string newFileName, out string newFilePath)
  {
    newFilePath = "";
    try
    {
      string[] jsonStrings = File.ReadAllLines(filePathToConvert);

      int failedLines = TryCreateDictonaryFromJsonStrings(jsonStrings, out Dictionary<Vector2Int, FileV1_2Data.Element> gridDictionary);

      if (failedLines <= -1)
      {
        StatusBar.Print($"This level seems to be invalid and can not be converted.");
        Debug.Log($"File with path\"" + filePathToConvert + "\" was unable to be converted.");
        return false;
      }
      else if (failedLines > 0)
      {
        StatusBar.Print($"File converted with {failedLines} read failures");
      }



      // Create file data V1_2 from the tile dictonary
      FileV1_2Data.FileInfo sourceFileInfo = new()
      {
        m_FileHeader = new(),
        m_FileData = new()
      };
      FileV1_2Data.LevelData levelData = new()
      {
        m_AddedTiles = new List<FileV1_2Data.Element>(gridDictionary.Values),
      };

      sourceFileInfo.m_FileData.m_ManualSaves.Add(levelData);

      FileInfo convertedFileInfo = (FileInfo)sourceFileInfo.ConvertToNewest();


      bool autosave = false;
      bool isSaveAs = true;
      bool updateCameraPosButtonPressed = false;
      bool shouldPrintElapsedTime = false;
      bool shouldMountFile = false;
      var directoryPath = FileSystem.Instance.GetDirectoryPath();
      var baseFileName = Path.GetFileNameWithoutExtension(newFileName);
      newFilePath = Path.Combine(directoryPath, newFileName);

      // If a file already exists with the same name in the default directory, change the file name
      int duplicateIndex = 1;
      while (File.Exists(newFilePath))
      {
        newFileName = $"{baseFileName} ({duplicateIndex}){FileDirUtilities.s_FilenameExtension}";
        newFilePath = Path.Combine(directoryPath, newFileName);
        duplicateIndex++;
      }

      Dictionary<Vector2Int, Element> grid = convertedFileInfo.m_FileData.m_ManualSaves[0].m_AddedTiles
        .ToDictionary(element => element.m_GridIndex);
      FileSystem.Instance.StartSavingThreadForConversion(newFilePath, convertedFileInfo, grid, autosave, isSaveAs, updateCameraPosButtonPressed, shouldPrintElapsedTime, shouldMountFile);
    }
    catch (Exception e)
    {
      Debug.LogError($"Error while loading. {e.Message} ({e.GetType()})");
      return false;
    }
    return true;
  }

  // Creates a grid of tiles from JSON strings from BLB V0
  // Returns the number of failures. If there were no sucesses, returns -1.
  private static int TryCreateDictonaryFromJsonStrings(string[] jsonStrings, out Dictionary<Vector2Int, FileV1_2Data.Element> gridDictionary)
  {
    int successes = 0;
    int failures = 0;

    bool startTileFound = false;
    Vector2 camPos = Vector2.zero;
    Vector2 minBounds = new(float.MaxValue, float.MaxValue);
    Vector2 maxBounds = new(float.MinValue, float.MinValue);

    gridDictionary = new();
    foreach (var jsonString in jsonStrings)
    {
      try
      {
        FileV1_2Data.Element element = JsonUtility.FromJson<FileV1_2Data.Element>(jsonString);
        Vector2Int index = element.m_GridIndex;
        gridDictionary.Add(index, element);

        if (!startTileFound)
        {
          if (element.m_Type == FileV1_2Data.TileType.START)
          {
            camPos = index;
            startTileFound = true;
          }

          if (index.x < minBounds.x)
            minBounds.x = index.x;
          if (index.x > maxBounds.x)
            maxBounds.x = index.x;
          if (index.y < minBounds.y)
            minBounds.y = index.y;
          if (index.y > maxBounds.y)
            maxBounds.y = index.y;
        }

        ++successes;
      }
      catch (System.ArgumentException e)
      {
        Debug.Log($"Failed to parse the line \"{jsonString}\" " +
          $"as a grid element. {e.Message} ({e.GetType()})");

        ++failures;
      }
    }

    if (successes > 0)
    {
      if (!startTileFound)
        camPos = maxBounds - minBounds;

      Camera.main.transform.position = new Vector3(camPos.x, camPos.y, Camera.main.transform.position.z);
      return failures;
    }
    return -1;
  }
}
