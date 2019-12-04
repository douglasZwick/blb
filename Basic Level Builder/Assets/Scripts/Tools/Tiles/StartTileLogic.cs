/***************************************************
File:           StartTileLogic.cs
Authors:        Christopher Onorati
Last Updated:   6/5/2019
Last Version:   2019.1.4

Description:
  Logic for the start tiles to spawn the Hero in play
  mode.

Copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(TileDirection))]
public class StartTileLogic : MonoBehaviour
{
  /************************************************************************************/

  //Keep track of the player this tile created.
  GameObject m_Hero;

  /************************************************************************************/

  public GameObject m_HeroPrefab;

  //Game object's transform.
  Transform m_cTransform;
  TileDirection m_TileDirection;

  //Game object's sprite renderer.
  public List<SpriteRenderer> m_SpriteRenderers;
  List<float> m_EditModeOpacities = new List<float>();

  public float m_PlayModeOpacity = 0.25f;

  /**
  * FUNCTION NAME: Awake
  * DESCRIPTION  : Caches components.
  * INPUTS       : None
  * OUTPUTS      : None
  **/
  void Awake()
  {
    m_cTransform = GetComponent<Transform>();
    m_TileDirection = GetComponent<TileDirection>();

    foreach (var renderer in m_SpriteRenderers)
      m_EditModeOpacities.Add(renderer.color.a);

    GlobalData.PlayModePreToggle += OnPlayModePreToggle;
    GlobalData.PlayModeToggled += OnPlayModeToggled;
  }


  void OnPlayModePreToggle(bool isInPlayMode)
  {
    if (isInPlayMode)
    {
      m_Hero = Instantiate(m_HeroPrefab, m_cTransform.position, Quaternion.identity);

      var heroController = m_Hero.GetComponent<HeroController>();
      heroController?.SetDirection(m_TileDirection.m_Direction);
      var tileDirection = m_Hero.GetComponent<TileDirection>();
      tileDirection?.Initialize(m_TileDirection.m_Direction);

      var ghostMaker = m_Hero.GetComponent<GhostMaker>();
      if (ghostMaker != null)
        ghostMaker.enabled = GhostToggler.s_IsInGhostMode;
    }
  }


  /**
  * FUNCTION NAME: OnPlayModeToggled
  * DESCRIPTION  : Spawns/removes the player.
  * INPUTS       : _IsInPlayMode - True/false flag for the playmode state of the game.
  * OUTPUTS      : None
  **/
  void OnPlayModeToggled(bool isInPlayMode)
  {
    if (isInPlayMode)
    {
      for (var i = 0; i < m_SpriteRenderers.Count; ++i)
      {
        var sprite = m_SpriteRenderers[i];
        var editModeOpacity = m_EditModeOpacities[i];
        var color = sprite.color;
        var playModeOpacity = editModeOpacity * m_PlayModeOpacity;
        color.a = playModeOpacity;
        sprite.color = color;
      }
    }
    else
    {
      for (var i = 0; i < m_SpriteRenderers.Count; ++i)
      {
        var sprite = m_SpriteRenderers[i];
        var editModeOpacity = m_EditModeOpacities[i];
        var color = sprite.color;
        color.a = editModeOpacity;
        sprite.color = color;
      }

      if (m_Hero)
        Destroy(m_Hero);
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
    GlobalData.PlayModePreToggle -= OnPlayModePreToggle;
    GlobalData.PlayModeToggled -= OnPlayModeToggled;
  }
}
