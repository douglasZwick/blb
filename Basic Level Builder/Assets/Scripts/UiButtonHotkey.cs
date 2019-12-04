using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UiButtonHotkey : MonoBehaviour
{
  [System.Serializable]
  public class Hotkey
  {
    public string m_Button;
    public ModifierRequirements m_Modifiers;
    public UnityEvent m_Event;
    public bool m_SubjectToHotkeyDisabling;
    public bool m_AllowedInPlayMode;
  }

  [System.Serializable]
  public enum ModifierRequirements
  {
    None,
    Primary,
    Secondary,
    Paired,
  }

  public List<Hotkey> m_List = new List<Hotkey>();


  void Update()
  {
    foreach (var hotkey in m_List)
    {
      if (hotkey.m_SubjectToHotkeyDisabling && !HotkeyMaster.s_HotkeysEnabled)
        return;

      if (!hotkey.m_AllowedInPlayMode && GlobalData.IsInPlayMode())
        return;

      switch (hotkey.m_Modifiers)
      {
        case ModifierRequirements.Primary:
          if (HotkeyMaster.IsPrimaryModifierHeld())
            goto default;
          break;
        case ModifierRequirements.Secondary:
          if (HotkeyMaster.IsSecondaryModifierHeld())
            goto default;
          break;
        case ModifierRequirements.Paired:
          if (HotkeyMaster.IsPairedModifierHeld())
            goto default;
          break;
        default:  // ModifierRequirements.None
          if (Input.GetButtonDown(hotkey.m_Button))
            hotkey.m_Event.Invoke();
          break;
      }
    }
  }
}
