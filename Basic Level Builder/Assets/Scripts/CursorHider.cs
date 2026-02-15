/***************************************************
Authors:        Brenden Epp
Last Updated:   2/14/2026

Copyright 2018-2026, DigiPen Institute of Technology
***************************************************/

using UnityEngine;
using UnityEngine.EventSystems;

public class CursorHider : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
  [SerializeField]
  private Renderer[] m_Renderers;

  // Called when pointer enters THIS UI element (background)
  public void OnPointerEnter(PointerEventData eventData)
  {
    SetVisible(true);
  }

  // Called when pointer exits THIS UI element
  public void OnPointerExit(PointerEventData eventData)
  {
    SetVisible(false);
  }

  private void SetVisible(bool visible)
  {
    foreach (var r in m_Renderers)
    {
      r.gameObject.SetActive(visible);
    }
  }
}
