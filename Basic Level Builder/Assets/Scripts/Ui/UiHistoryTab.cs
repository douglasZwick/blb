/***************************************************
Authors:        Brenden Epp
Last Updated:   3/24/2025

Copyright 2018-2025, DigiPen Institute of Technology
***************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static LevelVersioning;

public class UiHistoryTab : UiTab
{
  [SerializeField]
  private UiFileInfo m_FileInfo;

  [Header("Prefabs")]
  [SerializeField]
  private UiHistoryItem m_ManualSaveItemPrefab;
  [SerializeField]
  private UiHistoryItem m_AutoSaveItemPrefab;
  [SerializeField]
  private Sprite m_MissingThumbnail;

  [Header("Visuals")]
  [SerializeField]
  private RectTransform m_Content;
  [SerializeField]
  private TMPro.TextMeshProUGUI m_VersionInfoText;
  [SerializeField]
  private TMPro.TextMeshProUGUI m_VersionDeltasText;
  [SerializeField]
  private Image m_VersionInfoThumbnail;
  [SerializeField]
  private GameObject m_ShowAutosaveButton;
  [SerializeField]
  private UiIconRotator[] m_ExpandCollapseIcon;
  [SerializeField]
  private TMPro.TextMeshProUGUI m_ExpandCollapseButtonText;

  [Header("Buttons")]
  [SerializeField]
  private GameObject m_ExportButton;
  [SerializeField]
  private GameObject m_PromoteButton;
  [SerializeField]
  private GameObject m_LoadButton;
  [SerializeField]
  private GameObject m_DeleteButton;

  [SerializeField]
  private ModalDialogAdder m_CodaAdder;


  private List<UiHistoryItem> m_Selection = new();

  List<uint> m_ExpandedManuals = new();
  uint m_SelectedSave;

  // The first selected item to use when selecting in a range
  // This will update for as long as the range selection modifier is not held
  // If the modifier is held, it will select all items between this index and the next selected item
  private int m_RangeSelectionFirstIndex = 0;

  void OnEnable()
  {
    UiHistoryItem.OnSelected += OnHistoryItemSelected;
  }

  void OnDisable()
  {
    UiHistoryItem.OnSelected -= OnHistoryItemSelected;
  }

  public override void InitLoad(string fullFilePath)
  {
    LoadHistoryItemList();
    // Select the first item in the list
    OnHistoryItemSelected(GetAllHistoryItems()[0]);
    UpdateVersionInfo();
  }

  private void ClearHistoryItemList()
  {
    while (m_Content.childCount > 0)
    {
      DestroyImmediate(m_Content.GetChild(0).gameObject);
    }
  }

  private void LoadHistoryItemList()
  {
    try
    {
      // Create all the file items
      // Load the files data
      FileSystemInternal.FileInfo fileInfo;

      try
      {
        FileSystem.Instance.GetFileInfoFromFullFilePath(m_FileInfo.FullFilePath, out fileInfo);
      }
      catch (Exception e)
      {
        var errorString = $"Failed to get data from file path: {m_FileInfo.FullFilePath}. {e.Message}";
        StatusBar.Warning($"Error: Could not load file history.", errorString);
        FindObjectOfType<UiFileInfo>().CloseWindow();
        return;
      }

      List<UiHistoryItem> items = new();

      foreach (var levelData in fileInfo.m_FileData.m_ManualSaves)
      {
        items.Add(CreateHistoryItem(levelData, m_FileInfo.FullFilePath, m_ManualSaveItemPrefab));
      }
      foreach (var levelData in fileInfo.m_FileData.m_AutoSaves)
      {
        items.Add(CreateHistoryItem(levelData, m_FileInfo.FullFilePath, m_AutoSaveItemPrefab));
      }

      if (items.Count == 0)
      {
        StatusBar.Warning($"Error: File is empty", $"No versions found in file: {m_FileInfo.FullFilePath}");
        FindObjectOfType<UiFileInfo>().CloseWindow();
        return;
      }

      UpdateVersionList(items);

      // Disable the show autosaves button if we have none
      if (fileInfo.m_FileData.m_AutoSaves.Count == 0)
      {
        m_ShowAutosaveButton.SetActive(false);
      }
      else
      {
        // First time run will collapse all
        ToggleSaveExpansion();
      }
    }
    catch (Exception e)
    {
      StatusBar.Error($"Error: Could not load file history due to an unexpected error.", $"{e.Message} ({e.GetType()})");
      FindObjectOfType<UiFileInfo>().CloseWindow();
    }
  }

  private UiHistoryItem CreateHistoryItem(FileSystemInternal.LevelData levelData, string fullFilePath, UiHistoryItem prefab)
  {
    UiHistoryItem historyItem = Instantiate(prefab);
    // Give level data so it can init its text and thumbnail
    historyItem.Init(levelData, m_FileInfo);
    // Add item to list view
    if (historyItem.TryGetComponent(out RectTransform rect))
    {
      rect.SetParent(m_Content);
    }
    return historyItem;
  }

  public void ToggleSaveExpansion()
  {
    // If we have no expanded saves, set to expand all
    bool shouldExpand = GetNumberOfExpandedSaves() == 0;

    // Loop all save versions
    for (int i = 0; i < m_Content.childCount; i++)
    {
      var child = m_Content.GetChild(i);
      // If the version isn't what we want, toggle expand, else don't
      if (child != null && child.TryGetComponent<UiHistoryItem>(out var item) &&
          item.IsManualSave() && item.IsExpanded() != shouldExpand)
      {
        item.ToggleExpand();
      }
    }

    ToggleExpandCollapseIcon(shouldExpand);
  }

  private void ToggleExpandCollapseIcon(bool expanded)
  {
    // Update expand/collapse button icon
    foreach (var icon in m_ExpandCollapseIcon)
    {
      if (expanded)
        icon.Hide();
      else
        icon.Show();
    }

    m_ExpandCollapseButtonText.text = expanded ? "Collapse All" : "Expand All";
  }

  public void LoadSelectedVersion()
  {
    UiHistoryItem item = m_Selection[0];

    if (item)
      item.Load();
  }

  public void PromoteAutoSave()
  {
    if (m_Selection.Count <= 0)
    {
      throw new Exception("Promoting with no version selected");
    }

    // These should never be the case, but just to make sure...
    if (m_Selection.Count > 1 || m_Selection[0].GetVersion().IsManual())
      return;

    FileSystem.Instance.PromoteAutoSave(m_FileInfo.FullFilePath, m_Selection[0].GetVersion());

    RefreshUi();
  }

  public void ExportSelectedVersions()
  {
    if (m_Selection.Count <= 0)
    {
      throw new Exception("Exporting version(s) with no version(s) selected");
    }

    List<LevelVersion> versions = new();
    foreach (var item in m_Selection)
    {
      versions.Add(item.GetVersion());
    }

    if (m_Selection.Count == 1)
      FileSystem.Instance.ExportVersion(m_FileInfo.FullFilePath, m_Selection[0].GetVersion());
    else
      FileSystem.Instance.ExportMultipleVersions(m_FileInfo.FullFilePath, versions);
  }

  public void DeleteSelectedVersionsCoda()
  {
    FileSystem.Instance.GetFileInfoFromFullFilePath(m_FileInfo.FullFilePath, out FileSystemInternal.FileInfo fileInfo);
    int manualsSelected = 0;
    foreach (var item in m_Selection)
    {
      if (item.GetVersion().IsManual())
        manualsSelected++;
    }
    bool shouldDeleteFile = fileInfo.m_FileData.m_ManualSaves.Count == manualsSelected;

    string prompt = "";

    if (shouldDeleteFile)
    {
      prompt = $"You have selected all available versions." + Environment.NewLine +
        "This action will remove the entire file and all associated data, which cannot undone." + Environment.NewLine +
        "Are you sure you want to permanently delete this file?";
    }
    else
    {
      string target = m_Selection.Count > 1
        ? "these selected versions"
        : "this version";

      prompt = $"Are you sure you want to delete {target}?{Environment.NewLine}This can not be undone.";
    }


    m_CodaAdder.RequestDialogsAtCenterWithStrings(prompt);

    if (shouldDeleteFile)
      UiConfirmDestructiveActionModalDialog.OnConfirmDestructiveAction += DeleteFile;
    else
      UiConfirmDestructiveActionModalDialog.OnConfirmDestructiveAction += DeleteSelectedVersions;
    UiConfirmDestructiveActionModalDialog.OnDenyDestructiveAction += CancelDelete;
  }

  public void CancelDelete()
  {
    UnsubFromCoda();
  }

  public void UnsubFromCoda()
  {
    UiConfirmDestructiveActionModalDialog.OnConfirmDestructiveAction -= DeleteSelectedVersions;
    UiConfirmDestructiveActionModalDialog.OnDenyDestructiveAction -= CancelDelete;
  }

  private void DeleteFile()
  {
    UiConfirmDestructiveActionModalDialog.OnConfirmDestructiveAction -= DeleteFile;
    UiConfirmDestructiveActionModalDialog.OnDenyDestructiveAction -= CancelDelete;
    FindObjectOfType<UiFileInfo>().DeleteFile();
  }

  // TODO: If deleting last manual save ask if want to delete whole file.
  // Or remove delete button if there is only one version left
  public void DeleteSelectedVersions()
  {
    UnsubFromCoda();

    // This shouldn't happen as the button wouldn't be visable if no version are selected
    if (m_Selection.Count <= 0)
    {
      throw new Exception("Deleting version(s) with no version(s) selected");
    }

    FileSystem.Instance.GetFileInfoFromFullFilePath(m_FileInfo.FullFilePath, out FileSystemInternal.FileInfo fileInfo);
    if (m_Selection.Count > 1)
    {
      List<LevelVersion> versions = new();
      int lastManual = -1;
      foreach (var item in m_Selection)
      {
        LevelVersion version = item.GetVersion();

        // Skip auto save if we have selected its manual
        // Because all of a manuals autos are deleted with it, otherwise we are doing a double delete
        if (version.IsManual())
          lastManual = version.m_ManualVersion;
        else if (lastManual == version.m_ManualVersion)
          continue;

        versions.Add(version);
      }

      FileSystem.Instance.DeleteMultipleVersions(fileInfo, versions);
    }
    else
    {
      FileSystem.Instance.DeleteVersion(fileInfo, m_Selection[0].GetVersion());
    }

    RefreshUi();
  }

  private void RefreshUi()
  {
    SaveUiState();
    ClearHistoryItemList();
    LoadHistoryItemList();
    LoadUiState();
    UpdateVersionInfo();
  }

  private void LoadUiState()
  {
    m_Selection.Clear();

    ToggleExpandCollapseIcon(m_ExpandedManuals.Count > 0);

    for (int i = 0; i < m_Content.childCount; i++)
    {
      var child = m_Content.GetChild(i);
      if (child != null && child.TryGetComponent<UiHistoryItem>(out var item))
      {
        if (item.GetId() == m_SelectedSave)
          item.Select();

        if (item.IsManualSave() && m_ExpandedManuals.Any(p => p == item.GetId()))
        {
          item.ToggleExpand();
          m_ExpandedManuals.Remove(item.GetId());
        }
      }

      // If we finished expanding all saves and selecting the last selected
      if (m_ExpandedManuals.Count == 0 && m_Selection.Count == 1)
        break;
    }
  }

  private void SaveUiState()
  {
    if (m_Selection.Count > 0)
      m_SelectedSave = m_Selection[0].GetId();
    else
      m_SelectedSave = 0;

    m_ExpandedManuals.Clear();
    for (int i = 0; i < m_Content.childCount; i++)
    {
      var child = m_Content.GetChild(i);
      if (child != null && child.TryGetComponent<UiHistoryItem>(out var item))
      {
        if (item.IsManualSave() && item.IsExpanded())
        {
          m_ExpandedManuals.Add(item.GetId());
        }
      }
    }
  }

  private int GetNumberOfExpandedSaves()
  {
    int num = 0;
    for (int i = 0; i < m_Content.childCount; i++)
    {
      var child = m_Content.GetChild(i);
      if (child != null && child.TryGetComponent<UiHistoryItem>(out var item) &&
          item.IsManualSave() && item.IsExpanded())
      {
        ++num;
      }
    }
    return num;
  }

  // Sorts, checks lone manual
  private void UpdateVersionList(List<UiHistoryItem> items = null)
  {
    // If no items were passed in, create our own list
    items ??= GetAllHistoryItems();

    // Properly sort items
    items.Sort((a, b) => a.CompareTo(b));

    // Set sorted items index in hierarchy
    for (int i = 0; i < items.Count; i++)
    {
      items[i].transform.SetSiblingIndex(i);

      // Is there a previous item?
      if (i > 0)
      {
        // Set the prev item as the last auto save in the list so the branch lines don'e continue
        // (SetLastAutoSave is ignored if that item is a manual too)
        if (items[i].IsManualSave())
        {
          items[i - 1].SetLastAutoSave();
        }

        // Check if the last item was a manual with no autos
        if (items[i].IsManualSave() && items[i - 1].IsManualSave())
        {
          items[i - 1].SetArrowActive(false);
        }
        else
        {
          items[i - 1].SetArrowActive(true);
        }
      }

      // If this is the last item
      if (i == items.Count - 1)
      {
        // If this is the last item and auto, set as the last auto save in the list
        if (!items[i].IsManualSave())
        {
          items[i].SetLastAutoSave();
        }
        else
        {
          // If we are the last item and manual save, we have no autos, so remove the arrow
          items[i].SetArrowActive(false);
        }
      }
    }
  }

  private List<UiHistoryItem> GetAllHistoryItems()
  {
    List<UiHistoryItem> items = new();

    // Loop all save versions
    for (int i = 0; i < m_Content.childCount; i++)
    {
      var child = m_Content.GetChild(i);
      if (child != null && child.TryGetComponent<UiHistoryItem>(out var item))
      {
        items.Add(item);
      }
    }

    return items;
  }

  private bool IsCameraDifferent(LevelVersion version)
  {
    FileSystem.Instance.GetFileInfoFromFullFilePath(m_FileInfo.FullFilePath, out FileSystemInternal.FileInfo fileInfo);
    return LevelVersioning.IsCameraDifferent(fileInfo.m_FileData, version);
  }

  private void ClearSelection()
  {
    foreach (var item in GetAllHistoryItems())
    {
      item.SetColorAsUnselected();
    }
    m_Selection.Clear();
  }

  private void OnHistoryItemSelected(UiHistoryItem selectedItem)
  {
    // Clear text if null
    if (selectedItem == null)
    {
      ClearSelection();
      return;
    }

    List<UiHistoryItem> items = GetAllHistoryItems();

    if (!HotkeyMaster.IsRangeSelectHeld())
      m_RangeSelectionFirstIndex = items.IndexOf(selectedItem);

    if (HotkeyMaster.IsRangeSelectHeld())
    {
      int endIndex = items.IndexOf(selectedItem);
      int startIndex = m_RangeSelectionFirstIndex;

      // Swap indexes if we are selecing the other side of the list
      if (startIndex > endIndex)
      {
        (startIndex, endIndex) = (endIndex, startIndex);
      }

      ClearSelection();
      for (int i = startIndex; i <= endIndex; i++)
      {
        AddToSelection(items[i]);
      }
    }
    else if (HotkeyMaster.IsMultiSelectHeld())
    {
      if (!m_Selection.Contains(selectedItem))
        AddToSelection(selectedItem);
      else
        RemoveFromSelection(selectedItem);
    }
    else
    {
      ClearSelection();
      AddToSelection(selectedItem);
    }

    m_Selection.Sort((a, b) => a.CompareTo(b));

    UpdateVersionInfo();
  }

  private void AddToSelection(UiHistoryItem item)
  {
    m_Selection.Add(item);
    item.SetColorAndAutosAsSelected();
  }

  private void RemoveFromSelection(UiHistoryItem item)
  {
    m_Selection.Remove(item);
    bool manualSelected = false;
    if (!item.GetVersion().IsManual())
    {
      foreach (UiHistoryItem item2 in m_Selection)
      {
        if (item2.GetVersion().m_ManualVersion == item.GetVersion().m_ManualVersion && item2.GetVersion().IsManual())
          manualSelected = true;
      }
    }
    item.SetColorAndAutosAsUnselected(manualSelected);
  }

  private string GetDeltaDifferencesString(string addedTiles, string removedTiles, bool IsCameraUpdated)
  {
    string diff;

    // Check Tiles
    diff = addedTiles + " Tiles Added";
    diff += Environment.NewLine + removedTiles + " Tiles Removed";

    // Camera is always updated for each version
    if (IsCameraUpdated)
      diff += Environment.NewLine + "Camera Position Updated";

    return diff;
  }

  private void UpdateVersionInfo()
  {
    if (m_Selection.Count == 0)
    {
      m_VersionInfoText.text = "<b>No version selected</b>\r\n";
      m_VersionDeltasText.text = GetDeltaDifferencesString("--", "--", false);

      m_VersionInfoThumbnail.sprite = m_MissingThumbnail;

      // Reenable buttons if they were gone before
      m_ExportButton.SetActive(false);
      m_LoadButton.SetActive(false);
      m_DeleteButton.SetActive(false);
      m_PromoteButton.SetActive(false);
    }
    else if (m_Selection.Count == 1)
    {
      m_VersionInfoText.text = "<b>" + m_Selection[0].GetVersionName() + "</b>\r\n";
      m_VersionInfoText.text += "<color=#C6C6C6>" + m_Selection[0].GetVersionTimeStamp() + "</color>";

      m_VersionDeltasText.text = GetDeltaDifferencesString(
        m_Selection[0].GetAddedTilesCount().ToString(),
        m_Selection[0].GetRemovedTilesCount().ToString(),
        IsCameraDifferent(m_Selection[0].GetVersion()));

      m_VersionInfoThumbnail.sprite = m_Selection[0].GetThumbnail();

      // Reenable buttons if they were gone before
      m_ExportButton.SetActive(true);
      m_LoadButton.SetActive(true);
      m_DeleteButton.SetActive(true);
      m_PromoteButton.SetActive(!m_Selection[0].GetVersion().IsManual());
    }
    else
    {
      m_VersionInfoText.text = "<b>Multiple versions selected</b>\r\n";
      m_VersionInfoText.text += "<color=#C6C6C6>" + m_Selection[0].GetVersionName();

      int addedTiles = 0;
      int removedTiles = 0;
      bool cameraChanged = false;

      foreach (var item in m_Selection)
      {
        addedTiles += item.GetAddedTilesCount();
        removedTiles += item.GetRemovedTilesCount();
        if (cameraChanged)
          cameraChanged = IsCameraDifferent(item.GetVersion());
      }

      m_VersionDeltasText.text = GetDeltaDifferencesString(addedTiles.ToString(), removedTiles.ToString(), cameraChanged);

      m_VersionInfoThumbnail.sprite = m_Selection[^1].GetThumbnail();

      // Add all selected items up to 4 total
      if (m_Selection.Count < 5)
      {
        foreach (var item in m_Selection.GetRange(1, m_Selection.Count - 1))
        {
          m_VersionInfoText.text += ", " + item.GetVersionName();
        }
      }
      else
      {
        // Only add an ellipsis and the last item if we have too many selected
        m_VersionInfoText.text += " .... " + m_Selection[^1].GetVersionName();
      }

      // Remove the load buttons as we can't do that when multiple versions are selected
      m_ExportButton.SetActive(true);
      m_LoadButton.SetActive(false);
      m_DeleteButton.SetActive(true);
      m_PromoteButton.SetActive(false);
    }
  }
}