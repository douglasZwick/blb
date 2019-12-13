using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CheckpointResponder : MonoBehaviour
{
  public SpriteRenderer m_ColorReferenceRenderer;

  Transform m_Transform;
  Checkpoint m_LatestCheckpoint;
  Vector3 m_InitialPosition;

  [System.Serializable]
  public class Events
  {
    public CheckpointEvent UpdatedCheckpoint;
    public CheckpointEvent ReturnedToCheckpoint;
  }

  public Events m_Events;


  private void Awake()
  {
    m_Transform = transform;
    m_InitialPosition = m_Transform.position;
  }


  private void OnTriggerEnter2D(Collider2D collision)
  {
    if (!enabled)
      return;

    var checkpoint = collision.GetComponent<Checkpoint>();
    if (checkpoint != null)
      AttemptUpdateCheckpoint(checkpoint);
  }


  void AttemptUpdateCheckpoint(Checkpoint checkpoint)
  {
    if (checkpoint == m_LatestCheckpoint)
      return;

    UpdateCheckpoint(checkpoint);
  }


  void UpdateCheckpoint(Checkpoint checkpoint)
  {
    if (m_LatestCheckpoint != null)
      m_LatestCheckpoint.AttemptDeactivate();

    checkpoint.AttemptActivate(m_ColorReferenceRenderer.color);
    m_LatestCheckpoint = checkpoint;

    var eventData = new CheckpointEventData()
    {
      m_Checkpoint = checkpoint,
    };

    m_Events.UpdatedCheckpoint.Invoke(eventData);
  }


  public void OnDied(HealthEventData eventData)
  {
    enabled = false;
  }


  public void OnReturned(HealthEventData eventData)
  {
    enabled = true;

    var returnPosition = m_InitialPosition;
    if (m_LatestCheckpoint != null)
      returnPosition = m_LatestCheckpoint.m_Transform.position;

    m_Transform.position = returnPosition;

    var checkpointEventData = new CheckpointEventData()
    {
      m_Checkpoint = m_LatestCheckpoint,
      m_NewPosition = returnPosition,
    };

    m_Events.ReturnedToCheckpoint.Invoke(checkpointEventData);
  }
}

[System.Serializable]
public class CheckpointEvent : UnityEvent<CheckpointEventData> { }

public class CheckpointEventData
{
  public Checkpoint m_Checkpoint;
  public Vector3 m_NewPosition;
}
