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
    var keyboard = Keyboard.current;
    if (keyboard == null)
      return false;

    return keyboard.ctrlKey.isPressed || keyboard.leftCommandKey.isPressed || keyboard.rightCommandKey.isPressed;
  }

  public static bool IsRangeSelectHeld()
  {
    var keyboard = Keyboard.current;
    if (keyboard == null)
      return false;

    return keyboard.shiftKey.isPressed;
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
    var keyboard = Keyboard.current;
    if (keyboard == null)
      return false;

    var altHeld = keyboard.altKey.isPressed;

    if (Application.isEditor || Application.platform == RuntimePlatform.WebGLPlayer)
      return altHeld;

    return keyboard.ctrlKey.isPressed || keyboard.leftCommandKey.isPressed || keyboard.rightCommandKey.isPressed;
  }

  private static bool IsSecondaryModifierHeldEx()
  {
    var keyboard = Keyboard.current;
    if (keyboard == null)
      return false;

    return keyboard.shiftKey.isPressed;
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
