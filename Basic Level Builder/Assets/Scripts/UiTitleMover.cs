using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UiTitleMover : MonoBehaviour
{
  public Vector2 m_ShownPosition;
  public Vector2 m_HiddenPosition;
  public float m_ShownRotation;
  public float m_HiddenRotation;
  public float m_Duration = 1;
  public bool m_Hidden = false;

  RectTransform m_RectTransform;


  void Start()
  {
    m_RectTransform = GetComponent<RectTransform>();

    if (m_Hidden)
    {
      m_RectTransform.anchoredPosition = m_HiddenPosition;
      var eulerAngles = m_RectTransform.eulerAngles;
      eulerAngles.z = m_HiddenRotation;
      m_RectTransform.eulerAngles = eulerAngles;
    }
  }


  public void Toggle()
  {
    if (m_Hidden)
      Show();
    else
      Hide();
  }


  void Show()
  {
    m_Hidden = false;

    Cancel();
    ActionMaster.Actions.MoveRT(gameObject, m_ShownPosition, m_Duration, new Ease(Ease.Quad.InOut));
    ActionMaster.Actions.Rotate2DRT(gameObject, m_ShownRotation, m_Duration, new Ease(Ease.Quad.InOut));
  }


  void Hide()
  {
    m_Hidden = true;

    Cancel();
    ActionMaster.Actions.MoveRT(gameObject, m_HiddenPosition, m_Duration, new Ease(Ease.Quad.InOut));
    ActionMaster.Actions.Rotate2DRT(gameObject, m_HiddenRotation, m_Duration, new Ease(Ease.Quad.InOut));
  }


  void Cancel()
  {
    ActionMaster.Actions.CancelAllInTreeRegarding(gameObject);
  }
}
