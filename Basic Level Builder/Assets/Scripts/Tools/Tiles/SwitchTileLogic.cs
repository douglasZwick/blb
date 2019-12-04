/***************************************************
File:           SwitchTileLogic.cs
Authors:        Christopher Onorati
Last Updated:   6/10/2019
Last Version:   2019.1.4

Description:
  Logic for the tile that spawns switches.

Copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using UnityEngine;

[RequireComponent(typeof(ColorCode))]
public class SwitchTileLogic : MonoBehaviour
{
  /************************************************************************************/

  //Switch game object created by the tile.
  GameObject m_CreatedSwitch;

  /************************************************************************************/

  //Game object's transform.
  Transform m_Transform;

  //Game object's sprite renderers.
  ColorCode m_ColorCode;
  SetComponentColorsOnColorSet m_ComponentColors;

  public GameObject m_SwitchPrefab;


  /**
  * FUNCTION NAME: Start
  * DESCRIPTION  : Caches components.
  * INPUTS       : None
  * OUTPUTS      : None
  **/
  void Start()
  {
    m_Transform = transform;
    m_ColorCode = GetComponent<ColorCode>();
    m_ComponentColors = GetComponent<SetComponentColorsOnColorSet>();

    GlobalData.PlayModePreToggle += OnPlayModePreToggle;
    GlobalData.PlayModeToggled += OnPlayModeToggled;
  }


  void OnPlayModePreToggle(bool isInPlayMode)
  {
    if (isInPlayMode)
    {
      m_CreatedSwitch = Instantiate(m_SwitchPrefab, m_Transform.position, Quaternion.identity);
      var createdColorCode = m_CreatedSwitch.GetComponent<ColorCode>();
      createdColorCode.Set(m_ColorCode.m_TileColor);
    }
    else
    {
      if (m_CreatedSwitch)
        Destroy(m_CreatedSwitch);
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
    foreach (var spriteRenderer in m_ComponentColors.m_SpritesToColor)
      spriteRenderer.enabled = !isInPlayMode;
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
