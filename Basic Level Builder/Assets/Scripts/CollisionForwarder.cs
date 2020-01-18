using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class CollisionForwarder : MonoBehaviour
{
  [System.Serializable]
  public class Events
  {
    public Collider2DEvent Enter;
    public Collider2DEvent Exit;
  }

  public Events m_Events;


  private void OnCollisionEnter2D(Collision2D collision)
  {
    Enter(collision.collider);
  }


  private void OnTriggerEnter2D(Collider2D collision)
  {
    Enter(collision);
  }


  private void OnCollisionExit2D(Collision2D collision)
  {
    Exit(collision.collider);
  }


  private void OnTriggerExit2D(Collider2D collision)
  {
    Exit(collision);
  }


  void Enter(Collider2D collider)
  {
    m_Events.Enter.Invoke(collider);
  }


  void Exit(Collider2D collider)
  {
    m_Events.Exit.Invoke(collider);
  }
}

[System.Serializable]
public class Collider2DEvent : UnityEvent<Collider2D> { }
