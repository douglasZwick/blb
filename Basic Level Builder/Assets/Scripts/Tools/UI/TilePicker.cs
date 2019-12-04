/***************************************************
File:           TilePicker.cs
Authors:        Christopher Onorati
Last Updated:   6/13/2019
Last Version:   2019.1.4

Description:
  Logic to be placed on each tile that the user can
  select in the tile picker menu.  Perhaps there could
  have been a better name for this?  Oh well...

Copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; //Image.
using UnityEngine.EventSystems; //Access to Unity events for the mouse.

[RequireComponent(typeof(Image))]
public class TilePicker : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
  public static Dictionary<TileType, Image> s_Icons = new Dictionary<TileType, Image>();

  /************************************************************************************/

  [Tooltip("Type of tile this UI element represents.")]
  public TileType m_TileType;

  /************************************************************************************/

  //Image component of the game object.
  public Image m_IconImage;


  private void Awake()
  {
    var tileGrids = FindObjectsOfType<TileGrid>();

    foreach (var tileGrid in tileGrids)
    {
      tileGrid.RegisterRoot(m_TileType, gameObject.name);
    }

    s_Icons[m_TileType] = m_IconImage;
  }


  /**
  * FUNCTION NAME: OnPointerClick
  * DESCRIPTION  : Detects when the mouse clicks on a game object.
  * INPUTS       : eventData - Information regarding the input.
  * OUTPUTS      : None
  **/
  public void OnPointerClick(PointerEventData eventData)
  {
    //Only respond if I was the game object selected.
    if (eventData.pointerPress != gameObject)
      return;

    //Left mouse input handling.
    if(eventData.button == PointerEventData.InputButton.Left)
      GlobalData.SetPrimarySelectedTile(m_TileType);

    //Right mouse input handling.
    else if(eventData.button == PointerEventData.InputButton.Right)
      GlobalData.SetSecondarySelectedTile(m_TileType);
  }

  /**
  * FUNCTION NAME: OnPointerEnter
  * DESCRIPTION  : Detects when the mouse starts hovering over a game object.
  * INPUTS       : eventData - Information regarding the input.
  * OUTPUTS      : None
  **/
  public void OnPointerEnter(PointerEventData eventData)
  {
    //Only respond if I was the game object selected.
    if (eventData.pointerEnter != gameObject)
      return;

    //TODO: Code to make the tooltip appear.
  }

  /**
  * FUNCTION NAME: OnPointerExit
  * DESCRIPTION  : Detects when the mouse stops hovering over a game object.
  * INPUTS       : eventData - Information regarding the input.
  * OUTPUTS      : None
  **/
  public void OnPointerExit(PointerEventData eventData)
  {
    //TODO: Put code here to make tooltip disapear.
  }
}
