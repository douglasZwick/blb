using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Checkpoint : MonoBehaviour
{
  [HideInInspector]
  public Transform m_Transform;
  public SpriteRenderer m_Flag;
  public UnityEvent WasActivated;

  bool m_Active = false;
  Color m_DefaultColor;


  private void Awake()
  {
    m_Transform = transform;
    m_DefaultColor = m_Flag.color;

    GlobalData.PlayModeToggled += OnPlayModeToggled;
  }


  void OnPlayModeToggled(bool isInPlayMode)
  {
    if (!isInPlayMode)
      AttemptDeactivate();
  }


  public void AttemptActivate(Color flagColor)
  {
    if (m_Active)
      return;

    Activate(flagColor);
  }


  void Activate(Color flagColor)
  {
    m_Active = true;

    WasActivated.Invoke();
    m_Flag.color = flagColor;
  }


  public void AttemptDeactivate()
  {
    if (!m_Active)
      return;

    Deactivate();
  }


  void Deactivate()
  {
    m_Active = false;

    m_Flag.color = m_DefaultColor;
  }


  private void OnDestroy()
  {
    GlobalData.PlayModeToggled -= OnPlayModeToggled;
  }
}
