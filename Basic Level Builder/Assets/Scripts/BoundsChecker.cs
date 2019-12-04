using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BoundsChecker : MonoBehaviour
{
  public bool m_CheckRight = false;
  public bool m_CheckLeft = false;
  public bool m_CheckTop = false;
  public bool m_CheckBottom = true;
  public Vector2 m_MinBounds = new Vector2(-10, -10);
  public Vector2 m_MaxBounds = new Vector2(10, 10);
  public bool m_RelativeToGridBounds = true;

  [System.Serializable]
  public class Events
  {
    public BoundsCheckerEvent WentOutOfBounds;
  }

  public Events m_Events;

  Transform m_Transform;
  TileGrid m_ReferenceGrid;


  private void Awake()
  {
    m_Transform = transform;

    if (m_RelativeToGridBounds)
      m_ReferenceGrid = FindObjectOfType<TileGrid>();
  }


  private void Update()
  {
    var x = m_Transform.position.x;
    var y = m_Transform.position.y;

    var wentOutOfBounds = false;
    var eventData = new BoundsCheckerEventData();

    var rightEdge = m_MaxBounds.x;
    var leftEdge = m_MinBounds.x;
    var topEdge = m_MaxBounds.y;
    var bottomEdge = m_MinBounds.y;

    if (m_RelativeToGridBounds)
    {
      rightEdge += m_ReferenceGrid.m_MaxBounds.x;
      leftEdge += m_ReferenceGrid.m_MinBounds.x;
      topEdge += m_ReferenceGrid.m_MaxBounds.y;
      bottomEdge += m_ReferenceGrid.m_MinBounds.y;
    }

    if (m_CheckRight && x >= rightEdge)
    {
      wentOutOfBounds = true;
      eventData.m_Right = true;
    }
    if (m_CheckLeft && x <= leftEdge)
    {
      wentOutOfBounds = true;
      eventData.m_Left = true;
    }
    if (m_CheckTop && y >= topEdge)
    {
      wentOutOfBounds = true;
      eventData.m_Top = true;
    }
    if (m_CheckBottom && y <= bottomEdge)
    {
      wentOutOfBounds = true;
      eventData.m_Bottom = true;
    }

    if (wentOutOfBounds)
      m_Events.WentOutOfBounds.Invoke(eventData);
  }
}


[System.Serializable]
public class BoundsCheckerEvent : UnityEvent<BoundsCheckerEventData> { }

public class BoundsCheckerEventData
{
  public bool m_Right = false;
  public bool m_Left = false;
  public bool m_Top = false;
  public bool m_Bottom = false;
}
