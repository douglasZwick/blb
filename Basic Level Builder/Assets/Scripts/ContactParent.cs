using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContactParent : MonoBehaviour
{
  public Transform m_Transform { get; private set; }
  List<ContactChild> m_Children = new List<ContactChild>();


  private void Awake()
  {
    m_Transform = transform;
  }


  public void Add(ContactChild child)
  {
    m_Children.Add(child);
  }


  public void Remove(ContactChild child)
  {
    m_Children.Remove(child);
  }


  public void RemoveAll()
  {
    foreach (var child in m_Children)
      child.DetachFrom(this);

    m_Children.Clear();
  }
}
