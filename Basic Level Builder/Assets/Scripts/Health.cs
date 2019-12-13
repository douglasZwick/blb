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
  }

  public Events m_Events;


  private void OnCollisionEnter2D(Collision2D collision)
  {
    if (collision.collider.CompareTag("Deadly"))
      Die();
  }


  private void OnTriggerEnter2D(Collider2D collision)
  {
    if (collision.CompareTag("Deadly"))
      Die();
  }


  public void OnWentOutOfBounds(BoundsCheckerEventData eventData)
  {
    Die();
  }


  public void OnDeathEffectsFinished(HealthEventData eventData)
  {
    Return();
  }


  void Die()
  {
    m_Events.Died.Invoke(new HealthEventData());
  }


  void Return()
  {
    GlobalData.DispatchPreHeroReturn();

    m_Events.Returned.Invoke(new HealthEventData());

    GlobalData.DispatchHeroReturned();
  }
}


[System.Serializable]
public class HealthEvent : UnityEvent<HealthEventData> { }

public class HealthEventData
{
}
