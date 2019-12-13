using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EyesController))]
[RequireComponent(typeof(CameraTarget))]
public class HeroController : MonoBehaviour
{
  // TODO: factor out the squash logic into a SquashController or something like that
  public BoxCollider2D m_FeetCollider;
  public BoxCollider2D m_FrontSensor;
  public BoxCollider2D m_BackSensor;
  public Transform m_LandingSquashNode;
  public Transform m_FrontSquashNode;
  public Transform m_BackSquashNode;
  public Transform m_HopNode;
  public SpriteRenderer m_BodySpriteRenderer;
  public float m_MovementSpeed = 8;
  public float m_JumpSpeed = 20;
  public float m_TerminalVelocity = 15;
  public float m_KillHeight = -20.0f;
  public float m_BoostDelay = 0.25f;
  public float m_BoostFullDuration = 0.5f;
  public bool m_FacingLeft = false;
  public bool m_StartGrounded = true;
  public Vector3 m_LandingSquashScale = new Vector3(1.5f, 0.5f, 1.0f);
  public float m_LandingSquashDuration = 0.25f;
  public Vector3 m_FrontSquashScale = new Vector3(0.5f, 1.5f, 1.0f);
  public float m_MaxSquashFactor = 2;
  public float m_FrontSquashDuration = 0.25f;
  public float m_HopHeight = 0.25f;
  public float m_HopDuration = 0.25f;
  public float m_DeathVelocity = 10;
  public float m_DeathAngularVelocity = 360;
  public float m_DeathPreDelay = 0.5f;
  public float m_DeathPostDelay = 0.25f;

  TileGrid m_TileGrid;
  Rigidbody2D m_Rigidbody2D;
  EyesController m_EyesController;
  CameraController m_CameraController;
  Vector3 m_CheckpointPosition;
  Checkpoint m_LatestCheckpoint;
  bool m_CanMove = true;
  bool m_CanJump = true;
  bool m_Grounded;
  bool m_FrontTouching = false;
  bool m_BackTouching = false;
  bool m_Dead = false;
  float m_DefaultGravity;
  Color m_DefaultColor;

  ActionSequence m_LandingSquashSequence;
  ActionSequence m_FrontSquashSequence;
  ActionSequence m_BackSquashSequence;
  ActionSequence m_DeathSequence;

  ActionSequence m_HopSequence;
  float m_HopDefaultY;

  bool m_Rising = false;
  float m_PreviousX;
  bool m_Boosting = false;
  float m_BoostTimer = 0;
  float m_InputAxis = 0;

  /************************************************************************************/

  [Tooltip("Velocity a booster will set the Hero's rigidbody to in the X or Y axis.")]
  public float m_BoostSpeed = 40.0f;

  /************************************************************************************/

  //Last teleport destination.  Used to determine when teleporting is valid again.
  GameObject m_LastTeleportDestination = null;

  //List of keys.
  List<TileColor> m_CollectedKeys = new List<TileColor>();

  /************************************************************************************/

  //Transform of the Hero.
  Transform m_Transform;

  void Awake()
  {
    m_TileGrid = FindObjectOfType<TileGrid>();
    m_CameraController = FindObjectOfType<CameraController>();
    m_Transform = GetComponent<Transform>();
    m_Rigidbody2D = GetComponent<Rigidbody2D>();
    m_EyesController = GetComponent<EyesController>();

    m_CheckpointPosition = transform.position;

    if (m_FacingLeft)
      FaceLeft();

    m_Grounded = m_StartGrounded;
    m_LandingSquashSequence = ActionMaster.Actions.Sequence();
    m_FrontSquashSequence = ActionMaster.Actions.Sequence();
    m_BackSquashSequence = ActionMaster.Actions.Sequence();
    m_DeathSequence = ActionMaster.Actions.Sequence();
    m_HopSequence = ActionMaster.Actions.Sequence();
    m_HopDefaultY = m_HopNode.localPosition.y;
    m_PreviousX = transform.position.x;
    m_DefaultGravity = m_Rigidbody2D.gravityScale;
    m_DefaultColor = m_BodySpriteRenderer.color;

    GlobalData.PlayModePreToggle += OnPlayModePreToggle;
  }


  void Update()
  {
    if (Input.GetKey(KeyCode.U))
      m_EyesController.m_LookAngle += 600 * Time.deltaTime;
    if (Input.GetKey(KeyCode.I))
      m_EyesController.m_LookAngle -= 600 * Time.deltaTime;

    GatherInput();
    HandleGrounding();
    HandleFrontAndBackSquashing();
    HandleMovement();
    HandleJumping();
    HandleBoosting();
    BoundsCheck();
    ClampVelocity();

    m_PreviousX = transform.position.x;
  }


  void OnPlayModePreToggle(bool isInPlayMode)
  {
    if (!isInPlayMode)
      m_DeathSequence.Cancel();
  }


  void OnCollisionEnter2D(Collision2D collision)
  {
    if (m_Dead)
      return;

    if (collision.collider.CompareTag("Deadly"))
      Die();
    else if (collision.collider.CompareTag("Tile_Goal"))
      Win();
    else if (collision.collider.CompareTag("Door"))
      CheckOpenDoor(collision.gameObject);
  }


  void OnTriggerEnter2D(Collider2D collision)
  {
    if (m_Dead)
      return;

    if (collision.CompareTag("Checkpoint"))
      UpdateCheckpoint(collision.gameObject);
    else if (collision.CompareTag("Deadly"))
      Die();
    else if (collision.CompareTag("Tile_Goal"))
      Win();
    else if (collision.CompareTag("Coin"))
      CollectCoin(collision.gameObject);
    else if (collision.CompareTag("Key"))
      CollectKey(collision.gameObject);
    else if (collision.CompareTag("Switch"))
      ActivateSwitch(collision.gameObject);
    else if (collision.CompareTag("Teleporter"))
      Teleport(collision.gameObject);
    else if (collision.CompareTag("Booster"))
      ActivateBooster(collision.gameObject);
  }

  /**
  * FUNCTION NAME: OnTriggerExit2D
  * DESCRIPTION  : Detect collision exit with teleporters.
  * INPUTS       : collision - Collider that we stopped colliding with.
  * OUTPUTS      : None
  **/
  void OnTriggerExit2D(Collider2D collision)
  {
    //Enable teleportation again after leaving the hit box of the destination.
    if (collision.gameObject == m_LastTeleportDestination)
      m_LastTeleportDestination = null;
  }


  public void SetDirection(Direction direction)
  {
    if      (direction == Direction.RIGHT)
      FaceRight(snap: true);
    else if (direction == Direction.LEFT)
      FaceLeft(snap: true);
  }


  void GatherInput()
  {
    m_InputAxis = Input.GetAxis("Horizontal");
  }


  void HandleGrounding()
  {
    var grounded = m_FeetCollider.IsTouchingLayers();

    if      (grounded && !m_Grounded)
    {
      Land();
    }
    else if (!grounded && m_Grounded)
    {
      LeaveGround();
    }

    m_Grounded = grounded;
  }


  void HandleFrontAndBackSquashing()
  {
    var frontTouching = m_FrontSensor.IsTouchingLayers();
    var backTouching = m_BackSensor.IsTouchingLayers();

    var xDelta = Mathf.Abs(transform.position.x - m_PreviousX);
    var normalizedDelta = (xDelta / Time.deltaTime) / m_MovementSpeed;
    var clampedDelta = Mathf.Clamp(normalizedDelta, 0, m_MaxSquashFactor);
    var squashScale = Vector3.LerpUnclamped(Vector3.one, m_FrontSquashScale, clampedDelta);
    var duration = (m_FrontSquashDuration / 2) * clampedDelta;

    if (frontTouching && !m_FrontTouching && !m_FrontSquashSequence.Active)
    {
      m_FrontSquashSequence = ActionMaster.Actions.Sequence();
      m_FrontSquashSequence.Scale(m_FrontSquashNode.gameObject, squashScale, duration, new Ease(Ease.Quad.Out));
      m_FrontSquashSequence.Scale(m_FrontSquashNode.gameObject, Vector3.one, duration, new Ease(Ease.Quad.In));
    }

    if (backTouching && !m_BackTouching && !m_BackSquashSequence.Active)
    {
      m_BackSquashSequence = ActionMaster.Actions.Sequence();
      m_BackSquashSequence.Scale(m_BackSquashNode.gameObject, squashScale, duration, new Ease(Ease.Quad.Out));
      m_BackSquashSequence.Scale(m_BackSquashNode.gameObject, Vector3.one, duration, new Ease(Ease.Quad.In));
    }

    m_FrontTouching = frontTouching;
    m_BackTouching = backTouching;
  }


  void HandleMovement()
  {
    var canMove = m_CanMove && !m_Boosting;

    if (!canMove)
      return;

    var movementInput = m_InputAxis;

    var grounded = m_Grounded;
    var xDelta = Mathf.Abs(transform.position.x - m_PreviousX);
    var noActiveSequences = !m_LandingSquashSequence.Active && !m_FrontSquashSequence.Active && !m_HopSequence.Active;

    if (grounded && xDelta > 0 && noActiveSequences)
    {
      var normalizedDelta = (xDelta / Time.deltaTime) / m_MovementSpeed;
      var height = m_HopHeight * normalizedDelta;
      var duration = (m_HopDuration / 2) * Mathf.Sqrt(normalizedDelta);

      m_HopSequence = ActionMaster.Actions.Sequence();
      m_HopSequence.MoveLocalY(m_HopNode.gameObject, m_HopDefaultY + height, duration, new Ease(Ease.Quad.Out));
      m_HopSequence.MoveLocalY(m_HopNode.gameObject, m_HopDefaultY, duration, new Ease(Ease.Quad.In));
    }

    HandleFacing(movementInput);

    var velocity = m_Rigidbody2D.velocity;
    velocity.x = movementInput * m_MovementSpeed;
    m_Rigidbody2D.velocity = velocity;
  }


  void HandleFacing(float movementInput)
  {
    if      (movementInput < 0 && !m_FacingLeft)
      FaceLeft();
    else if (movementInput > 0 && m_FacingLeft)
      FaceRight();
  }


  void FaceLeft(bool snap = false)
  {
    m_FacingLeft = true;
    m_EyesController.FaceLeft(snap);

    var frontSensor = m_FrontSensor;
    m_FrontSensor = m_BackSensor;
    m_BackSensor = frontSensor;
    var frontNode = m_FrontSquashNode;
    m_FrontSquashNode = m_BackSquashNode;
    m_BackSquashNode = frontNode;
  }


  void FaceRight(bool snap = false)
  {
    m_FacingLeft = false;
    m_EyesController.FaceRight(snap);

    var frontSensor = m_FrontSensor;
    m_FrontSensor = m_BackSensor;
    m_BackSensor = frontSensor;
    var frontNode = m_FrontSquashNode;
    m_FrontSquashNode = m_BackSquashNode;
    m_BackSquashNode = frontNode;
  }


  void HandleJumping()
  {
    if (Input.GetButtonDown("Jump"))
      AttemptJump();

    if (m_Rising && m_Rigidbody2D.velocity.y <= 0)
    {
      m_Rising = false;
      m_EyesController.LookDown();
    }
  }


  void HandleBoosting()
  {
    // How to Stop Boosting
    // by doug zwick
    if (m_Boosting)
    {
      var epsilon = 0.001;
      var xVelocity = m_Rigidbody2D.velocity.x;

      // Early out if you hit a wall
      if (Mathf.Abs(xVelocity) < epsilon)
      {
        EndBoosting();
        return;
      }

      // Otherwise, we gotta check the boost timer
      m_BoostTimer += Time.deltaTime;

      // The "boost delay" is just the minimum boost duration, within
      // which the player cannot interrupt a horizontal boost with
      // their input
      if (m_BoostTimer >= m_BoostDelay)
      {
        // "Input alignment" is basically the 1D dot product of the
        // X velocity and the movement input
        var inputAlignment = xVelocity * m_InputAxis;

        if (inputAlignment < 0 || m_Grounded || m_BoostTimer >= m_BoostFullDuration)
          EndBoosting();
      }
    }
  }


  void AttemptJump()
  {
    if (m_CanJump && m_Grounded)
      Jump();
  }


  void Jump()
  {
    var velocity = m_Rigidbody2D.velocity;
    velocity.y = m_JumpSpeed;
    m_Rigidbody2D.velocity = velocity;

    m_Rising = true;
    m_EyesController.LookUp();
  }


  void Land()
  {
    m_LandingSquashSequence.Cancel();
    m_LandingSquashSequence = ActionMaster.Actions.Sequence();
    m_LandingSquashSequence.Scale(m_LandingSquashNode.gameObject, m_LandingSquashScale, m_LandingSquashDuration / 2, new Ease(Ease.Quad.Out));
    m_LandingSquashSequence.Scale(m_LandingSquashNode.gameObject, Vector3.one, m_LandingSquashDuration / 2, new Ease(Ease.Quad.In));
  }


  void LeaveGround()
  {

  }


  void UpdateCheckpoint(GameObject checkpointGameObject)
  {
    m_CheckpointPosition = checkpointGameObject.transform.position;

    if (m_LatestCheckpoint != null)
      m_LatestCheckpoint.AttemptDeactivate();

    var checkpoint = checkpointGameObject.GetComponent<Checkpoint>();
    checkpoint.AttemptActivate(m_BodySpriteRenderer.color);
    m_LatestCheckpoint = checkpoint;
  }


  void Die()
  {
    if (m_Dead)
      return;

    GlobalData.DispatchHeroDied();

    m_Dead = true;
    m_CanMove = false;
    m_CanJump = false;
    m_Rigidbody2D.freezeRotation = false;

    var velocity = m_Rigidbody2D.velocity;
    velocity.y = m_DeathVelocity;
    m_Rigidbody2D.velocity = velocity;
    var angularVelocity = m_FacingLeft ? m_DeathAngularVelocity : -m_DeathAngularVelocity;
    m_Rigidbody2D.angularVelocity = angularVelocity;

    if (m_Boosting)
      EndBoosting();

    m_DeathSequence.Cancel();
    m_DeathSequence = ActionMaster.Actions.Sequence();
    m_DeathSequence.SpriteAlpha(m_BodySpriteRenderer.gameObject, 0, m_DeathPreDelay, new Ease(Ease.Quad.Out));
    m_DeathSequence.Call(Return, gameObject);
    m_DeathSequence.Delay(m_DeathPostDelay);
    m_DeathSequence.Call(RegainControl, gameObject);
  }


  void Return()
  {
    m_Transform.position = m_CheckpointPosition;
    m_PreviousX = m_Transform.position.x;
    ResetVelocity();
    m_Rigidbody2D.angularVelocity = 0;

    m_Rigidbody2D.freezeRotation = true;
    m_Rigidbody2D.SetRotation(0);
    m_Rigidbody2D.gravityScale = 0;

    m_BodySpriteRenderer.color = m_DefaultColor;

    GlobalData.DispatchHeroReturned();
  }


  void RegainControl()
  {
    m_Dead = false;
    m_CanMove = true;
    m_CanJump = true;

    m_Rigidbody2D.gravityScale = m_DefaultGravity;
  }


  void Win()
  {
    GlobalData.DisablePlayMode();
  }

  /**
  * FUNCTION NAME: CollectCoin
  * DESCRIPTION  : Collects a coin.
  * INPUTS       : _coin - Coin game object collected.
  * OUTPUTS      : None
  **/
  void CollectCoin(GameObject _coin)
  {
    var collectable = _coin.GetComponent<Collectable>();
    collectable.AttemptCollect();
  }

  /**
  * FUNCTION NAME: CollectKey
  * DESCRIPTION  : Collects a key and adds it to the hero inventory.
  * INPUTS       : _key - Key game object collected.
  * OUTPUTS      : None
  **/
  void CollectKey(GameObject _key)
  {
    var keyColorCode = _key.GetComponent<ColorCode>();
    var keyColor = keyColorCode.m_TileColor;
    m_CollectedKeys.Add(keyColor);
    Destroy(_key);
    GlobalData.DispatchKeyCollected(keyColor);
    //TODO: handle feedback.
  }

  /**
  * FUNCTION NAME: CheckOpenDoor
  * DESCRIPTION  : Try to open a door the hero collides with.
  * INPUTS       : _door - Door to try and open.
  * OUTPUTS      : bool - True/false on whether the door was openable based on keys collected.
  **/
  bool CheckOpenDoor(GameObject _door)
  {
    if (_door.GetComponent<DoorLogic>() == null)
    {
      Debug.LogError("Something went wrong with a door.  Contact Chris Onorati!");
      return false;
    }

    var doorColorCode = _door.GetComponent<ColorCode>();
    var doorColor = doorColorCode.m_TileColor;

    foreach (var collectedKey in m_CollectedKeys)
    {
      if (doorColor == collectedKey)
      {
        var doorLogic = _door.GetComponent<DoorLogic>();
        doorLogic.DestroyLinkedDoors();

        return true;
      }
    }

    //TODO:  Feedback.
    return false;
  }

  /**
  * FUNCTION NAME: ActivateSwitch
  * DESCRIPTION  : Open every door with the same color as the switch.
  * INPUTS       : _switch - Switch activated.
  * OUTPUTS      : None
  **/
  void ActivateSwitch(GameObject _switch)
  {
    var switchLogic = _switch.GetComponent<SwitchLogic>();
    switchLogic.AttemptActivate();
  }

  /**
  * FUNCTION NAME: Teleport
  * DESCRIPTION  : Teleport the player to another teleporter of the same color as the one activated.
  * INPUTS       : _teleporter - Teleporter activated.
  * OUTPUTS      : None
  **/
  void Teleport(GameObject teleporter)
  {
    if (teleporter == m_LastTeleportDestination)
      return;

    var teleporterTileLogic = teleporter.GetComponent<TeleporterTileLogic>();

    if (teleporterTileLogic == null)
    {
      Debug.LogError("Something went wrong with a teleporter.  Contact Chris Onorati!");
      return;
    }

  }

  /**
  * FUNCTION NAME: ActivateBooster
  * DESCRIPTION  : Sends the player boosting in the direction of the booster.
  * INPUTS       : _booster - Booster activated.
  * OUTPUTS      : None
  **/
  void ActivateBooster(GameObject _booster)
  {
    var tileDirection = _booster.GetComponent<TileDirection>();
    var direction = tileDirection.m_Direction;
    var velocity = m_Rigidbody2D.velocity;
    var preventMovementInput = false;

    // You should only prevent movement input for
    // horizontal boosts
    switch (direction)
    {
      case Direction.RIGHT:
        velocity.x = m_BoostSpeed;
        preventMovementInput = true;
        break;

      case Direction.UP:
        velocity.y = m_BoostSpeed;
        break;

      case Direction.LEFT:
        velocity.x = -m_BoostSpeed;
        preventMovementInput = true;
        break;

      case Direction.DOWN:
        velocity.y = -m_BoostSpeed;
        break;
    }

    BeginBoosting(preventMovementInput);
    m_Rigidbody2D.velocity = velocity;

    //TODO: Feedback.
  }


  void ResetVelocity()
  {
    m_Rigidbody2D.velocity = Vector2.zero;
  }


  void BoundsCheck()
  {
    var currentY = transform.position.y;

    if (currentY <= m_TileGrid.m_MinBounds.y + m_KillHeight)
      Die();
  }


  void ClampVelocity()
  {
    var velocity = m_Rigidbody2D.velocity;
    if (velocity.y < -m_TerminalVelocity)
    {
      velocity.y = -m_TerminalVelocity;
    }
  }


  void BeginBoosting(bool preventMovementInput = true)
  {
    if (preventMovementInput)
    {
      m_Boosting = true;
      m_BoostTimer = 0;
    }

    m_CameraController.AttemptActionZoom();
  }


  void EndBoosting()
  {
    m_Boosting = false;
  }


  private void OnDestroy()
  {
    GlobalData.PlayModePreToggle -= OnPlayModePreToggle;
  }
}
