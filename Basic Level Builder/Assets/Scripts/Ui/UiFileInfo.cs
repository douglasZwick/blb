/***************************************************
Authors:        Brenden Epp
Last Updated:   12/27/2025

Copyright 2018-2025, DigiPen Institute of Technology
***************************************************/

using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class UiFileInfo : MonoBehaviour
{
  [SerializeField]
  private TMPro.TextMeshProUGUI m_TitlebarText;

  [SerializeField]
  private Vector2 m_MinimizedTabSizeDelta;
  [SerializeField]
  private Vector2 m_MaximizedTabSizeDelta;
  [SerializeField]
  private Color m_MinimizedColor;
  [SerializeField]
  private Color m_MaximizedColor;
  [SerializeField]
  private ModalDialogAdder m_CodaAdder;

  private List<UiTab> m_Tabs = new();

  public string FullFilePath => m_FullFilePath;

  private string m_FullFilePath;

  void Awake()
  {
    // Adds all the tab bodys to the tab list
    foreach (var tab in GetComponentsInChildren<UiTab>(true))
      m_Tabs.Add(tab);
  }

  private void OnApplicationFocus(bool focus)
  {
    if (focus && !File.Exists(m_FullFilePath))
    {
      var errorString = $"Error: File named \"{Path.GetFileName(m_FullFilePath)}\" could not be found.";
      StatusBar.Warning(errorString);

      CloseWindow();
      // Attempt to remove any open dialougs
      ModalDialog[] dialogs = FindObjectsOfType<ModalDialog>();
      foreach (var dialog in dialogs)
      {
        dialog.Close();
      }
    }
  }

  void Start()
  {
    if (m_Tabs.Count > 0)
      OpenTab(m_Tabs[0]);
  }

  void OnEnable()
  {
    UiHistoryItem.OnCloseInfoWindow += CloseWindow;
    
  }

  void OnDisable()
  {
    UiHistoryItem.OnCloseInfoWindow -= CloseWindow;
  }

  public void InitLoad(string fullFilePath)
  {
    m_FullFilePath = fullFilePath;
    m_TitlebarText.text = Path.GetFileNameWithoutExtension(fullFilePath);

    foreach (var tab in m_Tabs)
      tab.InitLoad(fullFilePath);
  }

  public void CloseWindow()
  {
    GameObject root = GameObject.FindGameObjectWithTag("FileInfoRoot");
    if (!root)
    {
      Debug.LogError("Could not find FileInfoRoot");
      return;
    }

    // Toggle on the black background
    root.GetComponent<Image>().enabled = false;
    GlobalData.DecrementUiPopup();

    Destroy(gameObject);
  }

  // Returns true if successfully renamed file
  public bool TrySetFileName(string name)
  {
    if (FileDirUtilities.IsFileNameValid(name))
    {
      string newFullFilePath = FileSystem.Instance.RenameFile(m_FullFilePath, name);
      // Check to see if rename was valid, as RenameFile returns old file path if can't rename
      if (newFullFilePath != m_FullFilePath)
      {
        m_FullFilePath = newFullFilePath;
        SetTitleBarText(name);
        // Rename successful
        return true;
      }
    }
    // File name is invalid or filesystem failed to rename the file
    return false;
  }

  public void SetTitleBarText(string text)
  {
    m_TitlebarText.text = text;
  }

  public void OpenTab(UiTab tabRef)
  {
    foreach (var tab in m_Tabs)
    {
      tab.gameObject.SetActive(tab == tabRef);

      // Let the opened tab know we have opened it
      if (tab == tabRef)
        tab.OpenTab();

      if (tab == tabRef)
      {
        //(tab.m_TabButton.transform as RectTransform).localScale = m_MaximizedTab.localScale;
        (tab.m_TabButton.transform as RectTransform).sizeDelta = m_MaximizedTabSizeDelta;

        tab.m_TabButton.GetComponent<Image>().color = m_MaximizedColor;
      }
      else
      {
        //(tab.m_TabButton.transform as RectTransform).localScale = m_MinimizedTab.localScale;
        (tab.m_TabButton.transform as RectTransform).sizeDelta = m_MinimizedTabSizeDelta;

        tab.m_TabButton.GetComponent<Image>().color = m_MinimizedColor;
      }
    }
  }

  public void DeleteFileCoda()
  {
    m_CodaAdder.RequestDialogsAtCenterWithStrings($"Are you sure you want to delete this file?{System.Environment.NewLine}This can not be undone.");
    UiConfirmDestructiveActionModalDialog.OnConfirmDestructiveAction += DeleteFile;
    UiConfirmDestructiveActionModalDialog.OnDenyDestructiveAction += CancelDelete;
  }

  public void CancelDelete()
  {
    UnsubFromCoda();
  }

  public void UnsubFromCoda()
  {
    UiConfirmDestructiveActionModalDialog.OnConfirmDestructiveAction -= DeleteFile;
    UiConfirmDestructiveActionModalDialog.OnDenyDestructiveAction -= CancelDelete;
  }

  public void DeleteFile()
  {
    UnsubFromCoda();
    FileSystem.Instance.DeleteFile(m_FullFilePath);
    StatusBar.SilentPrint($"Successfully deleted \"{Path.GetFileName(m_FullFilePath)}\".");
    CloseWindow();
  }
}