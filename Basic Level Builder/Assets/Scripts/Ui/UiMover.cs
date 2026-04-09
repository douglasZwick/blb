using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UiMover : MonoBehaviour
{
  public bool m_MoveX = true;

  [HideIf("m_MoveX", hideWhenTrue: false)]
  public float m_HiddenX = 0;
  [HideIf("m_MoveX", hideWhenTrue: false)]
  public float m_ShownX = 0;
  
  public bool m_MoveY = false;

  [HideIf("m_MoveY", hideWhenTrue: false)]
  public float m_HiddenY = 0;
  [HideIf("m_MoveY", hideWhenTrue: false)]
  public float m_ShownY = 0;

  public float m_Duration = 0.5f;
  public bool m_Hidden = false;

  RectTransform m_RectTransform;


  void Start()
  {
    m_RectTransform = GetComponent<RectTransform>();

    if (m_Hidden)
    {
      var anchoredPosition = m_RectTransform.anchoredPosition;
      
      if (m_MoveX)
        anchoredPosition.x = m_HiddenX;
      if (m_MoveY)
        anchoredPosition.y = m_HiddenY;

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
    if (m_MoveX)
      ActionMaster.Actions.MoveRTX(gameObject, m_ShownX, m_Duration, new Ease(Ease.Quad.InOut));
    if (m_MoveY)
      ActionMaster.Actions.MoveRTY(gameObject, m_ShownY, m_Duration, new Ease(Ease.Quad.InOut));
  }


  void Hide()
  {
    m_Hidden = true;

    Cancel();
    if (m_MoveX)
      ActionMaster.Actions.MoveRTX(gameObject, m_HiddenX, m_Duration, new Ease(Ease.Quad.InOut));
    if (m_MoveY)
      ActionMaster.Actions.MoveRTY(gameObject, m_HiddenY, m_Duration, new Ease(Ease.Quad.InOut));
  }


  void Cancel()
  {
    ActionMaster.Actions.CancelAllInTreeRegarding(gameObject);
  }
}
