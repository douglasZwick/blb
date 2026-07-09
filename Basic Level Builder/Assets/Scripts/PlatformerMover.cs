using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlatformerMover : MonoBehaviour
{
  public float m_MovementSpeed = 8;
  public float m_JumpSpeed = 20;
  public float m_StompBounceSpeed = 10;
  public float m_TerminalVelocity = 15;
  public Collider2D m_FeetCollider;
  public LayerMask m_SlopeCheckLayerMask;
  public Vector2 m_LocalSnapOrigin = new(0, -0.5f);

  [System.Serializable]
  public class Events
  {
    public MovementEvent Moved;
    public MovementEvent MovedOnGround;
    public UnityEvent LeftGround;
    public UnityEvent Jumped;
    public UnityEvent ReachedApex;
    public UnityEvent Landed;
    public UnityEvent Boosted;
    public MovementEvent ChangedSlopeNormal;
  }

  public Events m_Events;

  static readonly Vector2 s_DefaultSlopeNormal = Vector2.up;

  Transform m_Transform;
  Rigidbody2D m_Rigidbody;
  Collider2D m_Collider;
  float m_PreviousX;
  bool m_Grounded = true;
  bool m_Rising = false;
  bool m_Boosting = false;
  float m_BoostTimer = 0;
  float m_LatestBoostDelay;
  float m_LatestBoostFullDuration;
  readonly float m_BoostWallDetectionEpsilon = 0.001f;
  Vector2 m_SlopeNormal = s_DefaultSlopeNormal;
  Vector2 m_SnapPoint;

  Vector3 WorldSnapOrigin => m_Transform.TransformPoint(m_LocalSnapOrigin);


  private void Awake()
  {
    m_Transform = transform;
    m_Rigidbody = GetComponent<Rigidbody2D>();
    m_Collider = GetComponent<Collider2D>();

    m_PreviousX = m_Transform.position.x;
    enabled = false;
    m_Rigidbody.isKinematic = true;

    GlobalData.ModeStarted += OnModeStarted;
  }


  public void OnModeStarted(bool isInPlayMode)
  {
    enabled = true;
    m_Rigidbody.isKinematic = false;
  }


  private void Update()
  {
    HandleGrounding();
    HandleRising();
    HandleBoosting();
    HandleGroundSnapping();
  }


  void HandleGrounding()
  {
    var grounded = m_FeetCollider.IsTouchingLayers();

    if      (grounded && !m_Grounded)
      Land();
    else if (!grounded && m_Grounded)
      LeaveGround();

    m_Grounded = grounded;
  }


  void HandleRising()
  {
    if (m_Rising && m_Rigidbody.velocity.y <= 0)
    {
      m_Rising = false;
      m_Events.ReachedApex.Invoke();
    }
  }


  void HandleBoosting()
  {
    if (!m_Boosting)
      return;

    var xVelocity = m_Rigidbody.velocity.x;

    // Early out if you hit a wall
    if (Mathf.Abs(xVelocity) < m_BoostWallDetectionEpsilon)
    {
      EndBoosting();
      return;
    }

    // Otherwise, we gotta check the boost timer
    m_BoostTimer += Time.deltaTime;

    // The "boost delay" is just the minimum boost duration, within which
    // the player cannot interrupt a horizontal boost with their input
    if (m_BoostTimer < m_LatestBoostDelay)
      return;

    if (m_Grounded || m_BoostTimer >= m_LatestBoostFullDuration)
      EndBoosting();
  }


  void HandleGroundSnapping()
  {
    if (!m_Grounded) return;
    
    // var snappedPosition = m_Transform.position;
    // snappedPosition.y = m_SnapPoint.y;
    // m_Transform.position = snappedPosition;
    
    var worldOffset = m_Transform.TransformVector(m_LocalSnapOrigin);
    var snapDestinationForTransform = (Vector3)m_SnapPoint - worldOffset;
    m_Transform.position = snapDestinationForTransform;

    RotateToNormal();
  }


  void RotateToNormal()
  {
    // var slopeAngle = Vector2.SignedAngle(s_DefaultSlopeNormal, m_SlopeNormal);
    // var eulerAngles = Vector3.forward * slopeAngle;
    // m_Transform.eulerAngles = eulerAngles;
    // m_Transform.RotateAround(WorldSnapOrigin, Vector3.forward, slopeAngle);
    m_Transform.rotation = Quaternion.FromToRotation(s_DefaultSlopeNormal, m_SlopeNormal);
  }


  void ResetRotation()
  {
    m_Transform.rotation = Quaternion.identity;
  }


  public void Move(float input)
  {
    if (!enabled)
      return;

    SlopeCheck();

    if (m_Boosting)
    {
      if (m_BoostTimer >= m_LatestBoostDelay)
      {
        // "Input alignment" is basically the 1D dot product of the X velocity
        // and the movement input
        var xVelocity = m_Rigidbody.velocity.x;
        var inputAlignment = xVelocity * input;

        if (inputAlignment < 0)
          EndBoosting();
      }
    }
    else  // if not boosting
    {
      // TODO:
      //   If this doesn't work, maybe try the following:
      //   1. Assume the character is on a slope.
      //      COUNTER-rotate their velocity to accommodate the slope normal.
      //   2. Overwrite the x component of the counter-rotated velocity with the input vector.
      //   3. Rotate the velocity back to undo the counter-rotation.
      //   4. Put the new velocity back in the rigid body.

      if (m_Grounded && !m_Rising)
      {
        var slopeAngle = Vector2.SignedAngle(s_DefaultSlopeNormal, m_SlopeNormal);
        var slopeQuat = Quaternion.AngleAxis(slopeAngle, Vector3.forward);
        var direction = slopeQuat * (input * Vector3.right);
        var newVelocity = direction * m_MovementSpeed;
        m_Rigidbody.velocity = newVelocity;
      }
      else
      {
        var velocity = m_Rigidbody.velocity;
        velocity.x = input * m_MovementSpeed;
        m_Rigidbody.velocity = velocity;
      }

      if (Mathf.Abs(input) > 0)
      {
        var deltaX = m_Transform.position.x - m_PreviousX;
        var currentSpeed = deltaX / Time.deltaTime;
        var normalizedDelta = currentSpeed / m_MovementSpeed;

        var eventData = new MovementEventData()
        {
          m_NormalizedDelta = normalizedDelta,
        };

        m_Events.Moved.Invoke(eventData);

        if (m_Grounded)
          m_Events.MovedOnGround.Invoke(eventData);
      }

      m_PreviousX = m_Transform.position.x;
    }
  }


  public void AttemptJump()
  {
    if (!enabled)
      return;

    if (m_Grounded)
      Jump();
  }


  void Jump()
  {
    var velocity = m_Rigidbody.velocity;
    velocity.y = m_JumpSpeed;
    m_Rigidbody.velocity = velocity;

    m_Rising = true;

    m_Events.Jumped.Invoke();
  }


  void Land()
  {
    m_Events.Landed.Invoke();
  }


  void LeaveGround()
  {
    m_Events.LeftGround.Invoke();
    SetSlopeNormal(s_DefaultSlopeNormal);
    ResetRotation();
  }


  public void OnStompedEnemy(HealthEventData eventData)
  {
    var velocity = m_Rigidbody.velocity;
    velocity.y = m_StompBounceSpeed;
    m_Rigidbody.velocity = velocity;
  }


  public void OnBoostTriggered(BoostLogicEventData eventData)
  {
    var direction = eventData.m_TileDirection.m_Direction;
    var boostSpeed = eventData.m_BoostLogic.m_Speed;
    var velocity = m_Rigidbody.velocity;
    var preventMovementInput = false;

    // You should only prevent movement input for
    // horizontal boosts
    switch (direction)
    {
      case Direction.RIGHT:
        velocity.x = boostSpeed;
        preventMovementInput = true;
        break;

      case Direction.UP:
        velocity.y = boostSpeed;
        break;

      case Direction.LEFT:
        velocity.x = -boostSpeed;
        preventMovementInput = true;
        break;

      case Direction.DOWN:
        velocity.y = -boostSpeed;
        break;
    }

    m_LatestBoostDelay = eventData.m_BoostLogic.m_Delay;
    m_LatestBoostFullDuration = eventData.m_BoostLogic.m_FullDuration;

    BeginBoosting(preventMovementInput);
    m_Rigidbody.velocity = velocity;
  }


  void BeginBoosting(bool preventMovementInput)
  {
    m_BoostTimer = 0;

    m_Events.Boosted.Invoke();

    if (preventMovementInput)
      m_Boosting = true;
  }


  void EndBoosting()
  {
    m_Boosting = false;
  }


  public void OnUsedTeleporter(TeleportEventData eventData)
  {
    m_PreviousX = eventData.m_ToPosition.x;
  }


  void SlopeCheck()
  {
    var rayStart = m_Transform.position;
    var rayDirection = -m_SlopeNormal;
    var results = new RaycastHit2D[1];
    var distance = 1.0f;
    var raycastResult = Physics2D.Raycast(rayStart, rayDirection, distance, ~m_SlopeCheckLayerMask);
    var sweepHits = m_Collider.Cast(rayDirection, results, distance);
    // TODO:
    //   Here's where my mind was going with all this:
    //   - sweep downward (or really, in the opposite direction of the slope normal)
    //   - probably only need one hit in the results array
    //   - do pretty much everything else the same as before, so that now the hit point ends up
    //     being maybe the corner of the collider rather than near the center of the transform, etc.
    //   From here, things should all kinda more or less roll out the same way as how they work now
    
    if (raycastResult.collider == null) return;

    SetSlopeNormal(raycastResult.normal);
    SetSnapPoint(raycastResult.point);
  }


  void SetSlopeNormal(Vector2 newNormal)
  {
    if (m_SlopeNormal == newNormal) return;  // operator == uses approximation! :o

    var movementEventData = new MovementEventData
    {
      m_OldNormal = m_SlopeNormal,
      m_NewNormal = newNormal
    };

    m_SlopeNormal = newNormal;

    m_Events.ChangedSlopeNormal.Invoke(movementEventData);
  }


  void SetSnapPoint(Vector2 newPoint)
  {
    // TODO:
    //   There may be some reason to dispatch an event here, but I can't think of what it might be

    m_SnapPoint = newPoint;
  }


  void OnChangedSlopeNormal(MovementEventData movementEventData)
  {
    // TODO:
    //   Move this to a new component maybe? Probably?

    // TODO:
    //   Hook this event up. Then implement this callback to rotate the graphicals
  }
  

  private void OnDestroy()
  {
    GlobalData.ModeStarted -= OnModeStarted;
  }
}


[System.Serializable]
public class MovementEvent : UnityEvent<MovementEventData> { }

public class MovementEventData
{
  public float m_NormalizedDelta;
  public Vector2 m_OldNormal;
  public Vector2 m_NewNormal;
}
