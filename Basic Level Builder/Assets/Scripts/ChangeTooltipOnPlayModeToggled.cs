using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UITooltipMaker))]
public class ChangeTooltipOnPlayModeToggled : MonoBehaviour
{
  public string m_PlayModeTooltipText;

  UITooltipMaker m_TooltipMaker;
  string m_EditModeTooltipText;


  private void Awake()
  {
    m_TooltipMaker = GetComponent<UITooltipMaker>();
    m_EditModeTooltipText = m_TooltipMaker.m_Text;

    GlobalData.PlayModeToggled += OnPlayModeToggled;
  }


  void OnPlayModeToggled(bool isInPlayMode)
  {
    var newText = isInPlayMode ? m_PlayModeTooltipText : m_EditModeTooltipText;
    m_TooltipMaker.UpdateText(newText);
  }


  private void OnDestroy()
  {
    GlobalData.PlayModeToggled -= OnPlayModeToggled;
  }
}
