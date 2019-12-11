/***************************************************
File:           SelectorTool.cs
Authors:        Christopher Onorati
Last Updated:   6/13/2019
Last Version:   2019.1.4

Description:
  Script for selector tool control.

Copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections.Generic; //Lists.
using UnityEngine;

public class SelectorTool : BlbTool
{
  public Outliner m_Outliner;
  public Color m_ActiveOutlineColor = Color.white;


  /************************************************************************************/

  //Game object selected last by the pointer. (position)
  Vector2Int m_vecPointerDownPosition;

  //Position of the pointer when ending drag.
  Vector2Int m_vecPointerDragEndPosition;

  //Game object saved via copy or cut. (position)
  Vector2Int m_vecPointerDownSavedPosition;

  //Saved position of the pointer when the drag stopped.
  Vector2Int m_vecPointerDragEndSavedPosition;

  //Flag to check if paste is valid.
  bool m_bCanPaste = false;

  //List of tile elements to cache.
  List<TileGrid.Element> m_ClipboardElements = new List<TileGrid.Element>();

  // THE CURSOR!!!!
  Transform m_Cursor;

  Color m_DefaultOutlineColor;
  float m_BackgroundOpacity;
  float m_InnerOutlineOpacity;

  /************************************************************************************/

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
    m_ToolsPalette = FindObjectOfType<ToolsPalette>();
    m_Cursor = GameObject.FindGameObjectWithTag("Cursor").transform;

    m_ToolID = Tools.SELECTOR;

    m_DefaultOutlineColor = m_Outliner.m_InnerOutlineRenderer.color;
    m_BackgroundOpacity = m_Outliner.m_BackgroundRenderer.color.a;
    m_InnerOutlineOpacity = m_Outliner.m_InnerOutlineRenderer.color.a;

    GlobalData.PlayModePreToggle += OnPlayModePreToggle;
  }


  void OnPlayModePreToggle(bool isInPlayMode)
  {
    m_Outliner.Disable();
  }


  public override void LeftPointerDown(ToolEvent te)
  {
    // update the pointer down position --
    // this will be used as the minBounds for the outliner

    m_vecPointerDownPosition = te.GridIndex;
    m_vecPointerDragEndPosition = te.GridIndex;

    SetOutlinerToDefaultColors();
    Outline();
    PrintStatusMessage();
  }


  public override void LeftPointerUp(ToolEvent te)
  {
    m_vecPointerDragEndPosition = te.GridIndex;

    Outline();
  }


  public override void LeftDrag(ToolEvent te)
  {
    var shouldPrint = m_vecPointerDragEndPosition != te.GridIndex;

    // update the end position every drag
    m_vecPointerDragEndPosition = te.GridIndex;

    Outline();

    if (shouldPrint)
      PrintStatusMessage();
  }


  void Outline()
  {
    var min = m_vecPointerDownPosition;
    var max = m_vecPointerDragEndPosition;
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


  /**
  * FUNCTION NAME: Update
  * DESCRIPTION  : Gets keyboard input for copy/cut/paste.
  * INPUTS       : None
  * OUTPUTS      : None
  **/
  private void Update()
  {
    if (!HotkeyMaster.s_HotkeysEnabled || GlobalData.IsInPlayMode())
      return;

    if (!m_ToolsPalette.IsSelectorActive())
    {
      if (Input.GetButtonDown(m_KeyboardShortcut))
        RequestActivate();

      return;
    }

    var modifierKeyHeld = HotkeyMaster.IsPrimaryModifierHeld();

    //Copy.
    if (modifierKeyHeld && Input.GetKeyDown(KeyCode.C))
      CreateClipboard(cutting: false);

    //Cut.
    else if (modifierKeyHeld && Input.GetKeyDown(KeyCode.X))
    {
      CreateClipboard(cutting: true);
    }

    //Paste.
    else if (modifierKeyHeld && Input.GetKeyDown(KeyCode.V) && m_bCanPaste)
      CreateSavedTiles();
  }

  /**
  * FUNCTION NAME: CreateClipboard
  * DESCRIPTION  : Create the data for the clipboard.
  * INPUTS       : None
  * OUTPUTS      : None
  **/
  void CreateClipboard(bool cutting = false)
  {
    m_vecPointerDownSavedPosition = m_vecPointerDownPosition;
    m_vecPointerDragEndSavedPosition = m_vecPointerDragEndPosition;
    m_bCanPaste = true;

    m_ClipboardElements.Clear();

    CreateCachedTiles(cutting);

    if (cutting)
      m_Outliner.Disable();
    else
      SetOutlinerToActiveColors();
  }

  /**
  * FUNCTION NAME: CreateSavedTiles
  * DESCRIPTION  : Create a bactch of files from the clipboard at the mouse's position.
  * INPUTS       : None
  * OUTPUTS      : None
  **/
  void CreateSavedTiles()
  {
    m_TileGrid.BeginBatch("Paste");

    var spawnIndex = new Vector2Int((int)m_Cursor.position.x, (int)m_Cursor.position.y);
    var indexOffset = spawnIndex - m_vecPointerDownSavedPosition;

    foreach (var clipboardElement in m_ClipboardElements)
    {
      var newIndex = clipboardElement.m_GridIndex + indexOffset;
      var newState = clipboardElement.ToState();

      m_TileGrid.AddRequest(newIndex, newState, false, false);
    }

    m_TileGrid.EndBatch(false);
    m_TileGrid.RecomputeBounds();
  }

  /**
  * FUNCTION NAME: CreateCachedTiles
  * DESCRIPTION  : Geneterates the tile cache (clipboard) and saves them + the types out into lists.
  * INPUTS       : None
  * OUTPUTS      : None
  **/
  void CreateCachedTiles(bool cutting = false)
  {
    Vector2Int moveDir = m_vecPointerDragEndSavedPosition - m_vecPointerDownSavedPosition;

    int dirXMultiplier = moveDir.x <= 0 ? 1 : -1;
    int dirYMultiplier = moveDir.y <= 0 ? 1 : -1;

    if (cutting)
    {
      m_TileGrid.BeginBatch("Cut");
    }

    for (int i = 0; i <= Mathf.Abs(moveDir.x); i++)
    {
      for (int j = 0; j <= Mathf.Abs(moveDir.y); j++)
      {
        Vector2Int index = m_vecPointerDragEndSavedPosition + new Vector2Int(i * dirXMultiplier, j * dirYMultiplier);

        var currentElement = m_TileGrid.Get(index);
        m_ClipboardElements.Add(currentElement);

        if (cutting)
        {
          m_TileGrid.AddRequest(index, TileType.EMPTY, false, false);
        }
      }
    }

    if (cutting)
    {
      m_TileGrid.EndBatch(false);
      m_TileGrid.RecomputeBounds();
    }
  }


  void SetOutlinerToDefaultColors()
  {
    var color = m_DefaultOutlineColor;
    color.a = m_BackgroundOpacity;
    m_Outliner.m_BackgroundRenderer.color = color;
    color.a = m_InnerOutlineOpacity;
    m_Outliner.m_InnerOutlineRenderer.color = color;
  }


  void SetOutlinerToActiveColors()
  {
    var color = m_ActiveOutlineColor;
    color.a = m_BackgroundOpacity;
    m_Outliner.m_BackgroundRenderer.color = color;
    color.a = m_InnerOutlineOpacity;
    m_Outliner.m_InnerOutlineRenderer.color = color;
  }


  void PrintStatusMessage()
  {
    var difference = m_vecPointerDragEndPosition - m_vecPointerDownPosition;
    var width = Mathf.Abs(difference.x) + 1;
    var height = Mathf.Abs(difference.y) + 1;
    var diagonal = Mathf.Sqrt(width * width + height * height);
    var diagonalString = diagonal.ToString("f2");
    var message = $"Selection size: <b>{width}</b> wide x <b>{height}</b> high, " +
      $"<b>{diagonalString}</b> diagonal";
    StatusBar.Print(message);
  }


  private void OnDestroy()
  {
    GlobalData.PlayModePreToggle -= OnPlayModePreToggle;
  }
}
