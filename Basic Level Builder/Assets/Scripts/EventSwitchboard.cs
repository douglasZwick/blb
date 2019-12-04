using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventSwitchboard : MonoBehaviour
{
  public UnityEvent m_HeroDied;
  public UnityEvent m_PlayModePreStarted;
  public UnityEvent m_PlayModeStarted;
  public UnityEvent m_EditModePreStarted;
  public UnityEvent m_EditModeStarted;


  private void Awake()
  {
    GlobalData.HeroDied += OnHeroDied;
    GlobalData.PlayModePreToggle += OnPlayModePreToggle;
    GlobalData.PlayModeToggled += OnPlayModeToggled;
  }


  void OnHeroDied()
  {
    m_HeroDied.Invoke();
  }


  void OnPlayModePreToggle(bool isInPlayMode)
  {
    (isInPlayMode ? m_PlayModePreStarted : m_EditModePreStarted).Invoke();
  }


  void OnPlayModeToggled(bool isInPlayMode)
  {
    (isInPlayMode ? m_PlayModeStarted : m_EditModeStarted).Invoke();
  }


  private void OnDestroy()
  {
    GlobalData.HeroDied -= OnHeroDied;
    GlobalData.PlayModePreToggle -= OnPlayModePreToggle;
    GlobalData.PlayModeToggled -= OnPlayModeToggled;
  }
}
