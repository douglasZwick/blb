using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ToggleVisibilityOnPlayModeToggle : MonoBehaviour
{
  SpriteRenderer m_SpriteRenderer;
  bool m_VisibleInEditMode;


  private void Awake()
  {
    m_SpriteRenderer = GetComponent<SpriteRenderer>();
    m_VisibleInEditMode = m_SpriteRenderer.enabled;

    GlobalData.PlayModeToggled += OnPlayModeToggled;
  }


  void OnPlayModeToggled(bool isInPlayMode)
  {
    m_SpriteRenderer.enabled = isInPlayMode ^ m_VisibleInEditMode;
  }


  private void OnDestroy()
  {
    GlobalData.PlayModeToggled -= OnPlayModeToggled;
  }
}
