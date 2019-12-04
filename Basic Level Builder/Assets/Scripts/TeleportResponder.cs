using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TeleportResponder : MonoBehaviour
{
  public TeleportationStreak m_StreakPrefab;

  [System.Serializable]
  public class Events
  {
    public TeleportEvent UsedTeleporter;
  }

  public Events m_Events;

  Transform m_Transform;
  TeleporterTileLogic m_LastTeleportDestination;


  private void Awake()
  {
    m_Transform = transform;
  }


  private void OnCollisionEnter2D(Collision2D collision)
  {
    Collision(collision.gameObject);
  }


  private void OnTriggerEnter2D(Collider2D collision)
  {
    Collision(collision.gameObject);
  }


  private void OnCollisionExit2D(Collision2D collision)
  {
    CollisionEnded(collision.gameObject);
  }


  private void OnTriggerExit2D(Collider2D collision)
  {
    CollisionEnded(collision.gameObject);
  }


  void Collision(GameObject obj)
  {
    var teleporter = obj.GetComponent<TeleporterTileLogic>();

    if (teleporter == null)
      return;

    AttemptTeleport(teleporter);
  }


  void CollisionEnded(GameObject obj)
  {
    var teleporter = obj.GetComponent<TeleporterTileLogic>();

    if (teleporter == null)
      return;

    if (teleporter == m_LastTeleportDestination)
      m_LastTeleportDestination = null;
  }


  void AttemptTeleport(TeleporterTileLogic teleporter)
  {
    if (teleporter == m_LastTeleportDestination)
      return;

    if (teleporter.m_DestinationTeleporter == null)
    {
      // No destination; say something about this
      return;
    }

    Teleport(teleporter);
  }


  void Teleport(TeleporterTileLogic teleporter)
  {
    var fromPosition = m_Transform.position;
    var toPosition = teleporter.m_Destination.position;

    var eventData = new TeleportEventData()
    {
      m_FromTeleporter = teleporter,
      m_ToTeleporter = teleporter.m_DestinationTeleporter,
      m_FromPosition = fromPosition,
      m_ToPosition = toPosition,
    };

    m_LastTeleportDestination = eventData.m_ToTeleporter;
    m_Transform.position = eventData.m_ToPosition;

    eventData.m_FromTeleporter.m_Events.TeleportedFrom.Invoke(eventData);
    m_Events.UsedTeleporter.Invoke(eventData);
    eventData.m_ToTeleporter.m_Events.TeleportedTo.Invoke(eventData);

    var teleporterColor = teleporter.m_ColorCode.GetCurrentColor();
    CreateStreak(fromPosition, toPosition, teleporterColor);
  }


  void CreateStreak(Vector3 from, Vector3 to, Color teleporterColor)
  {
    var streak = Instantiate(m_StreakPrefab);
    streak.Setup(from, to, teleporterColor);
  }
}


[System.Serializable]
public class TeleportEvent : UnityEvent<TeleportEventData> { }

public class TeleportEventData
{
  public TeleporterTileLogic m_FromTeleporter;
  public TeleporterTileLogic m_ToTeleporter;
  public Vector3 m_FromPosition;
  public Vector3 m_ToPosition;
}
