/***************************************************
Authors:        Douglas Zwick
Last Updated:   ???

Copyright 2018-2025, DigiPen Institute of Technology
***************************************************/


using UnityEngine;

public class HotkeyMaster : MonoBehaviour
{
  public static bool s_HotkeysEnabled = true;

  public static bool IsMultiSelectHeld()
  {
    return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand);
  }

  public static bool IsRangeSelectHeld()
  {
    return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
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
    var altHeld = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);

    if (Application.isEditor || Application.platform == RuntimePlatform.WebGLPlayer)
      return altHeld;

    return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftCommand) || Input.GetKey(KeyCode.RightCommand);
  }

  private static bool IsSecondaryModifierHeldEx()
  {
    return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
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
