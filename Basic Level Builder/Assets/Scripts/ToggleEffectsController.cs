using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// I should refactor this stuff if I ever decide to have
// multiple simultaneous effects for play mode toggling.
// Right now it's just a fade and it all happens here in
// this component, but if I want more, I can make each
// individual effect have its own component, and just have
// this one control the others (probably with UnityEvents)
[RequireComponent(typeof(Image))]
public class ToggleEffectsController : MonoBehaviour
{
  public Color m_IntoPlayModeColor = Color.black;
  public Color m_IntoEditModeColor = Color.black;
  public Color m_EditorStartColor = new Color(0.137f, 0.122f, 0.125f); //231F20
  public float m_IntoPlayModePreDuration = 0.15f;
  public float m_IntoPlayModeDelay = 0.1f;
  public float m_IntoPlayModePostDuration = 0.15f;
  public float m_IntoEditModePreDuration = 0.15f;
  public float m_IntoEditModeDelay = 0.1f;
  public float m_IntoEditModePostDuration = 0.15f;
  public float m_EditorStartFadeDuration = 0.25f;

  Image m_Image;
  ActionSequence m_FadeSequence;


  private void Awake()
  {
    m_Image = GetComponent<Image>();

    m_FadeSequence = ActionMaster.Actions.Sequence();

    GlobalData.PreToggleEffects += OnPreToggleEffects;
    GlobalData.PostToggleEffects += OnPostToggleEffects;
  }


  private void Start()
  {
    m_Image.enabled = true;
    m_Image.color = m_EditorStartColor;
    FadeOut(m_EditorStartFadeDuration);
  }


  void OnPreToggleEffects(PlayModeEventData eventData)
  {
    eventData.m_Handled = true;

    Color color;
    float fadeDuration;
    float delay;

    if (eventData.m_IsInPlayMode)
    {
      color = m_IntoEditModeColor;
      fadeDuration = m_IntoEditModePreDuration;
      delay = m_IntoEditModeDelay;
    }
    else
    {
      color = m_IntoPlayModeColor;
      fadeDuration = m_IntoPlayModePreDuration;
      delay = m_IntoPlayModeDelay;
    }

    FadeIn(color, fadeDuration, delay);
  }


  void OnPostToggleEffects(PlayModeEventData eventData)
  {
    eventData.m_Handled = true;

    float duration;

    if (eventData.m_IsInPlayMode)
      duration = m_IntoPlayModePostDuration;
    else
      duration = m_IntoEditModePostDuration;

    FadeOut(duration);
  }


  void FadeIn(Color color, float fadeDuration, float delay)
  {
    color.a = 0;
    m_Image.enabled = true;
    m_Image.color = color;

    m_FadeSequence.Cancel();
    m_FadeSequence = ActionMaster.Actions.Sequence();
    m_FadeSequence.UiAlpha(gameObject, 1, fadeDuration, new Ease(Ease.Quad.InOut));
    m_FadeSequence.Delay(delay);
    m_FadeSequence.Call(FadeInEnded, gameObject);
  }


  void FadeInEnded()
  {
    GlobalData.PreToggleEffectsEnded();
  }


  void FadeOut(float duration)
  {
    m_FadeSequence.UiAlpha(gameObject, 0, duration, new Ease(Ease.Quad.InOut));
    m_FadeSequence.Call(FadeOutEnded, gameObject);
  }


  void FadeOutEnded()
  {
    m_Image.enabled = false;

    GlobalData.PostToggleEffectsEnded();
  }


  private void OnDestroy()
  {
    GlobalData.PreToggleEffects -= OnPreToggleEffects;
    GlobalData.PostToggleEffects -= OnPostToggleEffects;
  }
}
