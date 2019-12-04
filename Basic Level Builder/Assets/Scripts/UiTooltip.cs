using System.Collections;
using System.Collections.Generic;
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
