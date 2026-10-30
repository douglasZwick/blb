/***************************************************
Authors:        Brenden Epp
Last Updated:   9/30/2026

Copyright 2018-2026, DigiPen Institute of Technology
***************************************************/


using UnityEngine;
using UnityEngine.InputSystem;

public class HotkeyMaster : MonoBehaviour
{
  public static bool s_HotkeysEnabled = true;

  public static bool IsMultiSelectHeld()
  {
    return IsPrimaryModifierHeld();
  }

  public static bool IsRangeSelectHeld()
  {
    return IsSecondaryModifierHeld();
  }

  public static bool IsPrimaryModifierHeld()
  {
    return IsPrimaryModifierHeldEx() && !IsSecondaryModifierHeldEx();
  }

  public static bool IsSecondaryModifierHeld()
  {
    return !IsPrimaryModifierHeldEx() && IsSecondaryModifierHeldEx();
  }

  private static bool IsPrimaryModifierHeldEx()
  {
    return InputSystem.actions["Primary Modifier"].ReadValue<float>() > 0;
  }

  private static bool IsSecondaryModifierHeldEx()
  {
    return InputSystem.actions["Secondary Modifier"].ReadValue<float>() > 0;
  }

  public static bool IsPairedModifierHeld()
  {
    return IsSecondaryModifierHeldEx() && IsPrimaryModifierHeldEx();
  }

  public static bool IsAnyModifierHeld()
  {
    return IsSecondaryModifierHeldEx() || IsPrimaryModifierHeldEx();
  }
}
