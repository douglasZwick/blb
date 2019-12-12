/***************************************************
File:           DoorTileLogic.cs
Authors:        Christopher Onorati
Last Updated:   6/14/2019
Last Version:   2019.1.4

Description:
  Logic for the tile that spawns doors.

Copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using UnityEngine;
using TMPro;  //Text

[RequireComponent(typeof(SpriteRenderer))]
public class DoorTileLogic : MonoBehaviour
{
  /************************************************************************************/

  //Door game object created by the tile.
  GameObject m_pCreatedDoor;

  /************************************************************************************/

  Transform m_cTransform;
  ColorCode m_cColorCode;

  public TextMeshPro m_Text;
  public GameObject m_DoorPrefab;

  /**
  * FUNCTION NAME: Start
  * DESCRIPTION  : Caches components.
  * INPUTS       : None
  * OUTPUTS      : None
  **/
  void Start()
  {
    m_cTransform = GetComponent<Transform>();
    m_cColorCode = GetComponent<ColorCode>();

    GlobalData.PlayModePreToggle += OnPlayModePreToggle;
    GlobalData.PlayModeToggled += OnPlayModeToggled;
  }


  void OnPlayModePreToggle(bool isInPlayMode)
  {
    if (isInPlayMode)
    {
      var parent = m_cTransform.parent;
      m_pCreatedDoor = Instantiate(m_DoorPrefab, m_cTransform.position, Quaternion.identity, parent);
      var createdColorCode = m_pCreatedDoor.GetComponent<ColorCode>();
      createdColorCode.Set(m_cColorCode.m_TileColor);

      var thisPathMover = GetComponent<PathMover>();
      if (thisPathMover != null)
      {
        var thatPathMover = m_pCreatedDoor.AddComponent<PathMover>();
        thatPathMover.Setup(thisPathMover.m_IndexList);
        var thatRigidbody2D = m_pCreatedDoor.AddComponent<Rigidbody2D>();
        thatRigidbody2D.isKinematic = true;
        m_pCreatedDoor.AddComponent<ContactParent>();
      }
    }
    else
    {
      if (m_pCreatedDoor != null)
        Destroy(m_pCreatedDoor);
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
  * FUNCTION NAME: GetDoorColor
  * DESCRIPTION  : Return the color of this tile to the requester.
  * INPUTS       : None
  * OUTPUTS      : TileColor - Color of the door.
  **/
  public TileColor GetDoorColor()
  {
    return m_cColorCode.m_TileColor;
  }

  /**
  * FUNCTION NAME: GetCreatedDoor
  * DESCRIPTION  : Return the door created by the tile.
  * INPUTS       : None
  * OUTPUTS      : GameObject - Door created by the tile for in game.
  **/
  public GameObject GetCreatedDoor()
  {
    return m_pCreatedDoor;
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
