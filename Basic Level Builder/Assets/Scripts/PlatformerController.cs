using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(PlatformerMover))]
public class PlatformerController : MonoBehaviour
{
  [System.Serializable]
  public class Events
  {
    public FacingEvent FacedRight;
    public FacingEvent FacedLeft;
  }

  public Events m_Events;

  PlatformerMover m_Mover;
  bool m_CanMove = true;
  bool m_CanJump = true;
  float m_InputAxis = 0;
  bool m_JumpRequested = false;
  Direction m_FacingDirection = Direction.RIGHT;


  private void Awake()
  {
    m_Mover = GetComponent<PlatformerMover>();
  }


  void Update()
  {
    GatherInput();
    HandleMovement();
    HandleJumping();
  }


  void GatherInput()
  {
    m_InputAxis = Input.GetAxis("Horizontal");
    m_JumpRequested = Input.GetButtonDown("Jump");
  }


  void HandleMovement()
  {
    if (!m_CanMove)
      return;

    HandleFacing();

    m_Mover.Move(m_InputAxis);
  }


  void HandleFacing()
  {
    if (m_InputAxis < 0 && m_FacingDirection != Direction.LEFT)
      FaceLeft();
    else if (m_InputAxis > 0 && m_FacingDirection != Direction.RIGHT)
      FaceRight();
  }


  void HandleJumping()
  {
    if (!m_CanJump)
      return;

    if (m_JumpRequested)
      m_Mover.AttemptJump();
  }


  void FaceLeft()
  {
    m_Events.FacedLeft.Invoke(new FacingEventData());
    m_FacingDirection = Direction.LEFT;
  }


  void FaceRight()
  {
    m_Events.FacedRight.Invoke(new FacingEventData());
    m_FacingDirection = Direction.RIGHT;
  }


  public void OnDirectionInitialized(TileDirectionEventData eventData)
  {
    m_FacingDirection = eventData.m_Direction;
  }


  public void OnDied(HealthEventData eventData)
  {
    m_CanMove = false;
    m_CanJump = false;
  }


  public void OnHeroReturned(HealthEventData eventData)
  {
    if (TryGetComponent(out TileDirection tileDirection))
      tileDirection.Initialize(tileDirection.Get());
      
    m_CanMove = true;
    m_CanJump = true;
  }
}

[System.Serializable]
public class FacingEvent : UnityEvent<FacingEventData> { }

public class FacingEventData
{
}
