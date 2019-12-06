using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContactChild : MonoBehaviour
{
  public Transform m_Transform { get; private set; }

  ContactParent m_CurrentParent;
  List<ContactParent> m_CurrentContacts = new List<ContactParent>();


  private void Awake()
  {
    m_Transform = transform;
  }


  private void OnCollisionEnter2D(Collision2D collision)
  {
    var contactParent = collision.gameObject.GetComponent<ContactParent>();

    if (contactParent == null)
      return;

    AttachTo(contactParent);
    contactParent.Add(this);
  }


  // put exit here


  public void AttachTo(ContactParent contact)
  {
    if (m_CurrentContacts.Count == 0)
      SetParent(contact);

    m_CurrentContacts.Add(contact);
  }


  public void DetachFrom(ContactParent contact)
  {
    m_CurrentContacts.Remove(contact);

    if (m_CurrentContacts.Count == 0)
    {
      SetParent(null);
    }
    else if (contact == m_CurrentParent)
    {
      SetParent(m_CurrentContacts[0]);
    }
  }


  void SetParent(ContactParent newParent)
  {
    m_CurrentParent = newParent;

    if (newParent == null)
      m_Transform.SetParent(null, worldPositionStays: true);
    else
      m_Transform.SetParent(newParent.m_Transform, worldPositionStays: true);
  }
}
