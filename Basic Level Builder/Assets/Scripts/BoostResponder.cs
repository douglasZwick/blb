using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class BoostResponder : MonoBehaviour
{
  [System.Serializable]
  public class Events
  {
    public BoostLogicEvent BoostTriggered;
  }

  public Events m_Events;


  private void OnTriggerEnter2D(Collider2D collision)
  {
    var boostLogic = collision.GetComponent<BoostLogic>();

    if (boostLogic != null)
    {
      var eventData = new BoostLogicEventData()
      {
        m_BoostLogic = boostLogic,
        m_TileDirection = boostLogic.GetComponent<TileDirection>(),
      };
      m_Events.BoostTriggered.Invoke(eventData);
    }
  }
}

[System.Serializable]
public class BoostLogicEvent : UnityEvent<BoostLogicEventData> { }

public class BoostLogicEventData
{
  public BoostLogic m_BoostLogic;
  public TileDirection m_TileDirection;
}
