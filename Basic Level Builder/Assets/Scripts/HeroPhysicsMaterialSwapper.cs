using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class HeroPhysicsMaterialSwapper : MonoBehaviour
{
  public PhysicsMaterial2D m_DeadPhysicsMaterial;

  Rigidbody2D m_Rigidbody;
  PhysicsMaterial2D m_DefaultPhysicsMaterial;


  private void Awake()
  {
    m_Rigidbody = GetComponent<Rigidbody2D>();
    m_DefaultPhysicsMaterial = m_Rigidbody.sharedMaterial;

    GlobalData.HeroDied += OnHeroDied;
    GlobalData.HeroReturned += OnHeroReturned;
  }


  public void OnHeroDied()
  {
    m_Rigidbody.sharedMaterial = m_DeadPhysicsMaterial;
  }


  public void OnHeroReturned()
  {
    m_Rigidbody.sharedMaterial = m_DefaultPhysicsMaterial;
  }


  private void OnDestroy()
  {
    GlobalData.HeroDied -= OnHeroDied;
    GlobalData.HeroReturned -= OnHeroReturned;
  }
}
