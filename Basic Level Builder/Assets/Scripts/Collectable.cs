using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Collectable : MonoBehaviour
{
  public bool m_DestroyOnCollect = true;

  [System.Serializable]
  public class Events
  {
    public CollectableEvent WasCollected;
  }

  public Events m_Events;

  CoinCountIncrementer m_CoinCountIncrementer;
  bool m_Collected = false;


  private void Awake()
  {
    m_CoinCountIncrementer = GetComponent<CoinCountIncrementer>();
  }


  public void AttemptCollect()
  {
    if (!m_Collected)
      Collect();
  }


  void Collect()
  {
    m_Collected = true;

    if (m_CoinCountIncrementer != null)
      m_CoinCountIncrementer.OnCollected();

    var eventData = new CollectableEventData();

    m_Events.WasCollected.Invoke(eventData);

    if (!eventData.m_Handled)
      Destroy(gameObject);
  }
}


[System.Serializable]
public class CollectableEvent : UnityEvent<CollectableEventData> { }

public class CollectableEventData
{
  public bool m_Handled = false;
}
