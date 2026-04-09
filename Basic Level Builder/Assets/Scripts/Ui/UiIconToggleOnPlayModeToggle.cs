using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiIconToggleOnPlayModeToggle : MonoBehaviour
{
  public Sprite m_EditModeSprite;
  public Sprite m_PlayModeSprite;

  Image m_Image;


  void Start()
  {
    m_Image = GetComponent<Image>();

    GlobalData.PlayModeToggled += OnPlayModeToggled;
  }


  void OnPlayModeToggled(bool _IsInPlayMode)
  {
    m_Image.sprite = _IsInPlayMode ? m_PlayModeSprite : m_EditModeSprite;
  }


  void OnDestroy()
  {
    GlobalData.PlayModeToggled -= OnPlayModeToggled;
  }
}
