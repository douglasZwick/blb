using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
  public int m_Value = 1;

  [System.Serializable]
  public class Events
  {
    public CollectableEvent WasCollected;
  }

  public Events m_Events;

  bool m_Collected = false;


  public void AttemptCollect(CoinCollector collector)
  {
    if (!m_Collected)
      Collect(collector);
  }


  void Collect(CoinCollector collector)
  {
    m_Collected = true;

    var eventData = new CollectableEventData();

    m_Events.WasCollected.Invoke(eventData);

    collector.Collected(this);

    Destroy(gameObject);
  }
}
