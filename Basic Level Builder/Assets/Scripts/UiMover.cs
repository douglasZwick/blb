using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UiMover : MonoBehaviour
{
  public float m_HiddenX = 0;
  public float m_ShownX = 0;
  public float m_Duration = 0.5f;
  public bool m_Hidden = false;

  RectTransform m_RectTransform;


  void Start()
  {
    m_RectTransform = GetComponent<RectTransform>();

    if (m_Hidden)
    {
      var anchoredPosition = m_RectTransform.anchoredPosition;
      anchoredPosition.x = m_HiddenX;
      m_RectTransform.anchoredPosition = anchoredPosition;
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
    ActionMaster.Actions.MoveRTX(gameObject, m_ShownX, m_Duration, new Ease(Ease.Quad.InOut));
  }


  void Hide()
  {
    m_Hidden = true;

    Cancel();
    ActionMaster.Actions.MoveRTX(gameObject, m_HiddenX, m_Duration, new Ease(Ease.Quad.InOut));
  }


  void Cancel()
  {
    ActionMaster.Actions.CancelAllInTreeRegarding(gameObject);
  }
}
