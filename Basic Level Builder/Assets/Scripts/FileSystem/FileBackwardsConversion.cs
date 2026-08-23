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

      if (fileVersion < new Version(1,0,0,0))
      {
        if (FileSystem.Instance.TryConvertV0FileToV1_2File(oldFilePath, fileName, out string newFilePath))
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
      .Where(path => FileDirUtilities.IsValidExtension(path) && (FileDirUtilities.GetFileVersion(path) < new Version(1,0,0,0)));
  }
}
