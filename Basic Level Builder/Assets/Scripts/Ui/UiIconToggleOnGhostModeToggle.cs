using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UiIconToggleOnGhostModeToggle : MonoBehaviour
{
  public Sprite m_GhostModeEnabledSprite;
  public Sprite m_GhostModeDisabledSprite;

  Image m_Image;


  private void Awake()
  {
    m_Image = GetComponent<Image>();

    GlobalData.GhostModeEnabled += OnGhostModeEnabled;
    GlobalData.GhostModeDisabled += OnGhostModeDisabled;
  }


  void OnGhostModeEnabled()
  {
    m_Image.sprite = m_GhostModeEnabledSprite;
  }


  void OnGhostModeDisabled()
  {
    m_Image.sprite = m_GhostModeDisabledSprite;
  }


  private void OnDestroy()
  {
    GlobalData.GhostModeEnabled -= OnGhostModeEnabled;
    GlobalData.GhostModeDisabled -= OnGhostModeDisabled;
  }
}
