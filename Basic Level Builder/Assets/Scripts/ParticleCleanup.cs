using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleCleanup : MonoBehaviour
{
  List<ParticleSystem> m_ParticleSystems = new List<ParticleSystem>();


  private void Awake()
  {
    var particleSystemObjects = GameObject.FindGameObjectsWithTag("GoalFX");

    foreach (var particleSystemObject in particleSystemObjects)
      m_ParticleSystems.Add(particleSystemObject.GetComponent<ParticleSystem>());

    GlobalData.PlayModeToggled += OnPlayModeToggled;
  }


  void OnPlayModeToggled(bool isInPlayMode)
  {
    foreach (var ps in m_ParticleSystems)
      ps.Clear();
  }


  private void OnDestroy()
  {
    GlobalData.PlayModeToggled -= OnPlayModeToggled;
  }
}
