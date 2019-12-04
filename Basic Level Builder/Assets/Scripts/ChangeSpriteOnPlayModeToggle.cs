using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ChangeSpriteOnPlayModeToggle : MonoBehaviour
{
  public Sprite m_PlayModeSprite;
  public Color m_PlayModeColor = Color.white;

  SpriteRenderer m_SpriteRenderer;
  Sprite m_EditModeSprite;
  Color m_EditModeColor;


  private void Awake()
  {
    m_SpriteRenderer = GetComponent<SpriteRenderer>();
    m_EditModeSprite = m_SpriteRenderer.sprite;
    m_EditModeColor = m_SpriteRenderer.color;

    GlobalData.PlayModeToggled += OnPlayModeToggled;
  }


  void OnPlayModeToggled(bool isInPlayMode)
  {
    if (isInPlayMode)
    {
      m_SpriteRenderer.sprite = m_PlayModeSprite;
      m_SpriteRenderer.color = m_PlayModeColor;
    }
    else
    {
      m_SpriteRenderer.sprite = m_EditModeSprite;
      m_SpriteRenderer.color = m_EditModeColor;
    }
  }


  private void OnDestroy()
  {
    GlobalData.PlayModeToggled -= OnPlayModeToggled;
  }
}
