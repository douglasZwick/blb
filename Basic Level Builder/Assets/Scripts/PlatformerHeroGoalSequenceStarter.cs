using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EyesController))]
public class PlatformerHeroGoalSequenceStarter : MonoBehaviour
{
  public PlatformerHeroGoalSequence m_SequencePrefab;

  Transform m_Transform;
  EyesController m_EyesController;
  PlatformerHeroGoalSequence m_Sequence;


  private void Awake()
  {
    m_Transform = transform;
    m_EyesController = GetComponent<EyesController>();
  }


  public void OnActivatedGoal(GoalEventData eventData)
  {
    if (!gameObject.activeInHierarchy)
      return;

    var spawnPosition = m_Transform.position;
    var spawnRotation = m_Transform.rotation;
    m_Sequence = Instantiate(m_SequencePrefab, spawnPosition, spawnRotation);
    m_Sequence.m_EyesController.m_LookAngle = m_EyesController.m_LookAngle;

    m_Sequence.Begin();

    gameObject.SetActive(false);
  }


  private void OnDestroy()
  {
    if (m_Sequence != null)
      Destroy(m_Sequence.gameObject);
  }
}
