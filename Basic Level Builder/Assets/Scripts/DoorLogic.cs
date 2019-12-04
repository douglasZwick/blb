/***************************************************
File:           DoorLogic.cs
Authors:        Christopher Onorati
Last Updated:   6/14/2019
Last Version:   2019.1.4

Description:
  Holds info on what color the door is.

Copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using UnityEngine;

public class DoorLogic : MonoBehaviour
{
  public SpriteRenderer m_OutlineSpriteRenderer;
  public SpriteRenderer m_BackgroundSpriteRenderer;
  public Collider2D m_Collider;
  public MeshRenderer m_TextMeshRenderer;
  public float m_FadeOpacity = 0.25f;
  public float m_FadeDuration = 0.25f;
  public float m_LinkOpenDelay = 0.1f;

  /************************************************************************************/

  // The main grid component, found on Start elsewhere in the scene, that manages all the tiles
  TileGrid m_TileGrid;

  //Linked doors
  DoorLogic m_LinkedDoorUp;
  DoorLogic m_LinkedDoorDown;
  DoorLogic m_LinkedDoorLeft;
  DoorLogic m_LinkedDoorRight;

  /************************************************************************************/

  Transform m_cTransform;
  ColorCode m_ColorCode;

  bool m_BeingOpenedByPropagation = false;
  bool m_BeingOpenedByKey = false;
  public TileColor m_Color { get; private set; }


  private void Awake()
  {
    m_ColorCode = GetComponent<ColorCode>();
    m_TileGrid = FindObjectOfType<TileGrid>();
    m_cTransform = transform;

    GlobalData.PlayModeToggled += OnPlayModeToggled;
  }


  void OnPlayModeToggled(bool isInPlayMode)
  {
    if (isInPlayMode)
      FindLinkedDoors();
  }


  void FindLinkedDoors()
  {
    var thisPosition = m_cTransform.position;
    var thisIndex = new Vector2Int((int)thisPosition.x, (int)thisPosition.y);

    //Indexes to look for.
    var rightIndex = thisIndex + Vector2Int.right;
    var leftIndex = thisIndex + Vector2Int.left;
    var upIndex = thisIndex + Vector2Int.up;
    var downIndex = thisIndex + Vector2Int.down;

    //Save the elements so we don't have to keep finding them.
    var rightElement = m_TileGrid.Get(rightIndex);
    var leftElement = m_TileGrid.Get(leftIndex);
    var upElement = m_TileGrid.Get(upIndex);
    var downElement = m_TileGrid.Get(downIndex);

    //Find right...
    if (rightElement.m_Type == TileType.DOOR && rightElement.m_TileColor == m_ColorCode.m_TileColor)
    {
      var doorTileLogic = rightElement.m_GameObject.GetComponent<DoorTileLogic>();
      var createdDoor = doorTileLogic.GetCreatedDoor();
      m_LinkedDoorRight = createdDoor.GetComponent<DoorLogic>();
    }
    //Find left...
    if (leftElement.m_Type == TileType.DOOR && leftElement.m_TileColor == m_ColorCode.m_TileColor)
    {
      var doorTileLogic = leftElement.m_GameObject.GetComponent<DoorTileLogic>();
      var createdDoor = doorTileLogic.GetCreatedDoor();
      m_LinkedDoorLeft = createdDoor.GetComponent<DoorLogic>();
    }
    //Find up...
    if (upElement.m_Type == TileType.DOOR && upElement.m_TileColor == m_ColorCode.m_TileColor)
    {
      var doorTileLogic = upElement.m_GameObject.GetComponent<DoorTileLogic>();
      var createdDoor = doorTileLogic.GetCreatedDoor();
      m_LinkedDoorUp = createdDoor.GetComponent<DoorLogic>();
    }
    //Find down...
    if (downElement.m_Type == TileType.DOOR && downElement.m_TileColor == m_ColorCode.m_TileColor)
    {
      var doorTileLogic = downElement.m_GameObject.GetComponent<DoorTileLogic>();
      var createdDoor = doorTileLogic.GetCreatedDoor();
      m_LinkedDoorDown = createdDoor.GetComponent<DoorLogic>();
    }
  }


  public void AttemptOpen()
  {
    if (!m_BeingOpenedByPropagation && !m_BeingOpenedByKey)
      Open();
  }


  void Open()
  {
    SetBeingOpenedByKey();
    DestroyLinkedDoors();
  }


  void SetBeingOpenedByKey()
  {
    DeactivateBackground();
    m_BeingOpenedByKey = true;

    if (m_LinkedDoorRight != null && !m_LinkedDoorRight.m_BeingOpenedByKey)
      m_LinkedDoorRight.SetBeingOpenedByKey();
    if (m_LinkedDoorLeft != null && !m_LinkedDoorLeft.m_BeingOpenedByKey)
      m_LinkedDoorLeft.SetBeingOpenedByKey();
    if (m_LinkedDoorUp != null && !m_LinkedDoorUp.m_BeingOpenedByKey)
      m_LinkedDoorUp.SetBeingOpenedByKey();
    if (m_LinkedDoorDown != null && !m_LinkedDoorDown.m_BeingOpenedByKey)
      m_LinkedDoorDown.SetBeingOpenedByKey();
  }


  /**
  * FUNCTION NAME: DestroyLinkedDoors
  * DESCRIPTION  : Destroy any linked doors.
  * INPUTS       : None
  * OUTPUTS      : None
  **/
  public void DestroyLinkedDoors()
  {
    m_BeingOpenedByPropagation = true;

    if (m_LinkedDoorRight != null)
      m_LinkedDoorRight.DestroyFromLink();
    if (m_LinkedDoorLeft != null)
      m_LinkedDoorLeft.DestroyFromLink();
    if (m_LinkedDoorUp != null)
      m_LinkedDoorUp.DestroyFromLink();
    if (m_LinkedDoorDown != null)
      m_LinkedDoorDown.DestroyFromLink();

    OpenThisPart();
  }

  /**
  * FUNCTION NAME: DestroyFromLink
  * DESCRIPTION  : Destroy a door due to it being linked to another destroyd door.
  * INPUTS       : None
  * OUTPUTS      : None
  **/
  public void DestroyFromLink()
  {
    if (m_BeingOpenedByPropagation)
      return;

    var openSequence = ActionMaster.Actions.Sequence();
    openSequence.Delay(m_LinkOpenDelay);
    openSequence.Call(DestroyLinkedDoors, gameObject);
    //TODO:  Feedback?
  }


  public void OpenThisPart()
  {
    if (!m_BeingOpenedByKey)
    {
      m_BeingOpenedByKey = true;
      DeactivateBackground();
    }

    DeactivateOutline();
  }


  void DeactivateBackground()
  {
    m_TextMeshRenderer.enabled = false;

    ActionMaster.Actions.SpriteAlpha(m_BackgroundSpriteRenderer.gameObject,
      m_FadeOpacity, m_FadeDuration, new Ease(Ease.Quad.InOut));
  }


  void DeactivateOutline()
  {
    m_Collider.enabled = false;

    ActionMaster.Actions.SpriteAlpha(m_OutlineSpriteRenderer.gameObject,
      m_FadeOpacity, m_FadeDuration, new Ease(Ease.Quad.InOut));
  }


  public void OnColorCodeSet(ColorCodeEventData eventData)
  {
    m_Color = eventData.m_Color;
  }


  /**
  * FUNCTION NAME: OnDestroy
  * DESCRIPTION  : Attempt to destroy other linked doors when this door is destroyed.
  * INPUTS       : None
  * OUTPUTS      : None
  **/
  void OnDestroy()
  {
    GlobalData.PlayModeToggled -= OnPlayModeToggled;
  }
}
