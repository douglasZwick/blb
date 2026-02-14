/***************************************************
Authors:        Brenden Epp
Last Updated:   11/17/2025

Copyright 2018-2025, DigiPen Institute of Technology
***************************************************/

using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class UiRestoreBackup : ModalDialog
{
  [SerializeField]
  private TMPro.TextMeshProUGUI m_FilePreviewText;
  [SerializeField]
  private Image m_FileThumbnail;
  private string m_fullFilePath;
  private LevelVersioning.LevelVersion m_VersionToLoad = new(0,0);

  public override void StringsSetup(string[] strings = null)
  {
    if (strings.Length < 1) {
      throw new Exception($"File path not passed to restore backup ui");
    }

    m_fullFilePath = strings[0];

    FileSystem.Instance.GetFileInfoFromFullFilePath(m_fullFilePath, out FileSystemInternal.FileInfo fileInfo);

    string fileName = Path.GetFileNameWithoutExtension(m_fullFilePath);
    string timestamp = File.GetLastWriteTime(m_fullFilePath).ToString("M/d/yy h:mm:sstt").ToLower();
    m_FilePreviewText.text = $"<b>{fileName}</b>{Environment.NewLine}<color=#C6C6C6>{timestamp}</color>";

    m_VersionToLoad.m_AutoVersion = LevelVersioning.GetLastAutoSaveVersion(fileInfo.m_FileData, 0);
    LevelVersioning.GetVersionLevelData(fileInfo.m_FileData, m_VersionToLoad, out FileSystemInternal.LevelData levelData);
    m_FileThumbnail.sprite = LevelVersioning.GetThumbnailSprite(levelData);
  }

  public override void Open()
  {
    var anchoredX = 0;
    var anchoredY = 0;
    var anchoredPosition = new Vector2(anchoredX, anchoredY);
    m_RectTransform.anchoredPosition = anchoredPosition;

    base.Open();
  }


  public override void Close()
  {
    base.Close();
    Destroy(gameObject);
  }

  public void YesPressed()
  {
    // Load temp file
    FileSystem.Instance.LoadFromFullFilePath(m_fullFilePath, m_VersionToLoad);
    Close();
  }

  public void NoPressed()
  {
    // Delete temp file
    File.Delete(m_fullFilePath);
    Close();
  }
}
