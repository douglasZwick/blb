using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ChangeClearColorOnPlayModeToggled : MonoBehaviour
{
  public Color m_PlayModeColor;

  Camera m_Camera;
  Color m_EditModeColor;


  private void Awake()
  {
    m_Camera = GetComponent<Camera>();
    m_EditModeColor = m_Camera.backgroundColor;

    GlobalData.PlayModeToggled += OnPlayModeToggled;
  }


  void OnPlayModeToggled(bool isInPlayMode)
  {
    m_Camera.backgroundColor = isInPlayMode ? m_PlayModeColor : m_EditModeColor;
  }


  private void OnDestroy()
  {
    GlobalData.PlayModeToggled -= OnPlayModeToggled;
  }
}
