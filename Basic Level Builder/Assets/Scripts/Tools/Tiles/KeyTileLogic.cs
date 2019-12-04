/***************************************************
File:           KeyTileLogic.cs
Authors:        Christopher Onorati
Last Updated:   6/14/2019
Last Version:   2019.1.4

Description:
  Logic for the tile that spawns keys.

Copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using UnityEngine;
using TMPro;  //Text

[RequireComponent(typeof(SpriteRenderer))]
public class KeyTileLogic : MonoBehaviour
{
  /************************************************************************************/

  //Key game object created by the tile.
  GameObject m_pCreatedKey;

  /************************************************************************************/

  Transform m_cTransform;
  SpriteRenderer m_cSpriteRenderer;
  ColorCode m_cColorCode;
  GameObject m_pText;

  public GameObject m_KeyPrefab;

  /**
  * FUNCTION NAME: Start
  * DESCRIPTION  : Caches components.
  * INPUTS       : None
  * OUTPUTS      : None
  **/
  void Start()
  {
    m_cTransform = GetComponent<Transform>();
    m_cSpriteRenderer = GetComponent<SpriteRenderer>();
    m_cColorCode = GetComponent<ColorCode>();
    m_pText = m_cTransform.GetChild(0).gameObject;

    GlobalData.PlayModePreToggle += OnPlayModePreToggle;
    GlobalData.PlayModeToggled += OnPlayModeToggled;
  }


  void OnPlayModePreToggle(bool isInPlayMode)
  {
    if (isInPlayMode)
    {
      var parent = m_cTransform.parent;
      m_pCreatedKey = Instantiate(m_KeyPrefab, m_cTransform.position, Quaternion.identity, parent);
      m_pCreatedKey.GetComponent<ColorCode>().Set(m_cColorCode.m_TileColor);
    }
    else
    {
      if (m_pCreatedKey)
        Destroy(m_pCreatedKey);
    }
  }


  /**
  * FUNCTION NAME: OnPlayModeToggled
  * DESCRIPTION  : Swaps sprite visibility.
  * INPUTS       : _IsInPlayMode - True/false flag for the playmode state of the game.
  * OUTPUTS      : None
  **/
  void OnPlayModeToggled(bool isInPlayMode)
  {
    gameObject.SetActive(!isInPlayMode);
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
