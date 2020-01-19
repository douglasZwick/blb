using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BecomeIntangibleOnDeath : MonoBehaviour
{
  public List<Collider2D> m_Colliders;


  public void OnDied(HealthEventData eventData)
  {
    foreach (var collider in m_Colliders)
      collider.enabled = false;
  }
}
