/***************************************************
File:           BrushTool.cs
Authors:        Christopher Onorati, Doug Zwick
Last Updated:   6/19/2019
Last Version:   2019.1.4

Description:
  Script for brush tool control.

Copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using UnityEngine;

/**
* CLASS NAME  : BrushTool
* DESCRIPTION : Class used to dictate where a new tile will be placed.
*             : Inherits from BlbTool so that it can respond to mouse events.
**/
public class BrushTool : BlbTool
{
  /************************************************************************************/

  [Tooltip("Sound effects to play when placing a tile.")]
  public AudioClip[] m_PlaceTileClips;

  /************************************************************************************/

  //Audio source of the game object.
  AudioSource m_cAudioSource;

  bool m_LeftBusy = false;
  bool m_RightBusy = false;

  string m_OperationNameOverride = "Brush Tool";

  /************************************************************************************/

  /**
  * FUNCTION NAME: Start
  * DESCRIPTION  : Caches components, connects to events, and loads prefabs
  * INPUTS       : None
  * OUTPUTS      : None
  **/
  void Start()
  {
    m_OperationName = m_OperationNameOverride;
    m_TileGrid = FindObjectOfType<TileGrid>();
    m_TilesPalette = FindObjectOfType<TilesPalette>();
    m_cAudioSource = GetComponent<AudioSource>();

    m_ToolID = Tools.BRUSH;
  }


  public override void LeftPointerDown(ToolEvent te)
  {
    if (GlobalData.IsInPlayMode() || m_LeftBusy || m_RightBusy)
      return;

    m_LeftBusy = true;
    BeginBatch();
    PaintHelper(te, GlobalData.GetSelectedPrimaryTile());
  }


  public override void RightPointerDown(ToolEvent te)
  {
    if (GlobalData.IsInPlayMode() || m_LeftBusy || m_RightBusy)
      return;

    m_RightBusy = true;
    BeginBatch();
    PaintHelper(te, GlobalData.GetSelectedSecondaryTile());
  }


  public override void LeftDrag(ToolEvent te)
  {
    if (GlobalData.IsInPlayMode() || !m_LeftBusy)
      return;

    PaintHelper(te, GlobalData.GetSelectedPrimaryTile());
  }


  public override void RightDrag(ToolEvent te)
  {
    if (GlobalData.IsInPlayMode() || !m_RightBusy)
      return;

    PaintHelper(te, GlobalData.GetSelectedSecondaryTile());
  }


  public override void LeftPointerUp(ToolEvent te)
  {
    if (GlobalData.IsInPlayMode() || !m_LeftBusy)
      return;

    m_LeftBusy = false;
    EndBatch();
  }


  public override void RightPointerUp(ToolEvent te)
  {
    if (GlobalData.IsInPlayMode() || !m_RightBusy)
      return;

    m_RightBusy = false;
    EndBatch();
  }


  void PaintHelper(ToolEvent te, TileType tileType)
  {
    AddRequest(te.GridIndex, tileType, cloning: false);
  }
}
