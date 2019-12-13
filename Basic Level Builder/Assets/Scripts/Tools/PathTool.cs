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
  public GameObject m_PathIconPrefab;

  public static GameObject s_PathIconPrefab;

  Vector2Int m_PointerDownPosition;
  Vector2Int m_PointerDragEndPosition;
  List<TileGrid.Element> m_SelectedElements;
  Vector2Int m_AnchorPoint;
  List<Vector2Int> m_Path;
  State m_State = State.Idle;


  void Start()
  {
    m_TileGrid = FindObjectOfType<TileGrid>();
    m_TilesPalette = FindObjectOfType<TilesPalette>();
    m_ToolsPalette = FindObjectOfType<ToolsPalette>();

    m_ToolID = Tools.PATH;

    s_PathIconPrefab = m_PathIconPrefab;

    GlobalData.PlayModePreToggle += OnPlayModePreToggle;
  }


  void OnPlayModePreToggle(bool isInPlayMode)
  {
    if (m_State != State.Idle)
      EnterIdle();
  }


  private void Update()
  {
    if (Input.GetButtonDown("Cancel") && m_State != State.Idle)
    {
      EnterIdle();
    }
    else if (Input.GetButtonDown("Submit") && m_State == State.PlacingPathPoints)
    {
      AssignPathPoints();
      EnterIdle();
    }
  }


  public override void LeftPointerDown(ToolEvent te)
  {
    var gridIndex = te.GridIndex;

    if (m_State == State.Idle)
    {
      m_PointerDownPosition = gridIndex;
      m_PointerDragEndPosition = gridIndex;

      Outline();
      EnterSelecting();
      PrintBoxDimensionsMessage();
    }
    else if (m_State == State.Selecting)
    {
      // This... cannot be....!!!!!
    }
    else if (m_State == State.PlacingAnchorPoint)
    {
      m_AnchorPoint = gridIndex;
      PrintAnchorPointPositionMessage();
    }
    else // m_State == State.PlacingPathPoints
    {
      AddNewPathPoint(gridIndex);
      PrintPathPointPositionsMessage();
    }
  }


  public override void LeftDrag(ToolEvent te)
  {
    var gridIndex = te.GridIndex;

    if (m_State == State.Idle)
    {
      // This... cannot be....!!!!!
    }
    else if (m_State == State.Selecting)
    {
      var shouldPrint = m_PointerDragEndPosition != gridIndex;
      m_PointerDragEndPosition = gridIndex;

      Outline();

      if (shouldPrint)
        PrintBoxDimensionsMessage();
    }
    else if (m_State == State.PlacingAnchorPoint)
    {
      if (m_AnchorPoint != gridIndex)
        PrintAnchorPointPositionMessage();

      m_AnchorPoint = gridIndex;
    }
    else // m_State == State.PlacingPathPoints
    {
      var currentPathPoint = m_Path[m_Path.Count - 1] - m_AnchorPoint;
      UpdateCurrentPathPoint(gridIndex);

      if (currentPathPoint != gridIndex)
        PrintPathPointPositionsMessage();
    }
  }


  public override void LeftPointerUp(ToolEvent te)
  {
    var gridIndex = te.GridIndex;

    if (m_State == State.Idle)
    {
      // This... cannot be....!!!!!
    }
    else if (m_State == State.Selecting)
    {
      m_PointerDragEndPosition = gridIndex;

      Outline();
      GatherSelectedElements();
      EnterPlacingAnchorPoint();
    }
    else if (m_State == State.PlacingAnchorPoint)
    {
      m_AnchorPoint = gridIndex;

      EnterPlacingPathPoints();
    }
    else // m_State == State.PlacingPathPoints
    {
      var currentPathPoint = m_Path[m_Path.Count - 1] - m_AnchorPoint;
      UpdateCurrentPathPoint(gridIndex);

      if (currentPathPoint != gridIndex)
        PrintPathPointPositionsMessage();
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

    PrintAnchorPointMessage();
  }


  void EnterPlacingPathPoints()
  {
    m_State = State.PlacingPathPoints;

    m_Path = new List<Vector2Int>();
    PrintPathPointMessage();
  }


  void GatherSelectedElements()
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

    m_SelectedElements = new List<TileGrid.Element>();

    for (var y = min.y; y <= max.y; ++y)
    {
      for (var x = min.x; x <= max.x; ++x)
      {
        var index = new Vector2Int(x, y);

        if (!m_TileGrid.Check(index))
          continue;

        m_SelectedElements.Add(m_TileGrid.Get(index));
      }
    }
  }


  void AddNewPathPoint(Vector2Int gridIndex)
  {
    m_Path.Add(gridIndex - m_AnchorPoint);
  }


  void UpdateCurrentPathPoint(Vector2Int gridIndex)
  {
    m_Path[m_Path.Count - 1] = gridIndex - m_AnchorPoint;
  }


  void AssignPathPoints()
  {
    if (m_Path.Count <= 0)
      return;

    m_TileGrid.BeginBatch("Assign Path");

    foreach (var element in m_SelectedElements)
    {
      var newState = element.ToState();
      newState.Path = m_Path;
      m_TileGrid.AddRequest(element.m_GridIndex, newState, recomputeBounds: false);
    }

    m_TileGrid.EndBatch(createDialogs: false);
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
    if (m_State != State.Idle)
      EnterIdle();

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
    StatusBar.Print("Next, <b>left-click</b> to place the path's <b>anchor point</b>");
  }


  void PrintAnchorPointPositionMessage()
  {
    var x = m_AnchorPoint.x;
    var y = m_AnchorPoint.y;
    var message = $"Placing anchor point at <color=#FFFF00><b>({x}, {y})</b></color>";
    StatusBar.Print(message);
  }


  void PrintPathPointMessage()
  {
    StatusBar.Print("Next, <b>left-click</b> to place <b>path points</b> relative to the anchor point. Press <b>Space</b> to finish");
  }


  void PrintPathPointPositionsMessage()
  {
    var count = m_Path.Count;
    var pointWord = count == 1 ? "point" : "points";
    var message = $"<b>{count}</b> {pointWord}: <color=#FFFF00><b>";

    if (count == 1)
    {
      message += $"{m_Path[0].ToString()}</b></color>";
    }
    else if (count <= 6)
    {
      message += $"{m_Path[0].ToString()}</b></color>";

      for (var i = 1; i < count; ++i)
      {
        message += $" > <color=#FFFF00><b>{m_Path[i].ToString()}</b></color>";
      }
    }
    else
    {
      var p0Str = m_Path[0].ToString();
      var p1Str = m_Path[1].ToString();
      var p2Str = m_Path[2].ToString();
      var pCountMinus3Str = m_Path[count - 3].ToString();
      var pCountMinus2Str = m_Path[count - 2].ToString();
      var pCountMinus1Str = m_Path[count - 1].ToString();

      message += $"{p0Str}</b></color> > " +
        $"<color=#FFFF00><b>{p1Str}</b></color> > " +
        $"<color=#FFFF00><b>{p2Str}</b></color> > ... > " +
        $"<color=#FFFF00><b>{pCountMinus3Str}</b></color> > " +
        $"<color=#FFFF00><b>{pCountMinus2Str}</b></color> > " +
        $"<color=#FFFF00><b>{pCountMinus1Str}</b></color>";
    }

    message += " | Press <b>Space</b> to finish";

    StatusBar.Print(message);
  }


  private void OnDestroy()
  {
    GlobalData.PlayModePreToggle -= OnPlayModePreToggle;
  }
}
