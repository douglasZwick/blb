using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class KeysDisplay : MonoBehaviour
{
  public GameObject m_IconPrefab;

  RectTransform m_RectTransform;
  List<TileColor> m_CollectedKeys = new List<TileColor>();


  private void Awake()
  {
    m_RectTransform = GetComponent<RectTransform>();

    GlobalData.PlayModeToggled += OnPlayModeToggled;
    GlobalData.KeyCollected += OnKeyCollected;
  }


  void OnPlayModeToggled(bool isInPlayMode)
  {
    if (isInPlayMode)
      Clear();
  }


  public bool Has(TileColor keyColor)
  {
    return m_CollectedKeys.Contains(keyColor);
  }


  void Clear()
  {
    foreach (Transform item in m_RectTransform)
      Destroy(item.gameObject);

    m_CollectedKeys = new List<TileColor>();
  }


  void OnKeyCollected(TileColor keyColor)
  {
    if (m_CollectedKeys.Contains(keyColor))
      return;

    var colorAsInt = (int)keyColor;

    var insertionIndex = 0;

    for (var i = 0; i < m_CollectedKeys.Count; ++i)
    {
      var oldKeyColor = m_CollectedKeys[i];
      var oldKeyColorAsInt = (int)oldKeyColor;

      if (colorAsInt > oldKeyColorAsInt)
        ++insertionIndex;
      else
        break;
    }

    m_CollectedKeys.Insert(insertionIndex, keyColor);

    var icon = Instantiate(m_IconPrefab);
    var iconRT = icon.GetComponent<RectTransform>();
    iconRT.SetParent(m_RectTransform);
    iconRT.SetSiblingIndex(insertionIndex);
    var iconColorCode = icon.GetComponent<ColorCode>();
    iconColorCode.Set(keyColor);
  }


  private void OnDestroy()
  {
    GlobalData.PlayModeToggled -= OnPlayModeToggled;
    GlobalData.KeyCollected -= OnKeyCollected;
  }
}
