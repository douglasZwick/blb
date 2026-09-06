using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NewestFileData;

public class KeyCollector : MonoBehaviour
{
  [System.Serializable]
  public class Events
  {
    public KeyCollectorEvent Collected;
  }

  public Events m_Events;

  public List<AudioClip> m_KeyClips;
  public List<AudioClip> m_DoorClips;
  public SfxPlayer m_SfxPlayer;
  public SfxPlayer m_DoorPopPlayer;

  KeysDisplay m_KeyHolder;


  private void Awake()
  {
    m_KeyHolder = FindObjectOfType<KeysDisplay>();

    GlobalData.DoorSectionOpened += OnDoorSectionOpened;
  }


  private void OnCollisionEnter2D(Collision2D collision)
  {
    Collision(collision.gameObject);
  }


  private void OnTriggerEnter2D(Collider2D collision)
  {
    Collision(collision.gameObject);
  }


  public void OnDoorSectionOpened()
  {
    m_DoorPopPlayer.AttemptPlay();
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

    var index = (int)color;
    PlaySfx(m_KeyClips, index);
  }


  void PlaySfx(List<AudioClip> list, int index)
  {
    var clip = list[index];
    m_SfxPlayer.AttemptPlay(clip);
  }


  void AttemptOpenDoor(DoorLogic door)
  {
    var doorColor = door.m_Color;

    if (m_KeyHolder.Has(doorColor))
      door.AttemptOpen(this);
  }


  public void OpenedDoor(TileColor doorColor)
  {
    var index = (int)doorColor;
    PlaySfx(m_DoorClips, index);
  }


  public void OnDied(HealthEventData eventData)
  {
    enabled = false;
  }


  public void OnReturned(HealthEventData eventData)
  {
    enabled = true;
  }


  void OnDestroy()
  {
    GlobalData.DoorSectionOpened -= OnDoorSectionOpened;
  }
}


[System.Serializable]
public class KeyCollectorEvent : UnityEvent<KeyCollectorEventData> { }

public class KeyCollectorEventData
{
  public Key m_Key;
  public TileColor m_Color;
}
