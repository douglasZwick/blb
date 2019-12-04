/***************************************************
File:           BlbTool.cs
Authors:        Doug Zwick, Chris Onorati
Last Updated:   6/19/2019
Last Version:   2019.1.4

Description:
  Base class for all of the BLB tools.

Copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BlbTool : MonoBehaviour
{
  /************************************************************************************/

  [HideInInspector]
  public Tools m_ToolID;

  public Color m_SelectedColor;
  public Color m_DefaultColor;
  public Image m_Icon;
  public Image m_SelectionIndicator;
  public string m_SelectedStatus;
  public string m_KeyboardShortcut;

  /************************************************************************************/

  // The main grid component, found on Start elsewhere in the scene, that manages all the tiles
  protected TileGrid m_TileGrid;

  // The TilesPalette component is needed because it contains the tile prefabs that this tool uses
  protected TilesPalette m_TilesPalette;
  protected ToolsPalette m_ToolsPalette;

  //List of all of the Color Codes to assing to for the next batch.
  protected GameObject m_pLastCreatedTile;

  protected string m_OperationName = "Error";


  private void Awake()
  {
    m_ToolsPalette = FindObjectOfType<ToolsPalette>();
  }


  private void Update()
  {
    if (!HotkeyMaster.s_HotkeysEnabled || GlobalData.IsInPlayMode())
      return;

    if (Input.GetButtonDown(m_KeyboardShortcut))
      RequestActivate();
  }


  /************************************************************************************/
  /************************************************************************************/
  /*******************************POINTER FUNCTIONS************************************/
  /************************************************************************************/
  /************************************************************************************/
  public virtual void LeftPointerDown(ToolEvent te) { }
  public virtual void RightPointerDown(ToolEvent te) { }
  public virtual void LeftPointerUp(ToolEvent te) { }
  public virtual void RightPointerUp(ToolEvent te) { }
  public virtual void BeginLeftDrag(ToolEvent te) { }
  public virtual void BeginRightDrag(ToolEvent te) { }
  public virtual void LeftDrag(ToolEvent te) { }
  public virtual void RightDrag(ToolEvent te) { }
  public virtual void EndLeftDrag(ToolEvent te) { }
  public virtual void EndRightDrag(ToolEvent te) { }


  protected void BeginBatch()
  {
    m_TileGrid.BeginBatch(m_OperationName);
  }


  protected void AddRequest(Vector2Int gridIndex, TileType tileType, bool cloning = true)
  {
    m_TileGrid.AddRequest(gridIndex, tileType, cloning: cloning);
  }


  protected void EndBatch()
  {
    m_TileGrid.EndBatch();
  }


  public void RequestActivate()
  {
    m_ToolsPalette.SetToolActive(this);
  }


  public void Activate()
  {
    m_Icon.color = m_SelectedColor;
    m_SelectionIndicator.enabled = true;

    StatusBar.Print(m_SelectedStatus);
  }


  public virtual void Deactivate()
  {
    m_Icon.color = m_DefaultColor;
    m_SelectionIndicator.enabled = false;
  }
}


public class ToolEvent
{
  public TileGrid.Element Element;
  public Vector2 EventScreenPosition;
  public Vector3 EventWorldPosition;
  public Vector3 TileWorldPosition;
  public Vector2Int GridIndex;
  public PointerEventData.InputButton Button;
  public Camera EventCamera;
}
