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

public class CoinTileLogic : MonoBehaviour
{
  /************************************************************************************/

  GameObject m_CreatedCoin;

  /************************************************************************************/

  //Game object's transform.
  Transform m_Transform;

  //Game object's sprite renderer.
  public SpriteRenderer m_SpriteRenderer;
  public GameObject m_CoinPrefab;
  public float m_PlayModeAlpha = 0.25f;
  public float m_PlayModeSize = 0.25f;

  float m_EditModeAlpha;
  Vector3 m_EditModeScale;

  /**
  * FUNCTION NAME: Start
  * DESCRIPTION  : Caches components.
  * INPUTS       : None
  * OUTPUTS      : None
  **/
  void Start()
  {
    m_Transform = GetComponent<Transform>();

    m_EditModeAlpha = m_SpriteRenderer.color.a;
    m_EditModeScale = m_Transform.localScale;

    GlobalData.PlayModePreToggle += OnPlayModePreToggle;
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
      var color = m_SpriteRenderer.color;
      color.a = m_PlayModeAlpha;
      m_SpriteRenderer.color = color;

      m_Transform.localScale = Vector3.one * m_PlayModeSize;

      CreateCoin();
    }
    else
    {
      var color = m_SpriteRenderer.color;
      color.a = m_EditModeAlpha;
      m_SpriteRenderer.color = color;

      m_Transform.localScale = m_EditModeScale;

      CleanUpCoin();
    }
  }


  void CreateCoin()
  {
    var parent = m_Transform.parent;
    m_CreatedCoin = Instantiate(m_CoinPrefab, m_Transform.position, Quaternion.identity, parent);
  }


  void CleanUpCoin()
  {
    if (m_CreatedCoin)
      Destroy(m_CreatedCoin);
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
  }
}
