/***************************************************
File:           EditorCameraController.cs
Authors:        Christopher Onorati
Last Updated:   6/17/2019
Last Version:   2019.1.4

Description:
  Script used to control the camera while in editor mode.

Copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/**
* CLASS NAME  : EditorCameraController
* DESCRIPTION : Allows the user the ability to move about the camera while in editor mode.
**/
public class EditorCameraController : MonoBehaviour
{
  /************************************************************************************/

  public float m_MinSpeed = 6;
  public float m_MaxSpeed = 30;
  public float m_Acceleration = 3;

  public float m_ZoomSpeedDivisor = 100;
  public float m_WheelSpeed = 10;

  [Tooltip("Audio clips that can be played when the camera is lerped.")]
  public AudioClip[] m_LerpAudioClips;

  public TileGrid m_TileGrid;

  //Transform of this game object.
  Transform m_Transform;

  //Camera of this game object.
  Camera m_Camera;

  //Audio source of this game object.
  AudioSource m_AudioSource;

  //Tools palette.
  ToolsPalette m_ToolsPalette;

  float m_InitialSize;
  float m_ZoomInput = 0;
  Vector3 m_InitialPosition;
  readonly float m_ResetDuration = 0.25f;
  public float m_MinZoomInput = -150;
  public float m_MaxZoomInput = 150;
  float m_Speed;
  Vector2 m_DragStartPosition;

  public UnityEvent Zoomed;

  /**
  * FUNCTION NAME: Start
  * DESCRIPTION  : Caches components.
  * INPUTS       : None
  * OUTPUTS      : None
  **/
  void Start()
  {
    m_Transform = GetComponent<Transform>();
    m_Camera = GetComponent<Camera>();
    m_AudioSource = GetComponent<AudioSource>();
    m_ToolsPalette = FindObjectOfType<ToolsPalette>();

    m_InitialSize = m_Camera.orthographicSize;
    m_InitialPosition = m_Transform.position;

    m_Speed = m_MinSpeed;
  }

  /**
  * FUNCTION NAME: FixedUpdate
  * DESCRIPTION  : Manages movement of the camera while in editor mode.
  * INPUTS       : None
  * OUTPUTS      : None
  **/
  void FixedUpdate()
  {
    //Do nothing if in play mode.
    if (GlobalData.IsInPlayMode())
      return;

    if (HotkeyMaster.s_HotkeysEnabled)
      HandleMovement();
  }


  public void OnScroll(PointerEventData e)
  {
    var delta = e.scrollDelta.y * m_WheelSpeed;
    Zoom(-delta);
  }


  public void StartDragMove(Vector2 startPosition)
  {
    m_DragStartPosition = startPosition;
  }


  public void DragMove(Vector2 newPosition)
  {
    var startPosition = m_Camera.ScreenToWorldPoint(m_DragStartPosition);
    var currentPosition = m_Camera.ScreenToWorldPoint(newPosition);
    var difference = startPosition - currentPosition;
    Move(difference);

    m_DragStartPosition = newPosition;
  }


  public void EndDragMove(Vector2 endPosition)
  {

  }


  public void StartDragZoom(Vector2 startPosition)
  {
    m_DragStartPosition = startPosition;
  }


  public void DragZoom(Vector2 newPosition)
  {
    var yDelta = newPosition.y - m_DragStartPosition.y;
    Zoom(yDelta);

    m_DragStartPosition = newPosition;
  }


  public void EndDragZoom(Vector2 endPosition)
  {

  }


  void Move(Vector3 delta)
  {
    m_Transform.Translate(delta);

    var nonZeroInput = delta.sqrMagnitude > 0;

    if (nonZeroInput /* && !m_ToolsPalette.IsHandActive() */)
      UpdateActiveTool();
  }


  void Zoom(float delta)
  {
    m_ZoomInput = Mathf.Clamp(m_ZoomInput + delta, m_MinZoomInput, m_MaxZoomInput);

    var @base = 1.1f;
    var exponent = m_ZoomInput / m_ZoomSpeedDivisor;
    var newSize = m_InitialSize * Mathf.Pow(@base, exponent);
    m_Camera.orthographicSize = newSize;

    if (delta != 0)
    {
      //m_TileGrid.SetUpOutline();
      Zoomed.Invoke();
    }
  }


  void ResetCamera()
  {
    var seq = ActionMaster.Actions.Sequence();
    var grp = seq.Group();
    grp.Move(gameObject, m_InitialPosition, m_ResetDuration, new Ease(Ease.Quad.InOut));
    grp.OrthographicZoom(gameObject, m_InitialSize, m_ResetDuration, new Ease(Ease.Quad.InOut));
    seq.Call(EndCameraReset, gameObject);
  }


  void EndCameraReset()
  {
    m_ZoomInput = 0;
  }


  /**
  * FUNCTION NAME: CameraMovement
  * DESCRIPTION  : Manage movement of the camera.
  * INPUTS       : None
  * OUTPUTS      : None
  **/
  public void HandleMovement()
  {
    if (Input.GetButtonDown("ResetCamera"))
      ResetCamera();

    var input = Vector2.zero;
    input.x = Input.GetAxis("Horizontal");
    input.y = Input.GetAxis("Vertical");

    var zeroInput = input.sqrMagnitude == 0;

    if (zeroInput)
      m_Speed = m_MinSpeed;

    var movement = input * m_Speed * Time.deltaTime;
    Move(movement);

    if (!zeroInput)
      m_Speed = Mathf.Clamp(m_Speed + m_Acceleration * Time.deltaTime, m_MinSpeed, m_MaxSpeed);
  }


  /**
  * FUNCTION NAME: UpdateActiveTool
  * DESCRIPTION  : Update the active tool due to the camera moving in some way.
  * INPUTS       : None
  * OUTPUTS      : None
  **/
  void UpdateActiveTool()
  {


    ToolEvent te = new ToolEvent();

    //Button setting.
    if (Input.GetMouseButton((int)PointerEventData.InputButton.Left))
      te.Button = PointerEventData.InputButton.Left;
    else if (Input.GetMouseButton((int)PointerEventData.InputButton.Right))
      te.Button = PointerEventData.InputButton.Right;
    else
      return;

    te.EventScreenPosition = Input.mousePosition;
    Vector3 eventScreenPositionVector3 = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
    te.EventWorldPosition = m_Camera.ScreenToWorldPoint(eventScreenPositionVector3);
    int xInt = Mathf.RoundToInt(te.EventWorldPosition.x);
    int yInt = Mathf.RoundToInt(te.EventWorldPosition.y);
    te.GridIndex = new Vector2Int(xInt, yInt);
    te.TileWorldPosition = new Vector3(te.GridIndex.x, te.GridIndex.y, 0);

    if (te.Button == PointerEventData.InputButton.Left)
      m_ToolsPalette.UpdateLeftDrag(te);
    else if (te.Button == PointerEventData.InputButton.Right)
      m_ToolsPalette.UpdateRightDrag(te);
  }
}
