/***************************************************
File:           BoxTool.cs
Authors:        Christopher Onorati
Last Updated:   6/19/2019
Last Version:   2019.1.4

Description:
  Script for box tool control.

Copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using UnityEngine;

public class BoxTool : BlbTool
{
  public Outliner m_Outliner;

  /************************************************************************************/

  //Flag to check if the box tool is currently active.
  bool m_LeftBusy = false;
  bool m_RightBusy = false;

  string m_OperationNameOverride = "Box Tool";

  //Starting grid index of the box tool drag.
  Vector2Int m_StartIndex;
  Vector2Int m_EndIndex;

  /************************************************************************************/

  /**
  * FUNCTION NAME: Start
  * DESCRIPTION  : Initialize game objects used later and set tool ID.
  * INPUTS       : None
  * OUTPUTS      : None
  **/
  void Start()
  {
    m_OperationName = m_OperationNameOverride;
    m_TileGrid = FindObjectOfType<TileGrid>();
    m_TilesPalette = FindObjectOfType<TilesPalette>();
    m_ToolsPalette = FindObjectOfType<ToolsPalette>();

    m_ToolID = Tools.BOX;

    GlobalData.PlayModePreToggle += OnPlayModePreToggle;
  }


  private void OnPlayModePreToggle(bool isInPlayMode)
  {
    if (m_LeftBusy)
    {
      m_LeftBusy = false;
      PointerUpHelper(m_EndIndex, GlobalData.GetSelectedPrimaryTile());
    }
    else if (m_RightBusy)
    {
      m_RightBusy = false;
      PointerUpHelper(m_EndIndex, GlobalData.GetSelectedSecondaryTile());
    }
  }


  public override void LeftPointerDown(ToolEvent te)
  {
    if (GlobalData.IsInPlayMode() || m_LeftBusy || m_RightBusy)
      return;

    m_LeftBusy = true;
    BeginBatch();
    m_StartIndex = te.GridIndex;
    DragHelper(te);
    PrintStatusMessage();
  }


  public override void RightPointerDown(ToolEvent te)
  {
    if (GlobalData.IsInPlayMode() || m_LeftBusy || m_RightBusy)
      return;

    m_RightBusy = true;
    BeginBatch();
    m_StartIndex = te.GridIndex;
    DragHelper(te);
    PrintStatusMessage();
  }


  public override void LeftDrag(ToolEvent te)
  {
    if (GlobalData.IsInPlayMode() || !m_LeftBusy)
      return;

    DragHelper(te);
  }


  public override void RightDrag(ToolEvent te)
  {
    if (GlobalData.IsInPlayMode() || !m_RightBusy)
      return;

    DragHelper(te);
  }


  void DragHelper(ToolEvent te)
  {
    // outline
    var shouldPrint = m_EndIndex != te.GridIndex;

    m_EndIndex = te.GridIndex;
    Outline();

    if (shouldPrint)
      PrintStatusMessage();
  }


  public override void LeftPointerUp(ToolEvent te)
  {
    if (GlobalData.IsInPlayMode() || !m_LeftBusy)
      return;

    m_LeftBusy = false;
    PointerUpHelper(te.GridIndex, GlobalData.GetSelectedPrimaryTile());
  }


  public override void RightPointerUp(ToolEvent te)
  {
    if (GlobalData.IsInPlayMode() || !m_RightBusy)
      return;

    m_RightBusy = false;
    PointerUpHelper(te.GridIndex, GlobalData.GetSelectedSecondaryTile());
  }


  void PointerUpHelper(Vector2Int gridIndex, TileType tileType)
  {
    // TODO: this check seems unnecessary
    if (!m_ToolsPalette.IsBoxActive())
      return;

    Vector2Int moveDir = gridIndex - m_StartIndex;
    m_StartIndex = gridIndex;

    int dirXMultiplier = moveDir.x <= 0 ? 1 : -1;
    int dirYMultiplier = moveDir.y <= 0 ? 1 : -1;

    for (int i = Mathf.Abs(moveDir.x); i >= 0; --i)
    {
      for (int j = Mathf.Abs(moveDir.y); j >= 0; --j)
      {
        var xIndex = i * dirXMultiplier;
        var yIndex = j * dirYMultiplier;
        var indexOffset = new Vector2Int(xIndex, yIndex);
        var newGridIndex = m_StartIndex + indexOffset;
        AddRequest(newGridIndex, tileType, cloning: false);
      }
    }

    EndBatch();

    m_Outliner.Disable();
  }


  void Outline()
  {
    var min = m_StartIndex;
    var max = m_EndIndex;
    var delta = max - min;

    if (delta.x < 0)
    {
      var temp = min.x;
      min.x = max.x;
      max.x = temp;
    }

    if (delta.y < 0)
    {
      var temp = min.y;
      min.y = max.y;
      max.y = temp;
    }

    var minBounds = new Vector3(min.x, min.y);
    var maxBounds = new Vector3(max.x, max.y);

    if (min == max)
      m_Outliner.OutlineSinglePosition(minBounds);
    else
      m_Outliner.OutlineWithBounds(minBounds, maxBounds);
  }


  void PrintStatusMessage()
  {
    var difference = m_EndIndex - m_StartIndex;
    var width = Mathf.Abs(difference.x) + 1;
    var height = Mathf.Abs(difference.y) + 1;
    var diagonal = Mathf.Sqrt(width * width + height * height);
    var diagonalString = diagonal.ToString("f2");
    var message = $"Box size: <b>{width}</b> wide x <b>{height}</b> high, " +
      $"<b>{diagonalString}</b> diagonal";
    StatusBar.Print(message);
  }
}
