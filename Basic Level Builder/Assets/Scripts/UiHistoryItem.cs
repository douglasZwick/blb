using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UiHistoryItem : MonoBehaviour
{
  public TextMeshProUGUI m_Text;
  public Color m_AutosaveColor = Color.white;
  public Color m_ManualSaveColor = Color.yellow;

  FileSystem m_FileSystem;
  public string m_FullPath { get; private set; }


  public void Setup(FileSystem fileSystem, string fullPath, string fileName)
  {
    m_FileSystem = fileSystem;
    m_FullPath = fullPath;
    m_Text.text = fileName;

    m_Text.color = fileName.StartsWith("Auto") ? m_AutosaveColor : m_ManualSaveColor;
  }


  public void Load()
  {
    m_FileSystem.LoadFromFullPath(m_FullPath);
  }
}
