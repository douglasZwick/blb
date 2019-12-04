/***************************************************
File:           PlayModeToggleHotkey.cs
Authors:        Christopher Onorati
Last Updated:   5/31/2019
Last Version:   2019.1.4

Description:
  Simple hotkey switch for playmode vs not play mode.

Copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using UnityEngine;

/**
* CLASS NAME  : PlayModeToggleHotkey
* DESCRIPTION : Toggle the playmode via hotkey.  Probably have a button on the UI too.
**/
public class PlayModeToggleHotkey : MonoBehaviour
{
  /**
  * FUNCTION NAME: Update
  * DESCRIPTION  : Toggles playmode state based on a hotkey. input.
  * INPUTS       : None
  * OUTPUTS      : None
  **/
  void Update()
  {
    if (HotkeyMaster.s_HotkeysEnabled && Input.GetButtonDown("Play"))
    {
      GlobalData.TogglePlayMode();
    }
  }
}
