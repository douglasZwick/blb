using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleComponentEnabledStatesOnPlayModeToggled : MonoBehaviour
{
  public List<Behaviour> m_Components;

  List<bool> m_EnabledInEditMode = new List<bool>();


  private void Awake()
  {
    foreach (var component in m_Components)
      m_EnabledInEditMode.Add(component.enabled);

    GlobalData.PlayModeToggled += OnPlayModeToggled;
  }


  void OnPlayModeToggled(bool isInPlayMode)
  {
    for (var i = 0; i < m_Components.Count; ++i)
      m_Components[i].enabled = isInPlayMode ^ m_EnabledInEditMode[i];
  }


  private void OnDestroy()
  {
    GlobalData.PlayModeToggled -= OnPlayModeToggled;
  }
}
