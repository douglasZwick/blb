using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CheckpointResponder : MonoBehaviour
{
  public SpriteRenderer m_ColorReferenceRenderer;

  Transform m_Transform;
  Vector3 m_CheckpointPosition;
  Checkpoint m_LatestCheckpoint;

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
    m_CheckpointPosition = m_Transform.position;
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
    m_CheckpointPosition = checkpoint.transform.position;

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

    m_Transform.position = m_CheckpointPosition;

    var checkpointEventData = new CheckpointEventData()
    {
      m_Checkpoint = m_LatestCheckpoint,
      m_NewPosition = m_CheckpointPosition,
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
