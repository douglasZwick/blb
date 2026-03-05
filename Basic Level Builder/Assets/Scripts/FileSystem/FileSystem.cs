/***************************************************
Authors:        Brenden Epp
Last Updated:   12/16/2025

Copyright 2018-2025, DigiPen Institute of Technology
***************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using static LevelVersioning;

public class FileSystem : FileSystemInternal
{
  #region singleton
  private static FileSystem _instance;
  public static FileSystem Instance
  {
    get
    {
      if (_instance == null)
      {
        _instance = FindObjectOfType<FileSystem>();
      }
      return _instance;
    }
  }

  private void Awake()
  {
    if (_instance != null && _instance != this)
    {
      Destroy(this.gameObject);
    }
    else
    {
      _instance = this;
    }
  }
  #endregion

  public string GetMountedFilePath()
  {
    return m_MountedFileInfo.m_SaveFilePath;
  }

  /// <summary>
  /// Performs a manual save of the current level.
  /// </summary>
  public void SaveCameraPosition()
  {
    Save(false, null, true);
  }

  /// <summary>
  /// Performs a manual save of the current level.
  /// </summary>
  public void ManualSave()
  {
    Save(false);
  }

  /// <summary>
  /// Performs an automatic save of the current level.
  /// </summary>
  public void Autosave()
  {
    Save(true);
  }

  public void SaveAs(string name, bool shouldPrintElapsedTime = true)
  {
    Save(false, name, false, shouldPrintElapsedTime);
  }

  public void CreateNewLevel()
  {
    TryCreateNewLevel();
  }

  public void ExportMultipleVersions(string sourcePath, List<LevelVersion> versions)
  {
    // Gather the level data to export
    GetFileInfoFromFullFilePath(sourcePath, out FileInfo sourceFileInfo);

    m_PendingExportVersions = versions;
    m_PendingExportFileData = sourceFileInfo.m_FileData;

    // Call dialogue to get export file name
    m_ExportAsDialogAdder.RequestDialogsAtCenterWithStrings();
  }

  public void ExportVersion(string sourcePath, LevelVersion version)
  {
    // Gather the level data to export
    GetFileInfoFromFullFilePath(sourcePath, out FileInfo sourceFileInfo);

    GetVersionLevelData(sourceFileInfo.m_FileData, version, out LevelData levelData);
    levelData.m_AddedTiles = GetGridDictionaryFromFileData(sourceFileInfo, version).Values.ToList();
    // Set the data to be the first version of this file.
    levelData.m_Version = new(1, 0);

    m_PendingExportFileData = new();
    m_PendingExportFileData.m_ManualSaves.Add(levelData);

    // Call dialogue to get export file name
    m_ExportAsDialogAdder.RequestDialogsAtCenterWithStrings();
  }

  public void SetVersionName(string fullFilePath, LevelVersion version, string name)
  {
    FileInfo fileInfo = SetVersionNameEx(fullFilePath, version, name);
    WriteDataToFile(fullFilePath, fileInfo);
  }

  public void SetFileDescription(string fullFilePath, string desc)
  {
    FileInfo fileInfo = SetFileDescriptionEx(fullFilePath, desc);
    WriteDataToFile(fullFilePath, fileInfo);
  }

  /// <summary>
  /// Gets the file info from a file at the specified path.
  /// </summary>
  /// <param name="fullFilePath">The full path to the file.</param>
  /// <param name="fileInfo">The file info to populate.</param>
  /// <exception cref="Exception">Thrown when the file cannot be found.</exception>
  public void GetFileInfoFromFullFilePath(string fullFilePath, out FileInfo fileInfo)
  {
    GetFileInfoFromFullFilePathEx(fullFilePath, out fileInfo);
  }

  /// <summary>
  /// Loads a file from a fill file path as the new mounted file
  /// </summary>
  /// <param name="fullFilePath">The full path to the file.</param>
  /// <param name="askToSave">If there are unsaved changes in the editor, will ask to save them first.</param>
  /// <param name="version">The version of the level to load.</param>
  /// <exception cref="Exception">Thrown when the file cannot be found.</exception>
  public void LoadFromFullFilePath(string fullFilePath, bool askToSave, LevelVersion? version = null)
  {
    if (askToSave)
      LoadFromFullFilePathExAndAskToSave(fullFilePath, version);
    else
      LoadFromFullFilePathEx(fullFilePath, version);
  }

  public void LoadFromTextAsset(TextAsset level)
  {
    LoadFromTextAssetEx(level);
  }

  /// <summary>
  /// Removes a number od saved versions and saves the file
  /// Deletes file if there is no more manual saves left
  /// </summary>
  /// <param name="fileInfo">The file info containing the save.</param>
  /// <param name="versions">A list of versions to delete.</param>
  /// <exception cref="Exception">Thrown when an error occurs.</exception>
  public void DeleteMultipleVersions(FileInfo fileInfo, List<LevelVersion> versions)
  {
    foreach (var version in versions)
    {
      DeleteVersionEx(fileInfo, version);
      UpdateLoadedVersionIfDeleted(fileInfo, version);
    }

    SaveAfterDeletion(fileInfo, "multiple versions");
  }

  /// <summary>
  /// Removes one saved version and saves the file
  /// Deletes file if there is no more manual saves left
  /// </summary>
  /// <param name="fileInfo">The file info containing the save.</param>
  /// <param name="version">The version of the save to delete.</param>
  /// <exception cref="Exception">Thrown when an error occurs.</exception>
  /// 
  public void DeleteVersion(FileInfo fileInfo, LevelVersion version)
  {
    DeleteVersionEx(fileInfo, version);
    SaveAfterDeletion(fileInfo, version.ToString());
    UpdateLoadedVersionIfDeleted(fileInfo, version);
  }

  public void DeleteFile(string fullFilePath)
  {
    DeleteFileEx(fullFilePath);
  }

  public void PromoteAutoSave(string fullFilePath, LevelVersion version)
  {
    GetFileInfoFromFullFilePathEx(fullFilePath, out FileInfo fileInfo);
    GetVersionLevelData(fileInfo.m_FileData, version, out LevelData level);
    PromoteAutoSaveEx(ref fileInfo.m_FileData, level);
    WriteDataToFile(fileInfo.m_SaveFilePath, fileInfo);
  }

  public void ShowDirectoryInExplorer()
  {
    if (Application.platform == RuntimePlatform.WebGLPlayer)
      return;

    if (Application.platform is RuntimePlatform.OSXEditor or RuntimePlatform.OSXPlayer)
    {
      FileDirUtilities.OpenInFinder(GetDirectoryPath());
    }
    else
    {
      Application.OpenURL($"file://{GetDirectoryPath()}");
    }
  }

  public string GetDirectoryPath()
  {
    return m_FileDirUtilities.GetCurrentDirectoryPath();
  }

  public void MainThreadDispatcherQueue(System.Action action)
  {
    m_MainThreadDispatcher.Enqueue(action);
  }

  public void ConfirmOverwrite()
  {
    // Check if we were doing a SaveAs or an Export
    // If the Export data is empty, then we are doing a SaveAs
    if (m_PendingExportFileData == null)
    {
      bool autosave = false;
      bool isSaveAs = true;
      bool updateCameraPosButtonPressed = false;
      bool shouldPrintElapsedTime = true;
      StartSavingThread(m_PendingSaveFullFilePath, m_TileGrid.GetGridDictionary(), autosave, isSaveAs, updateCameraPosButtonPressed, shouldPrintElapsedTime);
    }
    else
    {
      StartExportSavingThread(m_PendingSaveFullFilePath);
    }
    m_PendingSaveFullFilePath = "";
  }

  public void CancelOverwrite()
  {
    m_PendingSaveFullFilePath = "";
  }

  public void TryStartExportSavingThread(string fileName)
  {
    string destFilePath = m_FileDirUtilities.CreateFilePath(fileName);

    // Give prompt if we are going to write to and existing file
    if (File.Exists(destFilePath))
    {
      m_PendingSaveFullFilePath = destFilePath;

      m_OverrideDialogAdder.RequestDialogsAtCenterWithStrings(Path.GetFileName(destFilePath));
      return;
    }

    StartExportSavingThread(destFilePath);
  }

  public void RefreshFileList()
  {
    m_FileDirUtilities.UpdateFilesList();
  }

  public string RenameFile(string oldFilePath, string newFileName)
  {
    string newFilePath = m_FileDirUtilities.RenameFile(oldFilePath, newFileName);
    bool canRename = newFilePath != oldFilePath;

    if (canRename && IsFileMounted(oldFilePath))
    {
      RenameMountedFile(newFilePath);
      m_FileDirUtilities.SetTitleBarFileName(newFileName);
    }

    // Updates the file list after updating the cashed mounted files name
    RefreshFileList();

    return newFilePath;
  }
}