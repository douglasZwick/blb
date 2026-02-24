/***************************************************
Authors:        Douglas Zwick, Brenden Epp
Last Updated:   12/16/2025

Copyright 2018-2025, DigiPen Institute of Technology
***************************************************/

using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UiSaveFileItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
  public TextMeshProUGUI m_Text;
  public TextMeshProUGUI m_TimeStamp;
  public GameObject m_InfoButton;
  [SerializeField]
  private Image m_FileThumbnail;
  private Image m_Background;

  public string m_FullFilePath { get; private set; }

  private bool m_IsMouseHovering = false;

  public void Setup(string fullPath, string fileName, string timeStamp)
  {
    m_FullFilePath = fullPath;
    m_Text.text = fileName;
    m_TimeStamp.text = timeStamp;

    m_Background = GetComponent<Image>();

    UpdateThumbnail();
  }

  public void SetBackgroundColor(Color color)
  {
    m_Background.color = color;
  }

  public void Load()
  {
    FileSystem.Instance.LoadFromFullFilePath(m_FullFilePath, true);
  }

  public void OnPointerEnter(PointerEventData eventData)
  {
    m_IsMouseHovering = true;
    ShowInfoButton();
  }

  public void OnPointerExit(PointerEventData eventData)
  {
    // This check is nessesary because the file info button is ontop of this button, but only the top most button contains the mouse,
    // IE, OnPointerExit will trigger if the mouse leaves the button, or enters the ui button
    bool hovering = RectTransformUtility.RectangleContainsScreenPoint((RectTransform)transform, Input.mousePosition);

    // If the mouse is still over the file button
    if (hovering)
    {
      // Select the file button so the hilight stays on
      EventSystem.current.SetSelectedGameObject(gameObject);
      // Return without deselecting so the info button stays up
      return;
    }

    Deselect();
  }

  public void Update()
  {
    bool hovering = RectTransformUtility.RectangleContainsScreenPoint((RectTransform)transform, Input.mousePosition);

    // If the mouse moved outside the file button, but we are still in the hovered state; unselect button
    // This case is mainly for when the mouse was over the info button and just left
    if (m_IsMouseHovering && !hovering)
      Deselect();
  }

  private void Deselect()
  {
    // Unselect the file button if it was previously selected
    EventSystem.current.SetSelectedGameObject(null);
    m_IsMouseHovering = false;
    HideInfoButton();
  }

  private void ShowInfoButton()
  {
    // Show info button
    m_InfoButton.SetActive(true);
  }

  private void HideInfoButton()
  {
    // Hide info button
    m_InfoButton.SetActive(false);
  }

  public void UpdateThumbnail()
  {    
    FileSystemInternal.FileInfo fileInfo;
    try
    {
      FileSystem.Instance.GetFileInfoFromFullFilePath(m_FullFilePath, out fileInfo);
    }
    catch (Exception e)
    {
      Debug.LogWarning($"Failed to get data from file path: {m_FullFilePath}. {e.Message}");
      StatusBar.Print($"Error: Could not load file thumbnail for file \"{Path.GetFileName(m_FullFilePath)}\".");
      return;
    }

    // Get latest manual save and its thumbnail
    FileSystemInternal.LevelData levelData;
    // Check if we have any data to read
    if (fileInfo.m_FileData.m_ManualSaves.Count > 0)
      levelData = fileInfo.m_FileData.m_ManualSaves[^1];
    else
    {
      Debug.LogWarning($"No saves found in file \"{m_FullFilePath}\"");
      StatusBar.Print($"Error: Could not load file. No saves found in file \"{Path.GetFileName(m_FullFilePath)}\"");
      return;
    }

    m_FileThumbnail.sprite = LevelVersioning.GetThumbnailSprite(levelData);
  }
}