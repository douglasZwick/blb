using System;
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
  public List<AudioClip> m_StompClips;
  public SfxPlayer m_StompPlayer;
  public SfxPlayer m_OneUpPlayer;
  public int m_StompOneUpThreshold = 7;
  public Events m_Events;

  float m_StompVelocityThreshold = -0.8f;
  private int m_StompComboCounter = 0;
  private bool m_Dead = false;


  private void OnCollisionEnter2D(Collision2D collision)
  {
    OnCollision(collision.collider, collision.relativeVelocity);
  }


  private void OnTriggerEnter2D(Collider2D collider)
  {
    var relativeVelocity = m_Stomper ? -m_StomperRB.linearVelocity : Vector2.zero;

    OnCollision(collider, relativeVelocity);
  }


  void OnCollision(Collider2D collider, Vector2 relativeVelocity)
  {
    if (m_Dead) return;

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
        var eventData = new HealthEventData()
        {
          m_StompComboCounter = m_StompComboCounter,
          m_EnemyPosition = enemy.transform.position,
        };
        m_Events.StompedEnemy.Invoke(eventData);
        PlayStompSfx();

        ++m_StompComboCounter;
      }
      else if (m_KilledByEnemy)
      {
        Die();
      }
    }
  }


  private void PlayStompSfx()
  {
    var index = Math.Min(m_StompComboCounter, m_StompClips.Count - 1);
    m_StompPlayer.AttemptPlay(m_StompClips[index]);

    if (m_StompComboCounter >= m_StompOneUpThreshold)
      m_OneUpPlayer.AttemptPlay();
  }


  public void OnLanded()
  {
    m_StompComboCounter = 0;
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
    m_Dead = true;
    m_Events.Died.Invoke(new HealthEventData());
  }


  void Return()
  {
    m_Dead = false;

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
  public int m_StompComboCounter;
  public Vector3 m_EnemyPosition;
}
