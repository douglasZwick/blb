using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchActivator : MonoBehaviour
{
  [System.Serializable]
  public class Events
  {
    public SwitchEvent ActivatedSwitch;
  }

  public Events m_Events;


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

    @switch.AttemptActivate();
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
