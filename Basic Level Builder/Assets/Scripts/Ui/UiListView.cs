/***************************************************
Authors:        Douglas Zwick, Brenden Epp
Last Updated:   12/16/2025

Copyright 2018-2025, DigiPen Institute of Technology
***************************************************/

using System.Collections.Generic;
using System.IO;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UiListView : MonoBehaviour
{
  private readonly static Color s_SelectedItemColor = new Color32(82, 111, 155, 255);
  private readonly static Color s_UnselectedItemColor = new Color32(75, 75, 75, 255);

  RectTransform m_RectTransform;

  private void OnDestroy()
  {
    FileSystem.OnAnyFileSaved -= FileUpdated;
  }

  void Awake()
  {
    m_RectTransform = GetComponent<RectTransform>();

    FileSystem.OnAnyFileSaved += FileUpdated;
  }

  public void ItemSetSelected(string fullFilePath)
  {
    var items = m_RectTransform.GetComponentsInChildren<UiSaveFileItem>();
    foreach (var item in items)
    {
      Color color = item.m_FullFilePath == fullFilePath ? s_SelectedItemColor : s_UnselectedItemColor;
      item.SetBackgroundColor(color);
    }
  }

  public void DeselectAll()
  {
    var items = m_RectTransform.GetComponentsInChildren<UiSaveFileItem>();
    foreach (var item in items)
    {
      item.SetBackgroundColor(s_UnselectedItemColor);
    }
  }

  public void Add(RectTransform item)
  {
    AddHelper(item);
  }


  public void Add(List<RectTransform> items)
  {
    foreach (var item in items)
      AddHelper(item);
  }


  public void Remove(RectTransform item)
  {
    RemoveHelper(item);
  }


  public void Remove(List<RectTransform> items)
  {
    foreach (var item in items)
      RemoveHelper(item);
  }


  public void Clear()
  {
    DestroyAll();
  }


  void RemoveHelper(RectTransform item)
  {
    Destroy(item.gameObject);
  }


  void AddHelper(RectTransform item)
  {
    item.SetParent(m_RectTransform);
    item.SetAsFirstSibling();
  }


  void DestroyAll()
  {
    foreach (Transform item in m_RectTransform)
      Destroy(item.gameObject);
  }


  public UiSaveFileItem GetOldestItem()
  {
    var items = m_RectTransform.GetComponentsInChildren<UiSaveFileItem>();
    var returnIndex = items.Length - 1;

    return returnIndex >= 0 ? items[^1] : null;
  }


  public UiSaveFileItem GetItemByFullFilePath(string fullPath)
  {
    var items = m_RectTransform.GetComponentsInChildren<UiSaveFileItem>();

    foreach (var item in items)
    {
      if (string.Compare(item.m_FullFilePath, fullPath, System.StringComparison.InvariantCultureIgnoreCase) == 0)
        return item;
    }

    return null;
  }

  // Will check if each file exists and removes the ui if the file is missing
  public void ValidateAllItems()
  {
    var items = m_RectTransform.GetComponentsInChildren<UiSaveFileItem>();
    List<RectTransform> removeItems = new();
    foreach (var item in items)
    {
      if (!File.Exists(item.m_FullFilePath))
        removeItems.Add(item.GetComponent<RectTransform>());
    }

    Remove(removeItems);
  }

  public void FileUpdated(string fullFilePath)
  {
    var items = m_RectTransform.GetComponentsInChildren<UiSaveFileItem>();
    foreach (var item in items)
    {
      if (string.Compare(item.m_FullFilePath, fullFilePath, System.StringComparison.InvariantCultureIgnoreCase) == 0)
      {
        item.UpdateThumbnail();
        MoveToTop(item);
        break;
      }
    }
  }

  public void MoveToTop(UiSaveFileItem item)
  {
    if (item.transform.parent.parent != m_RectTransform)
      return;

    item.transform.parent.SetAsFirstSibling();
  }
}
