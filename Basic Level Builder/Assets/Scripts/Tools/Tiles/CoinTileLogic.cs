/***************************************************
File:           CoinTileLogic.cs
Authors:        Christopher Onorati
Last Updated:   6/6/2019
Last Version:   2019.1.4

Description:
  Logic for invisible solids to switch alpha on play mode.

Copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class CoinTileLogic : MonoBehaviour
{
  /************************************************************************************/

  GameObject m_pCreatedCoin;

  /************************************************************************************/

  //Game object's transform.
  Transform m_cTransform;

  //Game object's sprite renderer.
  SpriteRenderer m_cSpriteRenderer;

  public GameObject m_CoinPrefab;

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

    GlobalData.PlayModePreToggle += OnPlayModePreToggle;
    GlobalData.HeroReturned += OnHeroReturned;
  }


  /**
  * FUNCTION NAME: OnPlayModeToggled
  * DESCRIPTION  : Swaps sprite visibility.
  * INPUTS       : _IsInPlayMode - True/false flag for the playmode state of the game.
  * OUTPUTS      : None
  **/
  void OnPlayModePreToggle(bool isInPlayMode)
  {
    if (isInPlayMode)
    {
      m_cSpriteRenderer.enabled = false;

      CreateCoin();
    }
    else
    {
      m_cSpriteRenderer.enabled = true;

      CleanUpCoin();
    }
  }


  void OnHeroReturned()
  {
    if (m_pCreatedCoin == null)
      CreateCoin();
  }


  void CreateCoin()
  {
    var parent = m_cTransform.parent;
    m_pCreatedCoin = Instantiate(m_CoinPrefab, m_cTransform.position, Quaternion.identity, parent);
  }


  void CleanUpCoin()
  {
    if (m_pCreatedCoin)
      Destroy(m_pCreatedCoin);
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
    GlobalData.HeroReturned -= OnHeroReturned;
  }
}
