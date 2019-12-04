/***************************************************
File:           FalseSolidTileLogic.cs
Authors:        Christopher Onorati
Last Updated:   6/6/2019
Last Version:   2019.1.4

Description:
  Logic for the false solid to switch alpha on play mode.

Copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FalseSolidTileLogic : MonoBehaviour
{
  /************************************************************************************/

  public Sprite m_PlayModeSprite;

  //Game object's sprite renderer.
  SpriteRenderer m_cSpriteRenderer;

  Sprite m_EditModeSprite;

  /**
  * FUNCTION NAME: Start
  * DESCRIPTION  : Caches components.
  * INPUTS       : None
  * OUTPUTS      : None
  **/
  void Start()
  {
    m_cSpriteRenderer = GetComponent<SpriteRenderer>();
    m_EditModeSprite = m_cSpriteRenderer.sprite;

    GlobalData.PlayModeToggled += OnPlayModeToggled;
  }


  /**
  * FUNCTION NAME: OnPlayModeToggled
  * DESCRIPTION  : Set alpha of sprite based on play mode.
  * INPUTS       : _IsInPlayMode - True/false flag for the playmode state of the game.
  * OUTPUTS      : None
  **/
  void OnPlayModeToggled(bool _IsInPlayMode)
  {
    m_cSpriteRenderer.sprite = _IsInPlayMode ? m_PlayModeSprite : m_EditModeSprite;
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
