/***************************************************
File:           BrushTool.cs
Authors:        Christopher Onorati
Last Updated:   6/19/2019
Last Version:   2019.1.4

Description:
  Simple script to destroy a game object via UI events.

Copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using UnityEngine;

public class UIDestroyGameObject : MonoBehaviour
{
  /**
  * FUNCTION NAME: Destroy
  * DESCRIPTION  : Destroy the owner game object.
  * INPUTS       : None
  * OUTPUTS      : None
  **/
  public void Destroy()
  {
    Destroy(gameObject);
  }
}
