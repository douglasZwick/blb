/***************************************************
Authors:        Brenden Epp
Last Updated:   3/24/2025

Copyright 2018-2025, DigiPen Institute of Technology
***************************************************/

using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class UiGeneralInfoTab : UiTab
{
  [SerializeField]
  private UiFileInfo m_FileInfo;

  [Header("Visual Components")]
  [SerializeField]
  private TMPro.TextMeshProUGUI m_SaveNumberTxt;
  [SerializeField]
  private TMPro.TextMeshProUGUI m_CreationDateTxt;
  [SerializeField]
  private Image m_FileThumbnail;
  [SerializeField]
  private TMPro.TextMeshProUGUI m_LatestVersionTxt;
  [SerializeField]
  private TMPro.TMP_InputField m_DescriptionInputField;
  [SerializeField]
  private InputFieldUiHelper m_FileNameInputFieldHelper;

  public override void InitLoad(string fullFilePath)
  {
    // Get data
    if (ReadFile(out FileSystemInternal.FileInfo fileInfo))
      return;

    // Set text from file data
    m_FileNameInputFieldHelper.SetText(Path.GetFileNameWithoutExtension(fullFilePath));

    UpdateLatestVersionPreview(fileInfo);
  }

  public override void OpenTab()
  {
    if (ReadFile(out FileSystemInternal.FileInfo fileInfo))
      return;
    UpdateLatestVersionPreview(fileInfo);
  }

  // Returns true if an error occured
  private bool ReadFile(out FileSystemInternal.FileInfo fileInfo)
  {
    try
    {
      FileSystem.Instance.GetFileInfoFromFullFilePath(m_FileInfo.FullFilePath, out fileInfo);
    }
    catch (Exception e)
    {
      Debug.LogWarning($"Failed to get data from file path: {m_FileInfo.FullFilePath}. {e.Message}");
      StatusBar.Print($"Error: Could not load file history for file \"{Path.GetFileName(m_FileInfo.FullFilePath)}\"");
      FindObjectOfType<UiFileInfo>().CloseWindow();
      fileInfo = new();
      return true;
    }
    return false;
  }

  private void UpdateLatestVersionPreview(FileSystemInternal.FileInfo fileInfo)
  {
    m_SaveNumberTxt.text = fileInfo.m_FileData.m_ManualSaves.Count + " Manual Saves    " + fileInfo.m_FileData.m_AutoSaves.Count + " Auto Saves";
    string timeStamp = File.GetCreationTime(m_FileInfo.FullFilePath).ToString("M/d/yy h:mm:sstt").ToLower();
    m_CreationDateTxt.text = $"<b>Created on:</b> <color=#C6C6C6>{timeStamp}</color>";

    // Set the text description for the file
    m_DescriptionInputField.text = fileInfo.m_FileData.m_Description;
    m_DescriptionInputField.ForceLabelUpdate();

    // Get latest manual save and its thumbnail
    FileSystemInternal.LevelData levelData;
    // Check if we have any data to read
    if (fileInfo.m_FileData.m_ManualSaves.Count > 0)
      levelData = fileInfo.m_FileData.m_ManualSaves[^1];
    else
    {
      Debug.LogWarning($"No saves found in file \"{m_FileInfo.FullFilePath}\"");
      StatusBar.Print($"Error: Could not load file history. No saves found in file \"{Path.GetFileName(m_FileInfo.FullFilePath)}\"");
      FindObjectOfType<UiFileInfo>().CloseWindow();
      return;
    }

    m_FileThumbnail.sprite = LevelVersioning.GetThumbnailSprite(levelData);

    // Set text for the latest manial saves timestamp (to show where/when the thumbnail comes from)
    string latestVersionName = levelData.m_Name;

    if (string.IsNullOrEmpty(latestVersionName))
    {
      latestVersionName = "Version " + levelData.m_Version.m_ManualVersion;
    }

    timeStamp = ((DateTime)levelData.m_TimeStamp).ToString("M/d/yy h:mm:sstt").ToLower();
    m_LatestVersionTxt.text = $"<b>{latestVersionName}</b>{Environment.NewLine}<color=#C6C6C6>{timeStamp}</color>";
  }

  public void SetFileDescription(string desc)
  {
    FileSystem.Instance.SetFileDescription(m_FileInfo.FullFilePath, desc);
  }

  // Name input field functions

  public void SetName(string newName)
  {
    // Set the name, and if an error occurs reset the input text to the old name
    if (m_FileInfo.TrySetFileName(newName))
      m_FileNameInputFieldHelper.SetText(Path.GetFileNameWithoutExtension(m_FileInfo.FullFilePath));
  }
}