using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UITooltipMaker))]
public class ChangeTooltipOnGhostModeToggled : MonoBehaviour
{
  public string m_GhostModeEnabledTooltipText;

  UITooltipMaker m_TooltipMaker;
  string m_GhostModeDisabledTooltipText;


  private void Awake()
  {
    m_TooltipMaker = GetComponent<UITooltipMaker>();
    m_GhostModeDisabledTooltipText = m_TooltipMaker.m_Text;

    GlobalData.GhostModeEnabled += OnGhostModeEnabled;
    GlobalData.GhostModeDisabled += OnGhostModeDisabled;
  }


  void OnGhostModeEnabled()
  {
    m_TooltipMaker.UpdateText(m_GhostModeEnabledTooltipText);
  }


  void OnGhostModeDisabled()
  {
    m_TooltipMaker.UpdateText(m_GhostModeDisabledTooltipText);
  }


  private void OnDestroy()
  {
    GlobalData.GhostModeEnabled -= OnGhostModeEnabled;
    GlobalData.GhostModeDisabled -= OnGhostModeDisabled;
  }
}
