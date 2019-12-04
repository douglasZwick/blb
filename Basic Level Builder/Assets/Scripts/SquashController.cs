using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SquashController : MonoBehaviour
{
  public Transform m_SquashNode;
  public Vector3 m_SquashScale = new Vector3(1.5f, 0.5f, 1.0f);
  public Vector3 m_SquashDirection = Vector3.right;
  public bool m_UseCollider = true;
  public bool m_AmplifyBySpeed = true;
  public float m_ReferenceSpeed = 8;
  public float m_MaxAmplificationFactor = 2;
  public float m_Duration = 0.15f;

  [System.Serializable]
  public class Events
  {
    public UnityEvent SquashStarted;
    public UnityEvent SquashEnded;
  }

  public Events m_Events;

  Transform m_Transform;
  uint m_ContactCount = 0;
  ActionSequence m_SquashSequence;
  Vector3 m_PreviousPosition;


  private void Awake()
  {
    m_Transform = transform;
    m_SquashSequence = ActionMaster.Actions.Sequence();

    m_PreviousPosition = m_Transform.position;
    m_SquashDirection = m_SquashDirection.normalized;
  }


  private void Update()
  {
    m_PreviousPosition = m_Transform.position;
  }


  private void OnTriggerEnter2D(Collider2D collision)
  {
    if (!m_UseCollider)
      return;

    if (m_ContactCount == 0)
      AttemptSquash();

    ++m_ContactCount;
  }


  private void OnTriggerExit2D(Collider2D collision)
  {
    if (!m_UseCollider)
      return;

    --m_ContactCount;
  }


  public void AttemptSquash()
  {
    if (!m_SquashSequence.Active)
      Squash();
  }


  void Squash()
  {
    var squashScale = m_SquashScale;
    var duration = m_Duration / 2;

    if (m_AmplifyBySpeed)
    {
      var currentPosition = m_Transform.position;
      var difference = currentPosition - m_PreviousPosition;
      var projection = Vector3.Dot(difference, m_SquashDirection);
      var currentSpeed = Mathf.Abs(projection) / Time.deltaTime;
      var amplificationFactor = currentSpeed / m_ReferenceSpeed;
      var interpolant = Mathf.Clamp(amplificationFactor, 0, m_MaxAmplificationFactor);

      squashScale = Vector3.LerpUnclamped(Vector3.one, m_SquashScale, interpolant);
      duration *= interpolant;
    }

    m_Events.SquashStarted.Invoke();

    m_SquashSequence = ActionMaster.Actions.Sequence();
    m_SquashSequence.Scale(m_SquashNode.gameObject, squashScale, duration, new Ease(Ease.Quad.Out));
    m_SquashSequence.Scale(m_SquashNode.gameObject, Vector3.one, duration, new Ease(Ease.Quad.In));
    m_SquashSequence.Call(EndSequence, gameObject);
  }


  void EndSequence()
  {
    m_Events.SquashEnded.Invoke();
  }


  public void OnReturnedToCheckpoint(CheckpointEventData eventData)
  {
    m_PreviousPosition = eventData.m_NewPosition;
  }


  public void OnUsedTeleporter(TeleportEventData eventData)
  {
    m_PreviousPosition = eventData.m_ToPosition;
  }
}
