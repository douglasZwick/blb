using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
public class PlatformerMover : MonoBehaviour
{
  public float m_MovementSpeed = 8;
  public float m_JumpSpeed = 20;
  public float m_StompBounceSpeed = 10;
  public float m_TerminalVelocity = 15;
  public Collider2D m_FeetCollider;

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
  }

  public Events m_Events;

  Transform m_Transform;
  Rigidbody2D m_Rigidbody;
  float m_PreviousX;
  bool m_Grounded = true;
  bool m_Rising = false;
  bool m_Boosting = false;
  float m_BoostTimer = 0;
  float m_LatestBoostDelay;
  float m_LatestBoostFullDuration;
  readonly float m_BoostWallDetectionEpsilon = 0.001f;


  private void Awake()
  {
    m_Transform = transform;
    m_Rigidbody = GetComponent<Rigidbody2D>();

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


  public void Move(float input)
  {
    if (!enabled)
      return;

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
      var velocity = m_Rigidbody.velocity;
      velocity.x = input * m_MovementSpeed;
      m_Rigidbody.velocity = velocity;

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
}
