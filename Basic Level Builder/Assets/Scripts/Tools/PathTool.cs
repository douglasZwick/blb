using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathTool : BlbTool
{
  enum State
  {
    Idle,
    SelectingForCreation,
    PlacingAnchorPoint,
    PlacingPathPoints,
    SelectingForModification,
    ReadyToModify,
    ModifyingPathPoint,
  }

  public Outliner m_CreationOutliner;
  public Outliner m_ModificationOutliner;
  public GameObject m_PathIconPrefab;
  public GameObject m_AnchorIconPrefab;
  public PathNodeIcon m_NodeIconPrefab;

  public static GameObject s_PathIconPrefab;
  public static GameObject s_AnchorIconPrefab;
  public static PathNodeIcon s_NodeIconPrefab;

  Vector2Int m_PointerDownPosition;
  Vector2Int m_PointerDragEndPosition;
  List<TileGrid.Element> m_SelectedElements;
  Vector2Int m_AnchorIndex;
  Vector2Int m_AnchorStartingIndex; // used for moving the anchor
  List<Vector2Int> m_Path;
  State m_State = State.Idle;
  Transform m_AnchorIconTransform;
  List<PathNodeIcon> m_NodeIcons;
  int m_IndexBeingModified;


  void Start()
  {
    m_TileGrid = FindObjectOfType<TileGrid>();
    m_TilesPalette = FindObjectOfType<TilesPalette>();
    m_ToolsPalette = FindObjectOfType<ToolsPalette>();

    m_ToolID = Tools.PATH;

    s_PathIconPrefab = m_PathIconPrefab;
    s_AnchorIconPrefab = m_AnchorIconPrefab;
    s_NodeIconPrefab = m_NodeIconPrefab;

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
    else if (Input.GetButtonDown("Submit") &&
      (m_State == State.PlacingPathPoints || m_State == State.ReadyToModify))
    {
      AssignPathPoints();
      EnterIdle();
    }
    else if (Input.GetButtonDown("Delete") && m_State == State.PlacingAnchorPoint)
    {
      DeletePath();
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

      Outline(m_CreationOutliner);
      EnterSelectingForCreation();
      PrintBoxDimensionsMessage();
    }
    else if (m_State == State.SelectingForCreation)
    {
      // This... cannot be....!!!!!
    }
    else if (m_State == State.PlacingAnchorPoint)
    {
      SetAnchor(gridIndex);
    }
    else if (m_State == State.PlacingPathPoints)
    {
      AddNewPathPoint(gridIndex);
      PrintPathPointPositionsMessage();
    }
    else if (m_State == State.ReadyToModify)
    {
      var index = GetPathIndexAtGridIndex(gridIndex);

      if (index != null)
      {
        m_IndexBeingModified = index.Value;
        EnterModifyingPathPoint();
      }
    }
  }


  public override void RightPointerDown(ToolEvent te)
  {
    // TODO: gotta fix this as ASAP as possible
    // but for now, it's borken, so I gotta get outta here
    return;

    var gridIndex = te.GridIndex;

    if (m_State == State.Idle)
    {
      m_PointerDownPosition = gridIndex;
      m_PointerDragEndPosition = gridIndex;

      Outline(m_ModificationOutliner);
      EnterSelectingForModification();
      PrintBoxDimensionsMessage();
    }
    else if (m_State == State.ReadyToModify)
    {
      m_PointerDownPosition = gridIndex;
      m_PointerDragEndPosition = gridIndex;
      m_AnchorStartingIndex = m_AnchorIndex;

      PrintAnchorPointPositionMessage();
    }
  }


  public override void LeftDrag(ToolEvent te)
  {
    var gridIndex = te.GridIndex;

    if (m_State == State.Idle)
    {
      // This... cannot be....!!!!!
    }
    else if (m_State == State.SelectingForCreation)
    {
      var shouldPrint = m_PointerDragEndPosition != gridIndex;
      m_PointerDragEndPosition = gridIndex;

      Outline(m_CreationOutliner);

      if (shouldPrint)
        PrintBoxDimensionsMessage();
    }
    else if (m_State == State.PlacingAnchorPoint)
    {
      MoveAnchor(gridIndex);
    }
    else if (m_State == State.PlacingPathPoints)
    {
      var currentPathPoint = m_Path[m_Path.Count - 1] - m_AnchorIndex;
      UpdateCurrentPathPoint(gridIndex);

      if (currentPathPoint != gridIndex)
        PrintPathPointPositionsMessage();
    }
    else if (m_State == State.ModifyingPathPoint)
    {
      MovePathPoint(gridIndex);
    }
  }


  public override void RightDrag(ToolEvent te)
  {
    var gridIndex = te.GridIndex;

    if (m_State == State.Idle)
    {
      // This... cannot be....!!!!!
    }
    else if (m_State == State.SelectingForModification)
    {
      var shouldPrint = m_PointerDragEndPosition != gridIndex;
      m_PointerDragEndPosition = gridIndex;

      Outline(m_ModificationOutliner);

      if (shouldPrint)
        PrintBoxDimensionsMessage();
    }
    else if (m_State == State.ReadyToModify)
    {
      m_PointerDragEndPosition = gridIndex;

      var difference = m_PointerDragEndPosition - m_PointerDownPosition;
      MoveAnchor(m_AnchorStartingIndex + difference);
    }
  }


  public override void LeftPointerUp(ToolEvent te)
  {
    var gridIndex = te.GridIndex;

    if (m_State == State.Idle)
    {
      // This... cannot be....!!!!!
    }
    else if (m_State == State.SelectingForCreation)
    {
      m_PointerDragEndPosition = gridIndex;

      Outline(m_CreationOutliner);
      GatherSelectedElements();

      if (m_SelectedElements.Count == 0)
        EnterIdle();
      else
        EnterPlacingAnchorPoint();
    }
    else if (m_State == State.PlacingAnchorPoint)
    {
      m_AnchorIndex = gridIndex;

      EnterPlacingPathPoints();
    }
    else if (m_State == State.PlacingPathPoints)
    {
      var currentPathPoint = m_Path[m_Path.Count - 1] - m_AnchorIndex;
      UpdateCurrentPathPoint(gridIndex);

      if (currentPathPoint != gridIndex)
        PrintPathPointPositionsMessage();
    }
    else if (m_State == State.ModifyingPathPoint)
    {
      // TODO: this is a terrible hack but I gotta go with it for now
      foreach (var icon in m_NodeIcons)
        Destroy(icon);

      EnterReadyToModify();
    }
  }


  public override void RightPointerUp(ToolEvent te)
  {
    if (m_State == State.SelectingForModification)
    {
      m_PointerDragEndPosition = te.GridIndex;

      Outline(m_ModificationOutliner);
      GatherSelectedElements();

      if (AllSelectedPathsMatch())
      {
        if (m_SelectedElements.Count == 0 || m_SelectedElements[0].GetComponent<PathMover>() == null)
        {
          EnterIdle();

          var message = "No path is selected, so there is nothing to modify";
          StatusBar.Print(message);
        }
        else
        {
          EnterReadyToModify();
        }
      }
      else
      {
        EnterIdle();

        var message = "Not all the selected elements have matching paths, " +
          "so there is no single path to modify";
        StatusBar.Print(message);
      }
    }
  }


  void EnterIdle()
  {
    m_State = State.Idle;
    m_SelectedElements = new List<TileGrid.Element>();

    if (m_AnchorIconTransform != null)
      Destroy(m_AnchorIconTransform.gameObject);
    if (m_NodeIcons != null)
      ClearIcons();

    m_CreationOutliner.Disable();
    m_ModificationOutliner.Disable();

    StatusBar.Print(m_SelectedStatus);
  }


  void EnterSelectingForCreation()
  {
    m_State = State.SelectingForCreation;
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
    m_NodeIcons = new List<PathNodeIcon>();
    PrintPathPointMessage();
  }


  void EnterSelectingForModification()
  {
    m_State = State.SelectingForModification;
  }


  void EnterReadyToModify()
  {
    m_Path = new List<Vector2Int>();
    m_NodeIcons = new List<PathNodeIcon>();

    var anchorIndex = m_SelectedElements[0].m_GridIndex;
    SetAnchor(anchorIndex);

    PreparePathForModification();

    m_State = State.ReadyToModify;

    var message = "Move path points with the <b>left mouse button</b>, " +
      "or move the anchor point with the <b>right mouse button</b>. " +
      "Press <b>Space</b> to finish.";
    StatusBar.Print(message);
  }

  
  void EnterModifyingPathPoint()
  {
    m_State = State.ModifyingPathPoint;

    PrintPathPointModificationMessage();
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


  void SetAnchor(Vector2Int gridIndex)
  {
    m_AnchorIndex = gridIndex;
    var anchorPosition = new Vector3(gridIndex.x, gridIndex.y, 0);
    var anchorIcon = Instantiate(s_AnchorIconPrefab, anchorPosition, Quaternion.identity);
    m_AnchorIconTransform = anchorIcon.transform;
    PrintAnchorPointPositionMessage();
  }


  void MoveAnchor(Vector2Int gridIndex)
  {
    var oldAnchorIndex = m_AnchorIndex;
    m_AnchorIndex = gridIndex;

    if (m_AnchorIndex != oldAnchorIndex)
      PrintAnchorPointPositionMessage();

    var anchorPosition = new Vector3(gridIndex.x, gridIndex.y, 0);
    m_AnchorIconTransform.position = anchorPosition;
    m_AnchorIndex = gridIndex;

    var delta = m_AnchorIndex - oldAnchorIndex;

    foreach (var nodeIcon in m_NodeIcons)
      nodeIcon.MoveBy(delta);
  }


  bool AllSelectedPathsMatch()
  {
    if (m_SelectedElements.Count == 0)
      return true;

    var firstElement = m_SelectedElements[0];
    var firstPathMover = firstElement.GetComponent<PathMover>();

    foreach (var element in m_SelectedElements)
    {
      var pathMover = element.GetComponent<PathMover>();

      if (!PathMover.SamePath(firstPathMover, pathMover))
        return false;
    }

    return true;
  }


  void AddNewPathPoint(Vector2Int gridIndex)
  {
    m_Path.Add(gridIndex - m_AnchorIndex);
    AddNewNodeIcon(gridIndex);
  }


  void AddNewNodeIcon(Vector2Int gridIndex)
  {
    var icon = Instantiate(s_NodeIconPrefab);
    icon.MoveTo(gridIndex);
    m_NodeIcons.Add(icon);

    UpdateIcons();
  }


  void MovePathPoint(Vector2Int gridIndex)
  {
    m_Path[m_IndexBeingModified] = gridIndex - m_AnchorIndex;
    m_NodeIcons[m_IndexBeingModified].MoveTo(gridIndex);

    UpdateIcons();

    PrintPathPointModificationMessage();
  }


  void UpdateCurrentPathPoint(Vector2Int gridIndex)
  {
    var currentNodeIndex = m_Path.Count - 1;
    m_Path[currentNodeIndex] = gridIndex - m_AnchorIndex;
    m_NodeIcons[currentNodeIndex].MoveTo(gridIndex);

    UpdateIcons();
  }


  int? GetPathIndexAtGridIndex(Vector2Int gridIndex)
  {
    var adjustedIndex = gridIndex - m_AnchorIndex;
    var pathLength = m_Path.Count;

    for (var i = pathLength - 1; i >= 0; --i)
    {
      var node = m_Path[i];

      if (node == adjustedIndex)
        return i;
    }

    return null;
  }


  void UpdateIcons()
  {
    var iconCount = m_NodeIcons.Count;

    for (var i = 0; i < iconCount; ++i)
    {
      m_NodeIcons[i].Initialize(i);
    }

    for (var i = 0; i < iconCount; ++i)
    {
      var earlierIcon = m_NodeIcons[i];
      var duplicate = false;

      for (var j = i + 1; j < iconCount; ++j)
      {
        var laterIcon = m_NodeIcons[j];

        if (earlierIcon.m_GridIndex == laterIcon.m_GridIndex)
        {
          duplicate = true;
          laterIcon.Merge(earlierIcon);

          break;
        }
      }

      if (duplicate)
        earlierIcon.Hide();
      else
        earlierIcon.Show();
    }
  }


  void ClearIcons()
  {
    foreach (var icon in m_NodeIcons)
      Destroy(icon.gameObject);

    m_NodeIcons.Clear();
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


  void DeletePath()
  {
    if (m_Path.Count <= 0)
      return;

    m_TileGrid.BeginBatch("Delete Path");

    foreach (var element in m_SelectedElements)
    {
      var newState = element.ToState();
      newState.Path = null;
      m_TileGrid.AddRequest(element.m_GridIndex, newState, recomputeBounds: false);
    }

    m_TileGrid.EndBatch(createDialogs: false);
  }


  void PreparePathForModification()
  {
    m_Path = m_SelectedElements[0].m_Path;

    foreach (var gridIndex in m_Path)
      AddNewNodeIcon(gridIndex + m_AnchorIndex);
  }


  void Outline(Outliner outliner)
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
      outliner.OutlineSinglePosition(minBounds);
    else
      outliner.OutlineWithBounds(minBounds, maxBounds);
  }


  public override void Deactivate()
  {
    if (m_State != State.Idle)
      EnterIdle();

    m_CreationOutliner.Disable();
    m_ModificationOutliner.Disable();

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
    StatusBar.Print("Next, <b>left-click</b> to place the path's <b>anchor point</b>, or press <b>Delete</b> to delete the selected path");
  }


  void PrintAnchorPointPositionMessage()
  {
    var x = m_AnchorIndex.x;
    var y = m_AnchorIndex.y;
    var message = $"Anchor point at <color=#FFFF00><b>({x}, {y})</b></color>";
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


  void PrintPathPointModificationMessage()
  {
    var index = m_IndexBeingModified;
    var node = m_Path[index];
    var message = $"Path point <color=#FFFF00><b>{index}</b></color> " +
      $"is now at <color=#FFFF00><b>{node}</b></color>";
    StatusBar.Print(message);
  }


  private void OnDestroy()
  {
    GlobalData.PlayModePreToggle -= OnPlayModePreToggle;
  }
}
