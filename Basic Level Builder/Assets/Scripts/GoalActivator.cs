using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GoalActivator : MonoBehaviour
{
  [System.Serializable]
  public class Events
  {
    public GoalEvent ActivatedGoal;
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

    var goal = obj.GetComponent<Goal>();

    if (goal == null)
      return;

    goal.AttemptActivate(this);
  }


  public void Activated(GoalEventData eventData)
  {
    m_Events.ActivatedGoal.Invoke(eventData);

    enabled = false;
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
