using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoosterLightsAnimator : MonoBehaviour
{
  public SpriteRenderer m_Light0;
  public SpriteRenderer m_Light1;
  public SpriteRenderer m_Light2;


  private void Awake()
  {
    BoosterLightsAnimationMaster.Light0On += OnLight0On;
    BoosterLightsAnimationMaster.Light1On += OnLight1On;
    BoosterLightsAnimationMaster.Light2On += OnLight2On;
    BoosterLightsAnimationMaster.Light0Off += OnLight0Off;
    BoosterLightsAnimationMaster.Light1Off += OnLight1Off;
    BoosterLightsAnimationMaster.Light2Off += OnLight2Off;
  }


  void OnLight0On(float duration)
  {
    TurnLightOn(m_Light0, duration);
  }


  void OnLight1On(float duration)
  {
    TurnLightOn(m_Light1, duration);
  }


  void OnLight2On(float duration)
  {
    TurnLightOn(m_Light2, duration);
  }


  void OnLight0Off(float duration)
  {
    TurnLightOff(m_Light0, duration);
  }


  void OnLight1Off(float duration)
  {
    TurnLightOff(m_Light1, duration);
  }


  void OnLight2Off(float duration)
  {
    TurnLightOff(m_Light2, duration);
  }


  void TurnLightOn(SpriteRenderer light, float duration)
  {
    ActionMaster.Actions.SpriteAlpha(light.gameObject, 1, duration, new Ease(Ease.Quad.InOut));
  }


  void TurnLightOff(SpriteRenderer light, float duration)
  {
    ActionMaster.Actions.SpriteAlpha(light.gameObject, 0, duration, new Ease(Ease.Power.Out, 10));
  }


  private void OnDestroy()
  {
    BoosterLightsAnimationMaster.Light0On -= OnLight0On;
    BoosterLightsAnimationMaster.Light1On -= OnLight1On;
    BoosterLightsAnimationMaster.Light2On -= OnLight2On;
    BoosterLightsAnimationMaster.Light0Off -= OnLight0Off;
    BoosterLightsAnimationMaster.Light1Off -= OnLight1Off;
    BoosterLightsAnimationMaster.Light2Off -= OnLight2Off;
  }
}
