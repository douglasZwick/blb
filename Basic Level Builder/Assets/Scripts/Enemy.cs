using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class Enemy : MonoBehaviour
{
  public bool m_Stompable = false;

  Health m_Health;


  private void Awake()
  {
    m_Health = GetComponent<Health>();
  }


  public void GetStomped()
  {
    if (m_Stompable)
      m_Health.Die();
  }
}
