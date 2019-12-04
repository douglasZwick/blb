using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UiChangeIconAlphaOnColorblindModeToggle : MonoBehaviour
{
  public float m_ColorblindModeEnabledAlpha = 1;
  public float m_ColorblindModeDisabledAlpha = 0.5f;

  Image m_Image;


  private void Awake()
  {
    m_Image = GetComponent<Image>();

    GlobalData.ColorblindModeEnabled += OnColorblindModeEnabled;
    GlobalData.ColorblindModeDisabled += OnColorblindModeDisabled;
  }


  void OnColorblindModeEnabled()
  {
    var color = m_Image.color;
    color.a = m_ColorblindModeEnabledAlpha;
    m_Image.color = color;
  }


  void OnColorblindModeDisabled()
  {
    var color = m_Image.color;
    color.a = m_ColorblindModeDisabledAlpha;
    m_Image.color = color;
  }


  private void OnDestroy()
  {
    GlobalData.ColorblindModeEnabled -= OnColorblindModeEnabled;
    GlobalData.ColorblindModeDisabled -= OnColorblindModeDisabled;
  }
}
