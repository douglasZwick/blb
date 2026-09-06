/***************************************************
Authors:        Brenden Epp
Last Updated:   7/1/2026

Copyright 2018-2026, DigiPen Institute of Technology
***************************************************/

using UnityEngine;
using System.IO;
using UnityEngine.UI;
using NewestFileData;
using FileInfo = NewestFileData.FileInfo;

public class UiConvertedFilePreviewItem : MonoBehaviour
{
    [SerializeField]
    private Image m_ThumbnailImage;
    [SerializeField]
    private TMPro.TextMeshProUGUI m_FileNameText;

    public void Init(string fullFilePath)
    {
        FileSystem.Instance.GetFileInfoFromFullFilePath(fullFilePath, out FileInfo fileInfo);
        LevelData levelData = LevelVersioning.GetLastManualSaveData(fileInfo.m_FileData);

        string fileName = Path.GetFileName(fullFilePath);
        string date = levelData.m_TimeStamp.ToString("M/d/yy h:mm:sstt").ToLower();
        string previewFileNameText = "<b>" + fileName + "</b>" + System.Environment.NewLine + "<color=#C6C6C6>" + date + "</color>";

        m_FileNameText.text = previewFileNameText;
        m_ThumbnailImage.sprite = LevelVersioning.GetThumbnailSprite(levelData);
    }
}
