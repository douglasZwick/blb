using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EyesController))]
public class PlatformerHeroGoalSequence : MonoBehaviour
{
  public float m_RiseDuration = 2;
  public float m_RiseHeight = 4;
  public float m_PreWinkDelay = 0.1f;
  public float m_WinkDuration = 0.06f;
  public float m_TiltAngle = 15;
  public float m_PopScalar = 2;
  public float m_PopEndingScalar = 1.5f;
  public float m_ScalePopDuration = 0.1f;
  public float m_IrisIntermediateSize = 4;
  public float m_PreIrisFinishDelay = 0.5f;
  public float m_IrisFinishDuration = 1;
  public float m_FinalDelay = 2;
  public float m_NumberOfTurns = 4;
  public float m_SpinExponent = 8;
  public ParticleSystem m_CircleParticles;
  public Transform m_ScalePopNode;
  public Transform m_Iris;

  Transform m_Transform;
  [HideInInspector]
  public EyesController m_EyesController;
  float m_TargetLookAngle;
  List<ParticleSystem> m_FireworksSystems = new List<ParticleSystem>();


  private void Awake()
  {
    m_Transform = transform;
    m_EyesController = GetComponent<EyesController>();
    m_EyesController.enabled = false;
    m_TargetLookAngle = 90 + m_NumberOfTurns * 360;

    var goalFX = GameObject.FindGameObjectsWithTag("GoalFX");
    foreach (var fx in goalFX)
      m_FireworksSystems.Add(fx.GetComponent<ParticleSystem>());
  }


  public void Begin()
  {
    var quadInOut = new Ease(Ease.Quad.InOut);
    var spinEase = new Ease(Ease.Power.In, m_SpinExponent);

    var mainSeq = ActionMaster.Actions.Sequence();
    mainSeq.Call(SetOffFireworks, gameObject);
    var riseGrp = mainSeq.Group();
    var irisIntermediateScale = new Vector3(m_IrisIntermediateSize, m_IrisIntermediateSize, 1);
    var riseY = m_Transform.localPosition.y + m_RiseHeight;
    riseGrp.MoveLocalY(gameObject, riseY, m_RiseDuration, quadInOut);
    riseGrp.TurnFacing(gameObject, m_TargetLookAngle, m_RiseDuration, spinEase);
    riseGrp.ParticleEmissionRate(m_CircleParticles.gameObject, 0, m_RiseDuration, new Ease(Ease.Quad.Out));
    riseGrp.Scale(m_Iris.gameObject, irisIntermediateScale, m_RiseDuration, quadInOut);
    mainSeq.Delay(m_PreWinkDelay);
    var winkGrp = mainSeq.Group();
    var winkGO = m_EyesController.m_LeftBlinkRoot.gameObject;
    var winkEnd = m_EyesController.m_BlinkScale;
    winkGrp.Scale(winkGO, winkEnd, m_WinkDuration, quadInOut);
    winkGrp.Rotate2D(gameObject, m_TiltAngle, m_WinkDuration, quadInOut);
    winkGrp.Call(ScalePop, gameObject);
    mainSeq.Delay(m_PreIrisFinishDelay);
    mainSeq.Scale(m_Iris.gameObject, Vector3.forward, m_IrisFinishDuration, new Ease(Ease.Back.In));
    mainSeq.Delay(m_FinalDelay);
    mainSeq.Call(SequenceEnd, gameObject);
  }


  void SetOffFireworks()
  {
    var lifetime = m_RiseDuration + m_PreWinkDelay;

    foreach (var firework in m_FireworksSystems)
    {
      var mainModule = firework.main;
      mainModule.startLifetimeMultiplier = lifetime;
      firework.Play();
    }
  }


  void ExplodeFireworks()
  {
    foreach (var firework in m_FireworksSystems)
    {
      firework.Clear();
      firework.TriggerSubEmitter(1);
    }
  }


  void ScalePop()
  {
    var oldScale = m_ScalePopNode.localScale;
    var targetScale = oldScale * m_PopEndingScalar;
    m_ScalePopNode.localScale = oldScale * m_PopScalar;
    ActionMaster.Actions.Scale(m_ScalePopNode.gameObject, targetScale, m_ScalePopDuration, new Ease(Ease.Quad.Out));
  }


  void SequenceEnd()
  {
    foreach (var firework in m_FireworksSystems)
      firework.Clear();

    GlobalData.TogglePlayMode();
  }
}
