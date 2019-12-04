using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TileDirection : MonoBehaviour
{
  public delegate void DirectionEvent(Direction direction);
  public event DirectionEvent DirectionSet;

  [System.Serializable]
  public class Events
  {
    public TileDirectionEvent DirectionInitialized;
    public TileDirectionEvent DirectionSet;
  }

  public Events m_Events;

  [HideInInspector]
  public Direction m_Direction;
  [HideInInspector]
  public TileGrid.Element m_Element;


  public void Initialize(Direction direction)
  {
    SetHelper(direction, initialize: true);
  }


  public void Set(Direction direction)
  {
    SetHelper(direction, initialize: false);
  }


  void SetHelper(Direction direction, bool initialize = false)
  {
    m_Direction = direction;
    m_Element.m_Direction = direction;

    DirectionSet?.Invoke(direction);

    var eventData = new TileDirectionEventData()
    {
      m_Direction = direction,
    };

    if (initialize)
      m_Events.DirectionInitialized.Invoke(eventData);
    else
      m_Events.DirectionSet.Invoke(eventData);
  }
}

[System.Serializable]
public class TileDirectionEvent : UnityEvent<TileDirectionEventData> { }

public class TileDirectionEventData
{
  public Direction m_Direction;
}
