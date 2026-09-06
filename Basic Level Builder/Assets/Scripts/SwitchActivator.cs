using System.Collections.Generic;
using UnityEngine;
using NewestFileData;

public class SwitchActivator : MonoBehaviour
{
  [System.Serializable]
  public class Events
  {
    public SwitchEvent ActivatedSwitch;
  }

  public Events m_Events;

  public List<AudioClip> m_DoorClips;
  public SfxPlayer m_SfxPlayer;


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

    var @switch = obj.GetComponent<SwitchLogic>();

    if (@switch == null)
      return;

    @switch.AttemptActivate(this);
  }


  public void ActivatedSwitch(TileColor switchColor)
  {
    var index = (int)switchColor;
    PlaySfx(index);
  }
  

  void PlaySfx(int index)
  {
    var clip = m_DoorClips[index];
    m_SfxPlayer.AttemptPlay(clip);
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
