using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
  [System.Serializable]
  public class Events
  {
    public HealthEvent Died;
    public HealthEvent Returned;
    public HealthEvent StompedEnemy;  // TODO: this feels weeeeeird
  }

  public bool m_IsTheHero = false;
  public bool m_KilledByDeadly = false;
  public bool m_KilledByEnemy = false;
  public bool m_Stomper = false;  // TODO: come up with a less hacky solution
  public Rigidbody2D m_StomperRB;
  public Events m_Events;

  float m_StompVelocityThreshold = -0.8f;


  private void OnCollisionEnter2D(Collision2D collision)
  {
    OnCollision(collision.collider, collision.relativeVelocity);
  }


  private void OnTriggerEnter2D(Collider2D collider)
  {
    var relativeVelocity = m_Stomper ? -m_StomperRB.velocity : Vector2.zero;

    OnCollision(collider, relativeVelocity);
  }


  void OnCollision(Collider2D collider, Vector2 relativeVelocity)
  {
    if (m_KilledByDeadly && collider.CompareTag("Deadly"))
    {
      Die();
      return;
    }

    var enemy = collider.GetComponent<Enemy>();
    if (enemy != null)
      EnemyCollision(enemy, relativeVelocity);
  }


  void EnemyCollision(Enemy enemy, Vector2 relativeVelocity)
  {
    if (m_KilledByEnemy && !(m_Stomper && enemy.m_Stompable))
    {
      Die();
      return;
    }

    if (m_Stomper && enemy.m_Stompable)
    {
      if (-relativeVelocity.y < m_StompVelocityThreshold)
      {
        enemy.GetStomped();
        m_Events.StompedEnemy.Invoke(new HealthEventData());
      }
      else if (m_KilledByEnemy)
      {
        Die();
      }
    }
  }


  public void OnWentOutOfBounds(BoundsCheckerEventData eventData)
  {
    Die();
  }


  public void OnDeathEffectsFinished(HealthEventData eventData)
  {
    Return();
  }


  public void Die()
  {
    m_Events.Died.Invoke(new HealthEventData());
  }


  void Return()
  {
    if (m_IsTheHero)
      GlobalData.DispatchPreHeroReturn();

    m_Events.Returned.Invoke(new HealthEventData());

    if (m_IsTheHero)
      GlobalData.DispatchHeroReturned();
  }
}


[System.Serializable]
public class HealthEvent : UnityEvent<HealthEventData> { }

public class HealthEventData
{
}
