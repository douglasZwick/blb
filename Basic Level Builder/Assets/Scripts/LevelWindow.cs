using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LevelWindow : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler,
  IDragHandler, IEndDragHandler, IScrollHandler
{
  public Camera m_Camera;
  public ToolsPalette m_ToolsPalette;

  EditorCameraController m_CameraController;


  private void Awake()
  {
    m_CameraController = m_Camera.GetComponent<EditorCameraController>();
  }


  public void OnPointerDown(PointerEventData e)
  {
    if (GlobalData.AreEffectsUnderway())
      return;

    var te = PrepareEvent(e);
    //m_ToolsPalette.OnPointerDown(te);

    var leftAlt = Input.GetKey(KeyCode.LeftAlt);
    var rightAlt = Input.GetKey(KeyCode.RightAlt);
    var alt = leftAlt || rightAlt;

    if (alt)
      return;

    switch (e.button)
    {
      case PointerEventData.InputButton.Left:
        m_ToolsPalette.OnLeftPointerDown(te);
        break;
      case PointerEventData.InputButton.Right:
        m_ToolsPalette.OnRightPointerDown(te);
        break;
      case PointerEventData.InputButton.Middle:
        // *this* particular function handler probably
        // won't care about this, but the drag one
        // will want to handle camera stuff
        break;
    }
  }

  public void OnPointerUp(PointerEventData e)
  {
    if (GlobalData.AreEffectsUnderway())
      return;

    var te = PrepareEvent(e);
    //m_ToolsPalette.OnPointerUp(te);

    var leftAlt = Input.GetKey(KeyCode.LeftAlt);
    var rightAlt = Input.GetKey(KeyCode.RightAlt);
    var alt = leftAlt || rightAlt;

    if (alt)
      return;

    switch (e.button)
    {
      case PointerEventData.InputButton.Left:
        m_ToolsPalette.OnLeftPointerUp(te);
        break;
      case PointerEventData.InputButton.Right:
        m_ToolsPalette.OnRightPointerUp(te);
        break;
      case PointerEventData.InputButton.Middle:
        // *this* particular function handler probably
        // won't care about this, but the drag one
        // will want to handle camera stuff
        break;
    }
  }

  public void OnBeginDrag(PointerEventData e)
  {
    if (GlobalData.AreEffectsUnderway())
      return;

    var te = PrepareEvent(e);
    //m_ToolsPalette.OnBeginDrag(te);

    var leftAlt = Input.GetKey(KeyCode.LeftAlt);
    var rightAlt = Input.GetKey(KeyCode.RightAlt);
    var alt = leftAlt || rightAlt;

    if (alt)
    {
      m_CameraController.StartDragZoom(e.position);

      return;
    }

    switch (e.button)
    {
      case PointerEventData.InputButton.Left:
        m_ToolsPalette.OnBeginLeftDrag(te);
        break;
      case PointerEventData.InputButton.Right:
        m_ToolsPalette.OnBeginRightDrag(te);
        break;
      case PointerEventData.InputButton.Middle:
        m_CameraController.StartDragMove(e.position);
        break;
    }
  }

  public void OnDrag(PointerEventData e)
  {
    if (GlobalData.AreEffectsUnderway())
      return;

    var te = PrepareEvent(e);
    //m_ToolsPalette.OnDrag(te);

    var leftAlt = Input.GetKey(KeyCode.LeftAlt);
    var rightAlt = Input.GetKey(KeyCode.RightAlt);
    var alt = leftAlt || rightAlt;

    if (alt)
    {
      m_CameraController.DragZoom(e.position);

      return;
    }

    switch (e.button)
    {
      case PointerEventData.InputButton.Left:
        m_ToolsPalette.OnLeftDrag(te);
        break;
      case PointerEventData.InputButton.Right:
        m_ToolsPalette.OnRightDrag(te);
        break;
      case PointerEventData.InputButton.Middle:
        m_CameraController.DragMove(e.position);
        break;
    }
  }


  public void OnEndDrag(PointerEventData e)
  {
    if (GlobalData.AreEffectsUnderway())
      return;

    var te = PrepareEvent(e);
    //m_ToolsPalette.OnEndDrag(te);

    var leftAlt = Input.GetKey(KeyCode.LeftAlt);
    var rightAlt = Input.GetKey(KeyCode.RightAlt);
    var alt = leftAlt || rightAlt;

    if (alt)
    {
      m_CameraController.EndDragZoom(e.position);
    }

    switch (e.button)
    {
      case PointerEventData.InputButton.Left:
        m_ToolsPalette.OnEndLeftDrag(te);
        break;
      case PointerEventData.InputButton.Right:
        m_ToolsPalette.OnEndRightDrag(te);
        break;
      case PointerEventData.InputButton.Middle:
        m_CameraController.EndDragMove(e.position);
        break;
    }
  }


  public void OnScroll(PointerEventData e)
  {
    if (GlobalData.AreEffectsUnderway())
      return;

    m_CameraController.OnScroll(e);
  }


  private ToolEvent PrepareEvent(PointerEventData e)
  {
    var eventScreenPosition = e.position;
    var eventScreenPositionVector3 = new Vector3(e.position.x, e.position.y, 0);
    var eventWorldPosition = m_Camera.ScreenToWorldPoint(eventScreenPositionVector3);
    var xInt = Mathf.RoundToInt(eventWorldPosition.x);
    var yInt = Mathf.RoundToInt(eventWorldPosition.y);
    var gridIndex = new Vector2Int(xInt, yInt);
    var tileWorldPosition = new Vector3(gridIndex.x, gridIndex.y, 0);

    var te = new ToolEvent();
    te.EventScreenPosition = eventScreenPosition;
    te.EventWorldPosition = eventWorldPosition;
    te.TileWorldPosition = tileWorldPosition;
    te.GridIndex = gridIndex;
    te.Button = e.button;
    te.EventCamera = m_Camera;

    return te;
  }
}
