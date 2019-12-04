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

public class PlayModeToggler : MonoBehaviour
{
  /************************************************************************************/

  /**
  * FUNCTION NAME: Toggle
  * DESCRIPTION  : Toggle between play modes.
  * INPUTS       : None
  * OUTPUTS      : None
  **/
  public void Toggle()
  {
    GlobalData.TogglePlayMode();
  }
}
