using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CoinCollector : MonoBehaviour
{
  [System.Serializable]
  public class Events
  {
    public CoinCollectorEvent Collected;
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

    var coin = obj.GetComponent<Coin>();

    if (coin == null)
      return;

    coin.AttemptCollect(this);
  }


  public void Collected(Coin coin)
  {
    if (!enabled)
      return;

    var value = coin.m_Value;

    GlobalData.DispatchCoinCollected(value);

    var eventData = new CoinCollectorEventData()
    {
      m_Coin = coin,
      m_Value = value,
    };

    m_Events.Collected.Invoke(eventData);
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
public class CoinCollectorEvent : UnityEvent<CoinCollectorEventData> { }

public class CoinCollectorEventData
{
  public Coin m_Coin;
  public int m_Value;
}
