/***************************************************
Authors:        Douglas Zwick, Brenden Epp
Last Updated:   7/21/2026

Copyright 2018-2026, DigiPen Institute of Technology
***************************************************/

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

  [HideInInspector]
  public Events m_Events;
  [HideInInspector]
  public TileGrid.Element m_Element;

  private Direction m_Direction;
  public DirectionType m_DirectionType = DirectionType.ORTHOGONAL;

  public Direction Get()
  {
    return m_Direction;
  }

  public void Set(Direction direction)
  {
    SetHelper(direction, initialize: false);
  }


  void SetHelper(Direction direction, bool initialize = false)
  {
    m_Direction = direction;
    if (m_Element != null)
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