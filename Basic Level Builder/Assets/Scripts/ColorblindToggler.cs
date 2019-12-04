using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorblindToggler : MonoBehaviour
{
  public static bool s_IsInColorblindMode { get; private set; }


  private void Update()
  {
    if (!HotkeyMaster.s_HotkeysEnabled)
      return;

    if (HotkeyMaster.IsPrimaryModifierHeld() && Input.GetKeyDown(KeyCode.I))
      ToggleColorblindMode();
  }


  void EnableColorblindMode()
  {
    s_IsInColorblindMode = true;

    GlobalData.DispatchColorblindModeEnabled();
  }


  void DisableColorblindMode()
  {
    s_IsInColorblindMode = false;

    GlobalData.DispatchColorblindModeDisabled();
  }


  public void ToggleColorblindMode()
  {
    if (s_IsInColorblindMode)
      DisableColorblindMode();
    else
      EnableColorblindMode();
  }
}
