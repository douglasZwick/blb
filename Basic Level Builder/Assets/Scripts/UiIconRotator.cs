using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UiIconRotator : MonoBehaviour
{
  public float m_ShownRotation;
  public float m_HiddenRotation;
  public float m_Duration = 2;
  public bool m_Hidden = false;

  RectTransform m_RectTransform;


  void Start()
  {
    m_RectTransform = GetComponent<RectTransform>();

    if (m_Hidden)
    {
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
    ActionMaster.Actions.Rotate2DRT(gameObject, m_ShownRotation, m_Duration, new Ease(Ease.Elastic.Out));
  }


  void Hide()
  {
    m_Hidden = true;

    Cancel();
    ActionMaster.Actions.Rotate2DRT(gameObject, m_HiddenRotation, m_Duration, new Ease(Ease.Elastic.Out));
  }


  void Cancel()
  {
    ActionMaster.Actions.CancelAllInTreeRegarding(gameObject);
  }
}
