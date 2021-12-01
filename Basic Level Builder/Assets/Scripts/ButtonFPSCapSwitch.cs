using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ButtonFPSCapSwitch : MonoBehaviour
{
  public TextMeshProUGUI m_FPSIndicatorText;
  // float m_timer;

  /*
   * Called by UI Button
   * Switches FPS cap and updates UI text indication
   */
  public void SwitchFPS()
  {
    switch (OperationSystem.SwitchFPS())
    {
      case 0: // OFF
        m_FPSIndicatorText.text = "OFF";
        break;

      case 1: // 60
        m_FPSIndicatorText.text = "60";
        break;

      case 2: // 144
        m_FPSIndicatorText.text = "144";
        break;

      default:
        break;
    }
  }

  /*
  * FPS Counter, unimplemented since I don't wanna mess 
  * with the UI too much right now
  private void Update()
  {
    if (Time.unscaledTime > m_timer)
    {
      int fps = (int)(1f / Time.unscaledDeltaTime);
      StatusBar.Print("FPS: " + fps);
      m_timer = Time.unscaledTime + 1;
    }
  }
  */
  

}

