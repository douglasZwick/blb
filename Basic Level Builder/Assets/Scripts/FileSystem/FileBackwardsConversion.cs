/***************************************************
Authors:        Brenden Epp
Last Updated:   6/10/2026

Copyright 2018-2026, DigiPen Institute of Technology
***************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class FileBackwardsConversion
{
  readonly static private string s_ConvertedFilesMetaFile = "ConvertedFiles.meta";

  // Latest version handled by each conversion step
  readonly static public Version[] s_LatestFileVersionPerConversion =
  {
    new(1,0,0,0), // Ignore versions between 1.0.0.0 and 1.2.1.0 as they were alpha builds with diffrent save files that we never released
    new(1,2,0,0),
  };

    static public void ConvertAllOldFiles()
  {
    string documentsPath = FileDirUtilities.GetDocumentsPath();
    string rootPath = Path.Combine(documentsPath, FileDirUtilities.s_RootDirectoryName);
    List<string> invalidFiles = FindAllInvalidFiles(rootPath);
    List<string> corruptedFiles = new();
    List<string> convertedFiles = new();

    List<string> previouslyConvertedFiles = GetConvertedFilesList();

    foreach (string filePath in invalidFiles)
    {
      // Skip files that were converted before
      if (previouslyConvertedFiles.Any(previouslyConvertedFile => previouslyConvertedFile == filePath))
        continue;

      Version fileVersion = FileDirUtilities.GetFileVersion(filePath);
      if (fileVersion < s_LatestFileVersionPerConversion[0])
      {
        if (FileSystem.Instance.TryConvertV0FileToV1File(filePath))
        {
          convertedFiles.Add(filePath);

          string oldFilePath = Path.ChangeExtension(filePath, ".old" + Path.GetExtension(filePath));
          File.Move(filePath, oldFilePath);
          previouslyConvertedFiles.Add(oldFilePath);
        }
        else
          corruptedFiles.Add(filePath);
      }
      // Explicity ignore the alpha versions that we don't support
      else if (fileVersion < s_LatestFileVersionPerConversion[1])
        continue;
    }

    // TODO create ui to show all converted files
    // TODO add coda to see if use wants to delete corrupted files
    WriteConvertedFilesMeta(previouslyConvertedFiles);
  }

  static public bool IsFileConverted(string fullFilePath)
  {
    return File.Exists(fullFilePath) && GetConvertedFilesList().Any(f => f == fullFilePath);
  }

  // Moves all files from "Default Project" to "Saves" and removes the old directory
  static private List<string> GetConvertedFilesList()
  {
    string documentsPath = FileDirUtilities.GetDocumentsPath();
    string rootPath = Path.Combine(documentsPath, FileDirUtilities.s_RootDirectoryName);
    string metaPath = Path.Combine(rootPath, s_ConvertedFilesMetaFile);
    if (!File.Exists(metaPath))
    {
      File.Create(metaPath).Dispose();
      return new List<string>();
    }

    List<string> previouslyConvertedFiles = File.ReadAllLines(metaPath)
      .Where(line => !string.IsNullOrWhiteSpace(line))
      .Select(line => line.Trim())
      .ToList();

    bool listChanged = false;
    for (int i = previouslyConvertedFiles.Count - 1; i >= 0; i--)
    {
      string convertedFile = previouslyConvertedFiles[i];
      // If the old or converted file are missing, remove from the list
      if (!File.Exists(convertedFile) || !File.Exists(Path.Combine(rootPath, Path.GetFileName(convertedFile))))
      {
        previouslyConvertedFiles.RemoveAt(i);
        listChanged = true;
      }
    }

    if (listChanged)
      File.WriteAllLines(metaPath, previouslyConvertedFiles);

    return previouslyConvertedFiles;
  }

  static private void WriteConvertedFilesMeta(List<string> convertedFiles)
  {
    string documentsPath = FileDirUtilities.GetDocumentsPath();
    string rootPath = Path.Combine(documentsPath, FileDirUtilities.s_RootDirectoryName);
    string metaPath = Path.Combine(rootPath, s_ConvertedFilesMetaFile);
    File.WriteAllLines(metaPath, convertedFiles);
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
      .Where(path => FileDirUtilities.IsValidExtension(path) && (FileDirUtilities.GetFileVersion(path) < s_LatestFileVersionPerConversion[^1]));
  }
}
