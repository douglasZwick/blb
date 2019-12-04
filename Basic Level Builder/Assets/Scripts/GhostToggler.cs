using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostToggler : MonoBehaviour
{
  public static bool s_IsInGhostMode { get; private set; }


  private void Update()
  {
    if (!HotkeyMaster.s_HotkeysEnabled)
      return;

    if (HotkeyMaster.IsPrimaryModifierHeld() && Input.GetKeyDown(KeyCode.G))
      ToggleGhostMode();
  }


  void EnableGhostMode()
  {
    s_IsInGhostMode = true;

    GlobalData.DispatchGhostModeEnabled();
  }


  void DisableGhostMode()
  {
    s_IsInGhostMode = false;

    GlobalData.DispatchGhostModeDisabled();
  }


  public void ToggleGhostMode()
  {
    if (s_IsInGhostMode)
      DisableGhostMode();
    else
      EnableGhostMode();
  }
}
