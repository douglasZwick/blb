/***************************************************
File:           HideOnPlay.cs
Authors:        Christopher Onorati
Last Updated:   6/10/2019
Last Version:   2019.1.4

Description:
  Utility script to hide game objects while in play
  mode.

Copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using UnityEngine;

/**
* CLASS NAME  : HideOnPlay
* DESCRIPTION : Class used to make game objects dissapear while in play mode.
**/
public class HideOnPlay : MonoBehaviour
{
  /************************************************************************************/

  //SpriteRenderer of the game object.
  SpriteRenderer m_cSpriteRenderer;

  /**
  * FUNCTION NAME: Start
  * DESCRIPTION  : Latch onto playmode event.
  * INPUTS       : None
  * OUTPUTS      : None
  **/
  void Start()
  {
    m_cSpriteRenderer = GetComponent<SpriteRenderer>();

    GlobalData.PlayModeToggled += OnPlayModeToggled;
  }

  /**
  * FUNCTION NAME: OnPlayModeToggled
  * DESCRIPTION  : Hides the sprite while in playmode.
  * INPUTS       : _IsInPlayMode - True/false flag for the playmode state of the game.
  * OUTPUTS      : None
  **/
  void OnPlayModeToggled(bool _IsInPlayMode)
  {
    //Show the sprite.
    if (!_IsInPlayMode)
    {
      if (m_cSpriteRenderer)
        m_cSpriteRenderer.enabled = true;
    }

    //Hide the sprite.
    else
    {
      if (m_cSpriteRenderer)
        m_cSpriteRenderer.enabled = false;
    }
  }

  /**
  * FUNCTION NAME: OnDestroy
  * DESCRIPTION  : Remove event connection.
  * INPUTS       : None
  * OUTPUTS      : None
  **/
  private void OnDestroy()
  {
    GlobalData.PlayModeToggled -= OnPlayModeToggled;
  }
}
