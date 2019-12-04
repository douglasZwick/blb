using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class UiListView : MonoBehaviour
{
  public Color m_EvenColor = new Color(1, 1, 1, 0.1f);
  public Color m_OddColor = new Color(1, 1, 1, 0.05f);

  RectTransform m_RectTransform;


  void Awake()
  {
    m_RectTransform = GetComponent<RectTransform>();
  }


  public void Add(RectTransform item)
  {
    AddHelper(item);
    AssignColors();
  }


  public void Add(List<RectTransform> items)
  {
    foreach (var item in items)
      AddHelper(item);
    AssignColors();
  }


  public void Remove(RectTransform item)
  {
    RemoveHelper(item);
    AssignColors();
  }


  public void Remove(List<RectTransform> items)
  {
    foreach (var item in items)
      RemoveHelper(item);
    AssignColors();
  }


  public void Clear()
  {
    DestroyAll();
  }


  void RemoveHelper(RectTransform item)
  {
    item.SetParent(null);
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


  void AssignColors()
  {
    var images = m_RectTransform.GetComponentsInChildren<Image>();
    var odd = false;

    foreach (var image in images)
    {
      image.color = odd ? m_OddColor : m_EvenColor;
      odd = !odd;
    }
  }


  public UiHistoryItem GetOldestItem()
  {
    var items = m_RectTransform.GetComponentsInChildren<UiHistoryItem>();
    var returnIndex = items.Length - 1;

    return returnIndex >= 0 ? items[items.Length - 1] : null;
  }


  public UiHistoryItem GetItemByFullPath(string fullPath)
  {
    var items = m_RectTransform.GetComponentsInChildren<UiHistoryItem>();

    foreach (var item in items)
    {
      if (item.m_FullPath == fullPath)
        return item;
    }

    return null;
  }


  public void MoveToTop(Transform item)
  {
    if (item.parent != m_RectTransform)
      return;

    item.SetAsFirstSibling();
    AssignColors();
  }
}
