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
    public TileDirectionEvent DirectionSet;
  }

  public Events m_Events;
  
  [SerializeField]
  private Transform m_TransformToRotate;

  private Direction m_Direction;

  public Direction Get()
  {
    return m_Direction;
  }

  public void Set(Direction direction)
  {
    m_Direction = direction;

    var angle = direction switch
    {
      Direction.UP => 90,
      Direction.LEFT => 180,
      Direction.DOWN => -90,
      // Direction.RIGHT
      _ => (float)0,
    };
    m_TransformToRotate.localEulerAngles = Vector3.forward * angle;
  }
}

[System.Serializable]
public class TileDirectionEvent : UnityEvent<TileDirectionEventData> { }

public class TileDirectionEventData
{
  public Direction m_Direction;
}