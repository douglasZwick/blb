using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoosterLightsAnimationMaster : MonoBehaviour
{
  public delegate void FloatEvent(float value);
  public static event FloatEvent Light0On;
  public static event FloatEvent Light1On;
  public static event FloatEvent Light2On;
  public static event FloatEvent Light0Off;
  public static event FloatEvent Light1Off;
  public static event FloatEvent Light2Off;

  public float m_StepDelay = 0.5f;
  public float m_OnDuration = 0.3f;
  public float m_OffDuration = 0.3f;


  private void Awake()
  {
    ExecuteSequence();
  }


  void ExecuteSequence()
  {
    var mainSeq = ActionMaster.Actions.Sequence();
    var go = gameObject;

    mainSeq.Call(TurnLight0On, go);
    mainSeq.Delay(m_StepDelay);
    mainSeq.Call(TurnLight1On, go);
    mainSeq.Delay(m_StepDelay);
    mainSeq.Call(TurnLight2On, go);

    mainSeq.Delay(m_StepDelay * 3);

    mainSeq.Call(TurnLight0Off, go);
    mainSeq.Delay(m_StepDelay);
    mainSeq.Call(TurnLight1Off, go);
    mainSeq.Delay(m_StepDelay);
    mainSeq.Call(TurnLight2Off, go);

    mainSeq.Call(ExecuteSequence, go);
  }


  void TurnLight0On() { Light0On?.Invoke(m_OnDuration); }
  void TurnLight1On() { Light1On?.Invoke(m_OnDuration); }
  void TurnLight2On() { Light2On?.Invoke(m_OnDuration); }
  void TurnLight0Off() { Light0Off?.Invoke(m_OffDuration); }
  void TurnLight1Off() { Light1Off?.Invoke(m_OffDuration); }
  void TurnLight2Off() { Light2Off?.Invoke(m_OffDuration); }
}
