using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(ColorCode))]
public class SetTextOnColorSet : MonoBehaviour
{
  public List<TextMeshPro> m_TextsToSet;
  public List<TextMeshProUGUI> m_UiTextsToSet;

  ColorCode m_ColorCode;


  private void Awake()
  {
    m_ColorCode = GetComponent<ColorCode>();

    m_ColorCode.ColorSet += OnColorSet;
    GlobalData.ColorblindModeEnabled += OnColorblindModeEnabled;
    GlobalData.ColorblindModeDisabled += OnColorblindModeDisabled;

    if (ColorblindToggler.s_IsInColorblindMode)
      SetTextActiveStates(true);
  }


  void OnColorSet(TileColor tileColor)
  {
    var str = ColorCode.Strings[tileColor];

    foreach (var text in m_TextsToSet)
      text.SetText(str);
    foreach (var uiText in m_UiTextsToSet)
      uiText.SetText(str);
  }


  void OnColorblindModeEnabled()
  {
    SetTextActiveStates(true);
  }


  void OnColorblindModeDisabled()
  {
    SetTextActiveStates(false);
  }


  void SetTextActiveStates(bool active)
  {
    foreach (var text in m_TextsToSet)
      text.gameObject.SetActive(active);
    foreach (var uiText in m_UiTextsToSet)
      uiText.gameObject.SetActive(active);
  }


  private void OnDestroy()
  {
    m_ColorCode.ColorSet -= OnColorSet;
    GlobalData.ColorblindModeEnabled -= OnColorblindModeEnabled;
    GlobalData.ColorblindModeDisabled -= OnColorblindModeDisabled;
  }
}
