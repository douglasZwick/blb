using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContactChildSensor : MonoBehaviour
{
  public ContactChild m_ContactChild;


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
    var contactParent = collider.GetComponent<ContactParent>();
    if (contactParent != null)
      m_ContactChild.HandleEnter(contactParent);
  }


  void Exit(Collider2D collider)
  {
    var contactParent = collider.GetComponent<ContactParent>();
    if (contactParent != null)
      m_ContactChild.HandleExit(contactParent);
  }
}
