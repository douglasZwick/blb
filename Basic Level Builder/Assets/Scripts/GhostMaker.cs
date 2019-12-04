using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EyesController))]
[RequireComponent(typeof(HopController))]
public class GhostMaker : MonoBehaviour
{
  public uint m_MaxGhosts = 60;
  public Ghost m_GhostPrefab;
  public float m_ActivationDistance = 0.25f;
  public List<SquashController> m_SquashControllers;

  Transform m_Transform;
  EyesController m_EyesController;
  HopController m_HopController;
  Ghost[] m_Ghosts;
  uint m_NextGhostIndex = 0;
  Vector3 m_MostRecentGhostPosition = Vector3.positiveInfinity;
  float m_ActivationDistanceSq;
  bool m_Dead = false;


  void Awake()
  {
    m_Transform = transform;
    m_HopController = GetComponent<HopController>();
    m_EyesController = GetComponent<EyesController>();

    GlobalData.GhostModeEnabled += OnGhostModeEnabled;
    GlobalData.GhostModeDisabled += OnGhostModeDisabled;

    m_Ghosts = new Ghost[m_MaxGhosts];

    m_ActivationDistanceSq = m_ActivationDistance * m_ActivationDistance;
  }


  void Update()
  {
    if (m_Dead)
      return;

    var currentPosition = m_Transform.position;
    var difference = currentPosition - m_MostRecentGhostPosition;
    var distanceSq = Vector3.SqrMagnitude(difference);

    if (distanceSq >= m_ActivationDistanceSq)
      CreateGhost();
  }


  void OnGhostModeEnabled()
  {
    enabled = true;
  }


  void OnGhostModeDisabled()
  {
    enabled = false;
  }


  public void OnDied(HealthEventData eventData)
  {
    m_Dead = true;
  }


  public void OnReturned(HealthEventData eventData)
  {
    m_Dead = false;
  }


  void CreateGhost()
  {
    var position = m_Transform.position;
    var rotation = m_Transform.rotation;
    var ghost = Instantiate(m_GhostPrefab, position, rotation);
    ghost.Setup(m_SquashControllers, m_HopController, m_EyesController);

    m_MostRecentGhostPosition = position;

    var oldGhost = m_Ghosts[m_NextGhostIndex];
    if (oldGhost != null)
      oldGhost.Cleanup();

    m_Ghosts[m_NextGhostIndex] = ghost;
    m_NextGhostIndex = (m_NextGhostIndex + 1) % m_MaxGhosts;

    // If we end up going with a color-coded hero, then
    // the ghost will also have a ColoCode component. In
    // that case, here we should grab the ColorCode of
    // the ghost and Set it to match the hero's ColorCode
  }


  private void OnDestroy()
  {
    GlobalData.GhostModeEnabled -= OnGhostModeEnabled;
    GlobalData.GhostModeDisabled -= OnGhostModeDisabled;
  }
}
