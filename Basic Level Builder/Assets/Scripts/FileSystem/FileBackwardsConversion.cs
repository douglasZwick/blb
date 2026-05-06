/***************************************************
Authors:        Brenden Epp
Last Updated:   1/21/2026

Copyright 2018-2026, DigiPen Institute of Technology
***************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class FileBackwardsConversion : MonoBehaviour
{
  readonly static public string s_Version0DirectoryName = "Default Project";
  readonly static public string s_OldFileDirectoryName = "Old Saves";

  // The oldest file version the current save file format can support (read/write)
  readonly static public Version s_OldestSupportedSaveFileVersion = new(1, 0, 0, 0);

  // Latest version handled by each conversion step
  readonly static public Version[] s_LatestFileVersionPerConversion =
  {
    new(1,0,0,0),
  };

  protected string m_OldFileDirectoryPath;

  // Start is called before the first frame update
  void Start()
  {
    InitializeOldFileDirectoryPath();
  }

  private void InitializeOldFileDirectoryPath()
  {
    string documentsPath = FileDirUtilities.GetDocumentsPath();
    m_OldFileDirectoryPath = Path.Combine(documentsPath, FileDirUtilities.s_RootDirectoryName, s_OldFileDirectoryName);
  }

  public void MoveAndConvertOldFiles()
  {
    List<string> movedFiles = MoveOldFiles(FindAllOldFiles());

    foreach (string filePath in movedFiles)
    {
      if (FileSystem.GetFileVersion(filePath) < s_LatestFileVersionPerConversion[0])
      {
        FileSystem.Instance.ConvertV0FileToV1File(filePath);
      }
    }

    // TODO create ui to show all converted files
    // ALSO TEST
  }

  private List<string> MoveOldFiles(IEnumerable<string> oldFiles)
  {
    var movedFilePaths = new List<string>();

    foreach (string file in oldFiles)
    {
      string newPath = Path.Combine(m_OldFileDirectoryPath, Path.GetFileName(file));
      File.Move(file, newPath);
      movedFilePaths.Add(newPath);
    }

    return movedFilePaths;
  }

  private List<string> FindAllOldFiles()
  {
    string documentsPath = FileDirUtilities.GetDocumentsPath();
    string rootPath = Path.Combine(documentsPath, FileDirUtilities.s_RootDirectoryName);

    var oldFiles = new List<string>();

    oldFiles.AddRange(GetOldFilesFromDirectory(Path.Combine(rootPath, s_Version0DirectoryName)));

    oldFiles.AddRange(GetOldFilesFromDirectory(Path.Combine(rootPath, FileDirUtilities.s_DefaultDirectoryName)));

    return oldFiles;
  }

  private static IEnumerable<string> GetOldFilesFromDirectory(string directoryPath)
  {
    return Directory.GetFiles(directoryPath)
      .Where(path => FileSystem.GetFileVersion(path) < s_OldestSupportedSaveFileVersion);
  }
}
