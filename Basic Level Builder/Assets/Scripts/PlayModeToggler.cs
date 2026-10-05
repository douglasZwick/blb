/***************************************************
File:           UIPlayMode.cs
Authors:        Christopher Onorati
Last Updated:   6/17/2019
Last Version:   2019.1.4

Description:
  Toggles the play mode, buddy

Copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using UnityEngine;
using UnityEngine.InputSystem;

public class PlayModeToggler : MonoBehaviour
{
  /************************************************************************************/

  /**
  * FUNCTION NAME: Toggle
  * DESCRIPTION  : Toggle between play modes.
  * INPUTS       : None
  * OUTPUTS      : None
  **/
  
  private void OnEnable()
  {
    InputSystem.actions["Cancel"].performed += OnCancel;
  }

  private void OnDisable()
  {
    InputSystem.actions["Cancel"].performed -= OnCancel;
  }

  public void Toggle()
  {
    GlobalData.TogglePlayMode();
  }

  private void OnCancel(InputAction.CallbackContext context)
  {
    if (GlobalData.IsInPlayMode())
      GlobalData.DisablePlayMode();
  }
}
