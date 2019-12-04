/***************************************************
File:           InvisibleSolidTileLogic.cs
Authors:        Christopher Onorati
Last Updated:   6/6/2019
Last Version:   2019.1.4

Description:
  Logic for invisible solids to switch alpha on play mode.

Copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class InvisibleSolidTileLogic : MonoBehaviour
{
  /************************************************************************************/

  //Game object's sprite renderer.
  SpriteRenderer m_cSpriteRenderer;

  /**
  * FUNCTION NAME: Start
  * DESCRIPTION  : Caches components.
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
  * DESCRIPTION  : Swaps sprite visibility.
  * INPUTS       : _IsInPlayMode - True/false flag for the playmode state of the game.
  * OUTPUTS      : None
  **/
  void OnPlayModeToggled(bool _IsInPlayMode)
  {
    if (!_IsInPlayMode)
      m_cSpriteRenderer.enabled = true;

    else
      m_cSpriteRenderer.enabled = false;
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
