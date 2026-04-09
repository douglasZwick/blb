/***************************************************
Authors:        Douglas Zwick, Brenden Epp
Last Updated:   3/24/2025

Copyright 2018-2025, DigiPen Institute of Technology
***************************************************/

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UiTooltip : MonoBehaviour
{
  public Image m_Background;
  public Image m_Outline;
  public TextMeshProUGUI m_Text;

  public void SetBackgroundColor(Color color)
  {
    m_Background.color = color;
  }


  public void SetOutlineColor(Color color)
  {
    m_Outline.color = color;
  }


  public void SetTextColor(Color color)
  {
    m_Text.color = color;
  }


  public void SetText(string text)
  {
    m_Text.text = text;
  }
}
