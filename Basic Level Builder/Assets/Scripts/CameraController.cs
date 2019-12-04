using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
  [Range(0, 1)]
  public float m_Snappiness = 0.1f;
  [Range(0, 1)]
  public float m_VelocityOffsetSnappiness = 0.1f;
  public float m_OffsetMultiplier = 0.4f;
  public Vector3 m_RightFacingOffset = new Vector3(6, 2, -10);
  public float m_FacingOffsetChangeDuration = 2;
  public Vector2 m_MinBoundsMargin = new Vector2(-5, -5);
  public Vector2 m_MaxBoundsMargin = new Vector2(5, 5);
  public float m_ActionZoomSize = 8;
  public float m_ActionZoomInDuration = 0.05f;
  public float m_ActionZoomOutDuration = 0.5f;
  public Transform m_FinalFocusPointTransform;
  public Transform m_FacingFocusPointTransform;

  [System.Serializable]
  public class Events
  {
    public CameraControllerEvent FoundTarget;
  }

  public Events m_Events;

  [HideInInspector]
  public Vector3 m_Offset;
  Vector3 m_VelocityOffset = Vector3.zero;

  Transform m_Transform;
  Camera m_Camera;
  CameraTarget m_Target;
  TileDirection m_TargetDirection;
  TileGrid m_TileGrid;
  float m_CameraZ;
  bool m_FacingLeft;
  Vector3 m_LeftFacingOffset;
  ActionSequence m_OffsetMoveSequence;
  ActionSequence m_ActionZoomSequence;
  float m_DefaultSize;
  bool m_ShouldTrackTarget = true;

  bool CanActionZoom { get { return true; } }


  void Awake()
  {
    m_Transform = transform;
    m_Camera = GetComponent<Camera>();
    m_TileGrid = FindObjectOfType<TileGrid>();

    m_CameraZ = transform.position.z;
    m_Offset = m_RightFacingOffset;
    m_LeftFacingOffset = m_RightFacingOffset;
    m_LeftFacingOffset.x = -m_LeftFacingOffset.x;

    m_OffsetMoveSequence = ActionMaster.Actions.Sequence();
    m_ActionZoomSequence = ActionMaster.Actions.Sequence();

    m_DefaultSize = m_Camera.orthographicSize;

    FindTarget();

    GlobalData.HeroDied += OnHeroDied;
    GlobalData.HeroReturned += OnHeroReturned;
    GlobalData.PlayModeToggled += OnPlayModeToggled;
  }


  void FixedUpdate()
  {
    if (!m_ShouldTrackTarget)
      return;

    if (m_Target == null)
      FindTarget();
    else
      TrackTarget();
  }


  public void AttemptActionZoom()
  {
    if (CanActionZoom)
      ActionZoom();
  }


  public void StartTracking()
  {
    m_ShouldTrackTarget = true;
  }


  public void StopTracking()
  {
    m_ShouldTrackTarget = false;
  }


  void ActionZoom()
  {
    CancelActionZoom();
    m_ActionZoomSequence.OrthographicZoom(gameObject, m_ActionZoomSize, m_ActionZoomInDuration, new Ease(Ease.Quad.Out));
    m_ActionZoomSequence.OrthographicZoom(gameObject, m_DefaultSize, m_ActionZoomOutDuration, new Ease(Ease.Quad.InOut));
  }


  void FindTarget()
  {
    CancelOffsetMove();

    m_Target = FindObjectOfType<CameraTarget>();

    if (m_Target != null)
    {
      m_TargetDirection = m_Target.GetComponent<TileDirection>();
      m_FacingLeft = m_TargetDirection.m_Direction == Direction.LEFT;
      m_Offset = m_FacingLeft ? m_LeftFacingOffset : m_RightFacingOffset;

      var eventData = new CameraControllerEventData()
      {
        m_CameraTarget = m_Target,
      };

      m_Events.FoundTarget.Invoke(eventData);
    }
  }

  
  void TrackTarget()
  {
    var targetIsFacingLeft = m_TargetDirection.m_Direction == Direction.LEFT;

    if      (targetIsFacingLeft && !m_FacingLeft)
      FaceLeft();
    else if (!targetIsFacingLeft && m_FacingLeft)
      FaceRight();

    ComputeVelocityOffset();

    var cameraPosition = m_Transform.position;
    var targetPosition = m_Target.m_Transform.position;
    var totalOffset = m_Offset + m_VelocityOffset;
    var cameraDestination = targetPosition + totalOffset;
    var facingFocusPointPosition = targetPosition + m_Offset;
    facingFocusPointPosition.z = 0;
    var finalFocusPointPosition = cameraDestination;
    finalFocusPointPosition.z = 0;
    m_FacingFocusPointTransform.position = facingFocusPointPosition;
    m_FinalFocusPointTransform.position = finalFocusPointPosition;
    var snappiness = m_Snappiness;
    var newPosition = Vector3.Lerp(cameraPosition, cameraDestination, snappiness);
    newPosition -= targetPosition;
    var cameraSize = m_Camera.orthographicSize;
    var clampedHeight = cameraSize * 5.0f / 6.0f;
    var clampedWidth = clampedHeight * 16.0f / 9.0f;
    newPosition.x = Mathf.Clamp(newPosition.x, -clampedWidth, clampedWidth);
    newPosition.y = Mathf.Clamp(newPosition.y, -clampedHeight, clampedHeight);
    newPosition += targetPosition;
    newPosition.z = m_CameraZ;

    var minX = m_TileGrid.m_MinBounds.x - m_MinBoundsMargin.x;
    var maxX = m_TileGrid.m_MaxBounds.x + m_MaxBoundsMargin.x;
    var minY = m_TileGrid.m_MinBounds.y - m_MinBoundsMargin.y;
    var maxY = m_TileGrid.m_MaxBounds.y + m_MaxBoundsMargin.y;
    newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
    newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);

    m_Transform.position = newPosition;
  }


  void ComputeVelocityOffset()
  {
    var rb = m_Target.m_Rigidbody2D;
    if (rb == null)
      return;

    var velocity = rb.velocity;
    var newOffset = velocity * m_OffsetMultiplier;

    m_VelocityOffset = Vector3.Lerp(m_VelocityOffset, newOffset, m_VelocityOffsetSnappiness);
  }


  void FaceLeft()
  {
    m_FacingLeft = true;
    CancelOffsetMove();
    m_OffsetMoveSequence.ChangeCameraOffset(gameObject, m_LeftFacingOffset, m_FacingOffsetChangeDuration, new Ease(Ease.Quad.InOut));
  }


  void FaceRight()
  {
    m_FacingLeft = false;
    CancelOffsetMove();
    m_OffsetMoveSequence.ChangeCameraOffset(gameObject, m_RightFacingOffset, m_FacingOffsetChangeDuration, new Ease(Ease.Quad.InOut));
  }


  void CancelOffsetMove()
  {
    m_OffsetMoveSequence.Cancel();
    m_OffsetMoveSequence = ActionMaster.Actions.Sequence();
  }


  void CancelActionZoom()
  {
    m_ActionZoomSequence.Cancel();
    m_ActionZoomSequence = ActionMaster.Actions.Sequence();
  }


  void OnHeroDied()
  {
    StopTracking();
  }


  void OnHeroReturned()
  {
    StartTracking();
  }


  void OnPlayModeToggled(bool isInPlayMode)
  {
    if (!isInPlayMode)
      return;

    FindTarget();

    if (m_Target == null)
      return;

    var cameraPosition = transform.position;
    var newPosition = m_Target.transform.position + m_Offset;
    newPosition.z = m_CameraZ;

    m_Transform.position = newPosition;

    StartTracking();
  }


  private void OnDestroy()
  {
    GlobalData.HeroDied -= OnHeroDied;
    GlobalData.HeroReturned -= OnHeroReturned;
    GlobalData.PlayModeToggled -= OnPlayModeToggled;
  }
}


[System.Serializable]
public class CameraControllerEvent : UnityEvent<CameraControllerEventData> { }

public class CameraControllerEventData
{
  public CameraTarget m_CameraTarget;
}
