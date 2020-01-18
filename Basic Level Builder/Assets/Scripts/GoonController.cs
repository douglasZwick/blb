using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlatformerMover))]
[RequireComponent(typeof(TileDirection))]
public class GoonController : MonoBehaviour
{
  [System.Serializable]
  public class Events
  {
    public FacingEvent FacedRight;
    public FacingEvent FacedLeft;
  }

  enum State
  {
    Moving,
    Waiting,
    Ramping,
  }

  public float m_WaitDuration = 0.1f;
  public float m_RampDuration = 0.5f;
  public Events m_Events;

  PlatformerMover m_Mover;
  TileDirection m_TileDirection;
  float m_InputAxis = 0;
  Direction m_FacingDirection = Direction.RIGHT;
  State m_State = State.Moving;
  float m_Timer;
  bool m_CanMove = true;
  List<Collider2D> m_FootContacts = new List<Collider2D>();
  List<Collider2D> m_FrontFacingContacts = new List<Collider2D>();
  List<Collider2D> m_BackFacingContacts = new List<Collider2D>();


  public void OnFootColliderEnter(Collider2D collider)
  {
    m_FootContacts.Add(collider);
  }


  public void OnFootColliderExit(Collider2D collider)
  {
    m_FootContacts.Remove(collider);
  }


  public void OnFrontFacingColliderEnter(Collider2D collider)
  {
    m_FrontFacingContacts.Add(collider);
  }


  public void OnFrontFacingColliderExit(Collider2D collider)
  {
    m_FrontFacingContacts.Remove(collider);
  }


  public void OnBackFacingColliderEnter(Collider2D collider)
  {
    m_BackFacingContacts.Add(collider);
  }


  public void OnBackFacingColliderExit(Collider2D collider)
  {
    m_BackFacingContacts.Remove(collider);
  }


  private void Awake()
  {
    m_Mover = GetComponent<PlatformerMover>();
    m_TileDirection = GetComponent<TileDirection>();

    BeginRamping();
  }


  private void Update()
  {
    var dt = Time.deltaTime;

    switch (m_State)
    {
      case State.Moving:
        GatherInput();
        HandleMovement();
        CheckContacts();
        break;

      case State.Ramping:
        Ramp(dt);

        GatherInput();
        HandleMovement();
        CheckContacts();
        break;

      default:
        Wait(dt);

        GatherInput();
        HandleMovement();
        break;
    }
  }


  void GatherInput()
  {
    var input = m_FacingDirection == Direction.RIGHT ? 1.0f : -1.0f;

    if (m_State == State.Moving)
      m_InputAxis = input;
    else if (m_State == State.Ramping)
      m_InputAxis = input * m_Timer / m_RampDuration;
    else //  m_State == State.Waiting
      m_InputAxis = 0;
  }


  void HandleMovement()
  {
    if (!m_CanMove)
      return;

    m_Mover.Move(m_InputAxis);
  }


  void CheckContacts()
  {
    var contacts = m_FrontFacingContacts;
    if (m_FacingDirection == Direction.LEFT)
      contacts = m_BackFacingContacts;

    if (contacts.Count == 0 || m_FootContacts.Count == 0)
      return;

    foreach (var contact in contacts)
    {
      if (!m_FootContacts.Contains(contact))
      {
        BeginWaiting();
        break;
      }
    }
  }


  void BeginWaiting()
  {
    m_State = State.Waiting;
    m_Timer = 0;
  }


  void Wait(float dt)
  {
    m_Timer += dt;

    if (m_Timer >= m_WaitDuration)
      EndWaiting();
  }


  void EndWaiting()
  {
    TurnAround();
    BeginRamping();
  }


  void BeginRamping()
  {
    m_State = State.Ramping;
    m_Timer = 0;
  }


  void Ramp(float dt)
  {
    m_Timer += dt;

    if (m_Timer >= m_RampDuration)
      EndRamping();
  }


  void EndRamping()
  {
    BeginMoving();
  }


  void BeginMoving()
  {
    m_State = State.Moving;
  }


  void TurnAround()
  {
    if (m_FacingDirection == Direction.RIGHT)
      FaceLeft();
    else
      FaceRight();
  }


  void FaceLeft()
  {
    m_FacingDirection = Direction.LEFT;

    m_Events.FacedLeft.Invoke(new FacingEventData());
    m_TileDirection.Set(m_FacingDirection);
  }


  void FaceRight(bool snap = false)
  {
    m_FacingDirection = Direction.RIGHT;

    m_Events.FacedRight.Invoke(new FacingEventData());
    m_TileDirection.Set(m_FacingDirection);
  }


  public void OnDirectionInitialized(TileDirectionEventData eventData)
  {
    m_FacingDirection = eventData.m_Direction;
  }
}
