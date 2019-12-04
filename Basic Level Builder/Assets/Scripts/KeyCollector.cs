using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class KeyCollector : MonoBehaviour
{
  [System.Serializable]
  public class Events
  {
    public KeyCollectorEvent Collected;
  }

  public Events m_Events;

  KeysDisplay m_KeyHolder;


  private void Awake()
  {
    m_KeyHolder = FindObjectOfType<KeysDisplay>();
  }


  private void OnCollisionEnter2D(Collision2D collision)
  {
    Collision(collision.gameObject);
  }


  private void OnTriggerEnter2D(Collider2D collision)
  {
    Collision(collision.gameObject);
  }


  void Collision(GameObject obj)
  {
    if (!enabled)
      return;

    var key = obj.GetComponent<Key>();

    if (key != null)
    {
      key.AttemptCollect(this);
    }
    else
    {
      var door = obj.GetComponent<DoorLogic>();

      if (door != null)
      {
        AttemptOpenDoor(door);
      }
    }
  }


  public void Collected(Key key)
  {
    var color = key.m_Color;

    GlobalData.DispatchKeyCollected(color);

    var eventData = new KeyCollectorEventData()
    {
      m_Key = key,
      m_Color = color,
    };

    m_Events.Collected.Invoke(eventData);
  }


  void AttemptOpenDoor(DoorLogic door)
  {
    var doorColor = door.m_Color;

    if (m_KeyHolder.Has(doorColor))
      door.AttemptOpen();
  }


  public void OnDied(HealthEventData eventData)
  {
    enabled = false;
  }


  public void OnReturned(HealthEventData eventData)
  {
    enabled = true;
  }
}


[System.Serializable]
public class KeyCollectorEvent : UnityEvent<KeyCollectorEventData> { }

public class KeyCollectorEventData
{
  public Key m_Key;
  public TileColor m_Color;
}
