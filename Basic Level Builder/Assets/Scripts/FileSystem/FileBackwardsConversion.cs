/***************************************************
Authors:        Brenden Epp
Last Updated:   1/21/2026

Copyright 2018-2026, DigiPen Institute of Technology
***************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class FileBackwardsConversion
{  
  readonly static public string s_Version0DirectoryName = "Default Project";
  readonly static public string s_OldFileDirectoryName = "Old Saves";

  // Latest version handled by each conversion step
  readonly static public Version[] s_LatestFileVersionPerConversion =
  {
    new(1,0,0,0),
  };

  static public void ConvertAndMoveAllFiles(string currentDirectoryPath)
  {
    MoveV0FilesToNewDirectory(currentDirectoryPath);
    MoveAndConvertOldFiles(currentDirectoryPath);
  }

  static private string GetOldFileDirectoryPath()
  {
    string documentsPath = FileDirUtilities.GetDocumentsPath();
    return Path.Combine(documentsPath, FileDirUtilities.s_RootDirectoryName, s_OldFileDirectoryName);
  }

  // Moves all files from "Default Project" to "Saves" and removes the old directory
  static private void MoveV0FilesToNewDirectory(string currentDirectoryPath)
  {
    string documentsPath = FileDirUtilities.GetDocumentsPath();
    string rootPath = Path.Combine(documentsPath, FileDirUtilities.s_RootDirectoryName);
    string v0FilePath = Path.Combine(rootPath, s_Version0DirectoryName);
    if (!Directory.Exists(v0FilePath))
      return;

    foreach (string file in Directory.GetFiles(v0FilePath))
    {
      string newPath = Path.Combine(currentDirectoryPath, Path.GetFileName(file));
      File.Move(file, newPath);
    }

    Directory.Delete(v0FilePath);
  }

  static private void MoveAndConvertOldFiles(string currentDirectoryPath)
  {
    List<string> movedFiles = MoveInvalidFiles(FindAllInvalidFiles(currentDirectoryPath));
    List<string> corruptedFiles = new();

    foreach (string filePath in movedFiles)
    {
      if (FileDirUtilities.GetFileVersion(filePath) < s_LatestFileVersionPerConversion[0])
      {
        if (FileSystem.Instance.TryConvertV0FileToV1File(filePath) == false)
          corruptedFiles.Add(filePath);
      }
    }

    // TODO create ui to show all converted files
    // ALSO TEST
    // TODO add coda to see if use wants to delete corrupted files
  }

  static private List<string> MoveInvalidFiles(IEnumerable<string> oldFiles)
  {
    var movedFilePaths = new List<string>();
    
    if (oldFiles.Count() <= 0)
      return movedFilePaths;
    
    string oldFileDirectoryPath = GetOldFileDirectoryPath();
    if (!Directory.Exists(oldFileDirectoryPath))
      Directory.CreateDirectory(oldFileDirectoryPath);

    foreach (string file in oldFiles)
    {
      string newPath = Path.Combine(oldFileDirectoryPath, Path.GetFileName(file));
      File.Move(file, newPath);
      movedFilePaths.Add(newPath);
    }

    return movedFilePaths;
  }

  static private List<string> FindAllInvalidFiles(string currentDirectoryPath)
  {
    return new List<string>(GetInvalidFilesFromDirectory(currentDirectoryPath));
  }

  // Gets old files or files that can't be read
  private static IEnumerable<string> GetInvalidFilesFromDirectory(string directoryPath)
  {
    return Directory.GetFiles(directoryPath)
      .Where(path => FileDirUtilities.IsValidExtension(path) && (FileDirUtilities.GetFileVersion(path) < FileDirUtilities.s_OldestSupportedSaveFileVersion));
  }
}
