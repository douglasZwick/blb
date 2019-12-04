/***************************************************
File:           HandTool.cs
Authors:        Christopher Onorati
Last Updated:   6/10/2019
Last Version:   2019.1.4

Description:
  Literally does nothing.  Just detected by the
  CameraEditorController as the active tool.

Copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

public class HandTool : BlbTool
{
  public EditorCameraController m_CameraController;


  private void Start()
  {
    m_ToolID = Tools.HAND;
  }


  public override void BeginLeftDrag(ToolEvent te)
  {
    m_CameraController.StartDragMove(te.EventScreenPosition);
  }


  public override void BeginRightDrag(ToolEvent te)
  {
    m_CameraController.StartDragMove(te.EventScreenPosition);
  }


  public override void LeftDrag(ToolEvent te)
  {
    m_CameraController.DragMove(te.EventScreenPosition);
  }


  public override void RightDrag(ToolEvent te)
  {
    m_CameraController.DragMove(te.EventScreenPosition);
  }
}
