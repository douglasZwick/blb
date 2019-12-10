using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathTool : BlbTool
{
  enum State
  {
    Idle,
    Selecting,
    PlacingAnchorPoint,
    PlacingPathPoints,
  }

  public Outliner m_Outliner;

  Vector2Int m_PointerDownPosition;
  Vector2Int m_PointerDragEndPosition;
  List<TileGrid.Element> m_SelectedElements = new List<TileGrid.Element>();
  Vector2Int m_AnchorPoint;
  List<Vector2Int> m_PathPoints = new List<Vector2Int>();
  State m_State = State.Idle;


  void Start()
  {
    m_TileGrid = FindObjectOfType<TileGrid>();
    m_TilesPalette = FindObjectOfType<TilesPalette>();
    m_ToolsPalette = FindObjectOfType<ToolsPalette>();

    m_ToolID = Tools.PATH;
  }


  private void Update()
  {
    if (Input.GetButtonDown("Cancel") && m_State != State.Idle)
      EnterIdle();
  }


  public override void LeftPointerDown(ToolEvent te)
  {
    if (m_State == State.Idle || m_State == State.Selecting)
    {
      m_PointerDownPosition = te.GridIndex;
      m_PointerDragEndPosition = te.GridIndex;

      Outline();
      PrintBoxDimensionsMessage();
    }
    else if (m_State == State.PlacingAnchorPoint)
    {

    }
    else // m_State == State.PlacingPathPoints
    {

    }
  }


  public override void LeftDrag(ToolEvent te)
  {
    if (m_State == State.Idle || m_State == State.Selecting)
    {

    }
    else if (m_State == State.PlacingAnchorPoint)
    {

    }
    else // m_State == State.PlacingPathPoints
    {

    }
  }


  public override void LeftPointerUp(ToolEvent te)
  {
    if (m_State == State.Idle || m_State == State.Selecting)
    {
      m_PointerDragEndPosition = te.GridIndex;

      Outline();
      PrintAnchorPointMessage();
    }
    else if (m_State == State.PlacingAnchorPoint)
    {

    }
    else // m_State == State.PlacingPathPoints
    {

    }
  }


  void EnterIdle()
  {
    m_State = State.Idle;
    m_SelectedElements = new List<TileGrid.Element>();

    m_Outliner.Disable();

    StatusBar.Print(m_SelectedStatus);
  }


  void EnterSelecting()
  {
    m_State = State.Selecting;
  }


  void EnterPlacingAnchorPoint()
  {
    m_State = State.PlacingAnchorPoint;
  }


  void EnterPlacingPathPoints()
  {
    m_State = State.PlacingPathPoints;
  }


  void Outline()
  {
    var min = m_PointerDownPosition;
    var max = m_PointerDragEndPosition;
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


  public override void Deactivate()
  {
    m_Outliner.Disable();

    base.Deactivate();
  }


  void PrintBoxDimensionsMessage()
  {
    var difference = m_PointerDragEndPosition - m_PointerDownPosition;
    var width = Mathf.Abs(difference.x) + 1;
    var height = Mathf.Abs(difference.y) + 1;
    var diagonal = Mathf.Sqrt(width * width + height * height);
    var diagonalString = diagonal.ToString("f2");
    var message = $"Selection size: <b>{width}</b> wide x <b>{height}</b> high, " +
      $"<b>{diagonalString}</b> diagonal";
    StatusBar.Print(message);
  }


  void PrintAnchorPointMessage()
  {
    StatusBar.Print("Next, <b>left-click</b> to place the path's <b>anchor point</b>.");
  }


  void PrintPathPointMessage()
  {
    StatusBar.Print("Next, <b>left-click</b> to place <b>path points</b>. <b>Right-click</b> to confirm.");
  }
}
