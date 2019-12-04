/***************************************************
File:           EyeDropperTool.cs
Authors:        Christopher Onorati
Last Updated:   6/13/2019
Last Version:   2019.1.4

Description:
  Script for eye dropper tool control.

Copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using UnityEngine.UI; //Update UI images.
using UnityEngine;

public class EyeDropperTool : BlbTool
{
  /**
  * FUNCTION NAME: Start
  * DESCRIPTION  : Initialize game objects used later and set tool ID.
  * INPUTS       : None
  * OUTPUTS      : None
  **/
  void Start()
  {
    m_TileGrid = FindObjectOfType<TileGrid>();
    m_TilesPalette = FindObjectOfType<TilesPalette>();

    m_ToolID = Tools.EYE_DROPPER;
  }


  public override void LeftPointerDown(ToolEvent te)
  {
    LeftHelper(te);
  }


  public override void RightPointerDown(ToolEvent te)
  {
    RightHelper(te);
  }


  public override void LeftDrag(ToolEvent te)
  {
    LeftHelper(te);
  }


  public override void RightDrag(ToolEvent te)
  {
    RightHelper(te);
  }


  void LeftHelper(ToolEvent te)
  {
    // if we're in play mode, scram
    if (GlobalData.IsInPlayMode())
      return;

    var primaryTileType = GetTileTypeFromEvent(te);
    GlobalData.SetPrimarySelectedTile(primaryTileType);
  }


  void RightHelper(ToolEvent te)
  {
    // if we're in play mode, scram
    if (GlobalData.IsInPlayMode())
      return;

    var secondaryTileType = GetTileTypeFromEvent(te);
    GlobalData.SetSecondarySelectedTile(secondaryTileType);
  }


  TileType GetTileTypeFromEvent(ToolEvent te)
  {
    var selectedTile = m_TileGrid.Get(te.GridIndex);
    return selectedTile.m_Type;
  }
}
