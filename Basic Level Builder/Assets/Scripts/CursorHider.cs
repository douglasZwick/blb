/***************************************************
Authors:        Brenden Epp
Last Updated:   2/14/2026

Copyright 2018-2026, DigiPen Institute of Technology
***************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CursorHider : MonoBehaviour
{
  [SerializeField]
  private Renderer[] m_Renderers;

  [SerializeField]
  private GameObject m_BackGround;

  private PointerEventData m_PointerData;
  private List<RaycastResult> m_Results = new();

  void Update()
  {
    if (EventSystem.current == null)
      return;

    m_PointerData = new(EventSystem.current);
    m_PointerData.position = Input.mousePosition;

    m_Results.Clear();
    EventSystem.current.RaycastAll(m_PointerData, m_Results);

    bool overButton = false;

    foreach (var result in m_Results)
    {
      //print(result);
      // If we are raycasting the background, then there is no ui above the bg
      if (result.gameObject != m_BackGround)
      {
        overButton = true;
        break;
      }
    }

    SetVisible(!overButton);
  }

  private void SetVisible(bool visible)
  {
    foreach (var r in m_Renderers)
    {
      r.gameObject.SetActive(visible);
    }
  }
}
