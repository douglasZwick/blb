using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabSpawner : MonoBehaviour
{
  public GameObject m_Prefab;

  Transform m_Transform;


  private void Awake()
  {
    m_Transform = transform;
  }


  public void Spawn()
  {
    SpawnHelper();
  }


  public void Spawn(object nullEventData)
  {
    SpawnHelper();
  }


  void SpawnHelper()
  {
    var spawnPosition = m_Transform.position;
    var spawnRotation = Quaternion.identity;
    Instantiate(m_Prefab, spawnPosition, spawnRotation);
  }
}
