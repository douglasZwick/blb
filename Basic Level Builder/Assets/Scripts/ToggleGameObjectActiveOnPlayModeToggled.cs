using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleGameObjectActiveOnPlayModeToggled : MonoBehaviour
{
  public bool m_StartActive = true;


  private void Awake()
  {
    GlobalData.PlayModeToggled += OnPlayModeToggled;
  }


  private void Start()
  {
    gameObject.SetActive(m_StartActive);
  }


  void OnPlayModeToggled(bool isInPlayMode)
  {
    gameObject.SetActive(isInPlayMode ^ m_StartActive);
  }


  private void OnDestroy()
  {
    GlobalData.PlayModeToggled -= OnPlayModeToggled;
  }
}
