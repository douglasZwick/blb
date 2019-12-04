using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UITooltipMaker))]
public class ChangeTooltipOnColorblindModeToggled : MonoBehaviour
{
  public string m_ColorblindModeEnabledTooltipText;

  UITooltipMaker m_TooltipMaker;
  string m_ColorblindModeDisabledTooltipText;


  private void Awake()
  {
    m_TooltipMaker = GetComponent<UITooltipMaker>();
    m_ColorblindModeDisabledTooltipText = m_TooltipMaker.m_Text;

    GlobalData.ColorblindModeEnabled += OnColorblindModeEnabled;
    GlobalData.ColorblindModeDisabled += OnColorblindModeDisabled;
  }


  void OnColorblindModeEnabled()
  {
    m_TooltipMaker.UpdateText(m_ColorblindModeEnabledTooltipText);
  }


  void OnColorblindModeDisabled()
  {
    m_TooltipMaker.UpdateText(m_ColorblindModeDisabledTooltipText);
  }


  private void OnDestroy()
  {
    GlobalData.ColorblindModeEnabled -= OnColorblindModeEnabled;
    GlobalData.ColorblindModeDisabled -= OnColorblindModeDisabled;
  }
}
