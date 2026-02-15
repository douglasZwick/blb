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
  public Renderer[] renderers;

  private PointerEventData pointerData;
  private List<RaycastResult> results = new();

  void Update()
  {
    if (EventSystem.current == null)
      return;

    pointerData = new(EventSystem.current);
    pointerData.position = Input.mousePosition;

    results.Clear();
    EventSystem.current.RaycastAll(pointerData, results);

    bool overButton = false;

    foreach (var result in results)
    {
      if (result.gameObject != gameObject)
      {
        overButton = true;
        break;
      }
    }

    SetVisible(!overButton);
  }

  private void SetVisible(bool visible)
  {
    foreach (var r in renderers)
    {
      r.gameObject.SetActive(visible);
    }
  }
}
