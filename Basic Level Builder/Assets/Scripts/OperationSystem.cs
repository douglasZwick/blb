using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OperationSystem : MonoBehaviour
{
  static OperationSystem Instance;
  static bool s_UseAutosaving;
  static int s_AutosaveInterval;
  static int s_OperationCounter = 0;
  static List<Operation> s_Operations = new List<Operation>();
  static int s_StackIndex = 0;  // distance from the right side of the stack
  public static Operation s_CurrentOperation;
  public static bool s_Frozen { get; private set; } = false;
  public static Operation s_MostRecentlyPerformedOperation
  {
    get { return s_Operations[s_Operations.Count - 1]; }
  }

  public FileSystem m_FileSystem;
  public TileGrid m_TileGrid;
  public int m_UndoDepth = 64;
  public bool m_UseAutosaving = true;
  public int m_AutosaveInterval = 4;


  private void Awake()
  {
    if (Instance != null)
    {
      var thisGameObjectName = gameObject.name;
      var instanceGameObjectName = Instance.gameObject.name;
      var messageString = $"An OperationSystem is being created on {thisGameObjectName}" +
        " when one is already present on {instanceGameObjectName}";
      throw new SingletonInstanceAlreadyExistsException(messageString);
    }

    Instance = this;

    s_Operations.Capacity = Instance.m_UndoDepth;
    s_UseAutosaving = m_UseAutosaving;
    s_AutosaveInterval = m_AutosaveInterval;
  }


  private void Update()
  {
    if (GlobalData.IsInPlayMode() || !HotkeyMaster.s_HotkeysEnabled)
      return;

    var modifierKeyHeld = HotkeyMaster.IsPrimaryModifierHeld();
    var undoKeyDown = Input.GetKeyDown(KeyCode.Z);
    var redoKeyDown = Input.GetKeyDown(KeyCode.Y);

    if (modifierKeyHeld)
    {
      if (undoKeyDown)
        AttemptUndo();
      else if (redoKeyDown)
        AttemptRedo();
    }
  }


  public void SetAutosaving(bool value)
  {
    s_UseAutosaving = m_UseAutosaving = value;

    var enabledString = value ? "enabled" : "disabled";
    StatusBar.Print($"Autosaving {enabledString}");
  }


  static public void Perform(Operation operation = null)
  {
    if (operation == null)
      operation = s_CurrentOperation;

    if (operation.m_IncrementOperationCounter)
      ++s_OperationCounter;

    // if we're performing an operation with stuff to the right of
    // the index, then we need to axe that stuff. conveniently, if
    // we're performing an operation with NOTHING to the right of
    // the index, then Prune just won't do anything anyway [ ^ ^]b
    Prune();

    Push(operation);

    if (s_UseAutosaving && s_OperationCounter >= s_AutosaveInterval)
    {
      Instance.m_FileSystem.Autosave();
      s_OperationCounter = 0;
    }
  }


  static public void BeginOperation(string name = "Operation", bool incrementOperationCounter = true)
  {
    // i believe the only way that we could be frozen at this moment
    // is if you start an operation legitimately, but then you move
    // the mouse off the level window or something and release it
    // there. this would then cause the tool not to know to call
    // EndOperation, so the current operation would still be active.
    // 
    // the solution here is basically to make it so that every delta
    // you attempt to add in that second operation ends up going into
    // the first one, which can then be ended normally. not the best
    // solution of all time, but i think it'll get er done
    if (s_Frozen)
      return;

    s_Frozen = true;
    s_CurrentOperation = new Operation(name, incrementOperationCounter);
  }


  static public void AddDelta(Vector2Int gridIndex, TileState oldState, TileState newState, GameObject tile = null)
  {
    s_CurrentOperation.AddDelta(gridIndex, oldState, newState, tile);
  }


  static public void AddDelta(TileGrid.Element oldElement, TileGrid.Element newElement, GameObject tile = null)
  {
    s_CurrentOperation.AddDelta(oldElement, newElement, tile);
  }


  static public void EndOperation()
  {
    // in a similar manner to what i mentioned above, in BeginOperation,
    // i suppose it is possible that this function could be called without
    // an operation having been begun properly. i think this is actually a
    // worse case than the one above, because it means that a bunch of grid
    // elements will be created that i think won't be added as deltas to an
    // operation, so they won't be undo/redoable. hence, i'm gonna log an
    // error in that case, but that's all i wanna do with it for now
    // 
    // TODO: research this. is it sufficient?
    if (!s_Frozen)
    {
      var message = "The OperationSystem was asked to end an operation when " +
        "no operation had been begun! Proceed with caution.";
      Debug.LogError(message);

      return;
    }

    if (!s_CurrentOperation.Empty)
      Perform();

    s_CurrentOperation = null;
    s_Frozen = false;
  }


  static public void AttemptUndo()
  {
    if (s_Frozen)
      return;

    if (s_StackIndex > s_Operations.Count - 1)
    {
      // nothing to undo
      StatusBar.Print("Nothing to undo.", highPriority: false, duration: 5);
    }
    else
    {
      Undo();
    }
  }


  static void Undo()
  {
    var indexToUndo = (s_Operations.Count - s_StackIndex) - 1;
    var operationToUndo = s_Operations[indexToUndo];

    StatusBar.Print("Undoing " + operationToUndo.m_Name + "...");

    foreach (var delta in operationToUndo.m_Deltas)
    {
      // for Undo, you enact the old state
      var stateToEnact = delta.OldState;
      var gridIndex = delta.GridIndex;
      Instance.m_TileGrid.EnactOperationState(stateToEnact, gridIndex);
    }

    Instance.m_TileGrid.RecomputeBounds();

    ++s_StackIndex;
  }


  static public void AttemptRedo()
  {
    if (s_Frozen)
      return;

    if (s_StackIndex <= 0)
    {
      // nothing to redo
      StatusBar.Print("Nothing to redo.", highPriority: false, duration: 5);
    }
    else
    {
      Redo();
    }
  }


  static void Redo()
  {
    var indexToRedo = s_Operations.Count - s_StackIndex;
    var operationToRedo = s_Operations[indexToRedo];

    StatusBar.Print("Redoing " + operationToRedo.m_Name + "...");

    foreach (var delta in operationToRedo.m_Deltas)
    {
      // for Redo, you enact the new state
      var stateToEnact = delta.NewState;
      var gridIndex = delta.GridIndex;
      Instance.m_TileGrid.EnactOperationState(stateToEnact, gridIndex);
    }

    Instance.m_TileGrid.RecomputeBounds();

    --s_StackIndex;
  }


  static void Prune()
  {
    // pop from the stack everything to the right of the index, if any

    if (s_StackIndex == 0)
      return;

    var firstIndexToRemove = s_Operations.Count - s_StackIndex;
    var countToRemove = s_StackIndex;
    s_Operations.RemoveRange(firstIndexToRemove, countToRemove);

    // reset the index
    s_StackIndex = 0;
  }


  static void Push(Operation operation)
  {
    if (s_Operations.Count == s_Operations.Capacity)
      s_Operations.RemoveAt(0);

    s_Operations.Add(operation);
  }
}


public class Operation
{
  public class Delta
  {
    public Vector2Int GridIndex;
    public TileState OldState;
    public TileState NewState;
    public GameObject Tile;
  }

  public List<Delta> m_Deltas = new List<Delta>();
  public string m_Name = "Operation";
  public bool m_IncrementOperationCounter = true;

  public bool Empty { get { return m_Deltas.Count == 0; } }

  public Operation() {}

  public Operation(string name, bool incrementOperationCounter = true)
  {
    m_Name = name;
    m_IncrementOperationCounter = incrementOperationCounter;
  }

  public Operation(string name, TileGrid.Element oldElement, TileGrid.Element newElement,
    bool incrementOperationCounter = true)
  {
    m_Name = name;
    AddDelta(oldElement, newElement);
    m_IncrementOperationCounter = incrementOperationCounter;
  }

  public void AddDelta(Vector2Int gridIndex, TileState oldState, TileState newState, GameObject tile = null)
  {
    var unique = ConfirmUniqueness(gridIndex);
    if (!unique)
      return;

    var delta = new Delta()
    {
      GridIndex = gridIndex,
      OldState = oldState,
      NewState = newState,
      Tile = tile,
    };

    m_Deltas.Add(delta);
  }

  public void AddDelta(TileGrid.Element oldElement, TileGrid.Element newElement, GameObject tile = null)
  {
    if (oldElement == null && newElement == null)
      return;

    var gridIndex = newElement == null ? oldElement.m_GridIndex : newElement.m_GridIndex;

    var unique = ConfirmUniqueness(gridIndex);
    if (!unique)
      return;

    var delta = new Delta()
    {
      OldState = oldElement == null ? new TileState() : oldElement.ToState(),
      NewState = newElement == null ? new TileState() : newElement.ToState(),
    };

    delta.GridIndex = gridIndex;
    delta.Tile = tile;

    m_Deltas.Add(delta);
  }

  bool ConfirmUniqueness(Vector2Int gridIndex)
  {
    foreach (var delta in m_Deltas)
      if (delta.GridIndex == gridIndex)
        return false;

    return true;
  }
}


public class SingletonInstanceAlreadyExistsException : System.Exception
{
  public SingletonInstanceAlreadyExistsException() : base() { }
  public SingletonInstanceAlreadyExistsException(string message) : base(message) { }
}
