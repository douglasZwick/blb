using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ButtonFPSCapSwitch : MonoBehaviour
{
  public TextMeshProUGUI m_FPSIndicatorText;
  static int s_FPSCap = 0; // Current FPS cap | 0 = OFF | 1 = 60 | 2 = 144
   float m_timer;

  private void Awake()
  {
    GlobalData.FPSCapSwitched += SwitchFPS;

    // Ensure vsync is off to use frame rate caps
    QualitySettings.vSyncCount = 0;
    Application.targetFrameRate = 300;
  }

  // Called by Unity UI Button
  public void SwitchFPSButton()
  {
    GlobalData.DispatchFPSCapSwitched();
  }

  /*
  * Toggles between different FPS caps
  * 0 = OFF | 1 = 60 | 2 = 144
  */
  private void SwitchFPS()
  {
    switch (s_FPSCap)
    {
      case 0:
        // Set to 60 cap
        s_FPSCap = 1;
        Application.targetFrameRate = 60;
        m_FPSIndicatorText.text = "OFF";
        m_FPSIndicatorText.text = "60";
        break;

      case 1:
        // Set to 144 cap
        s_FPSCap = 2;
        Application.targetFrameRate = 144;
        m_FPSIndicatorText.text = "144";
        break;

      case 2:
        // Set to OFF and return targetFrameRate to it's default
        s_FPSCap = 0;
        Application.targetFrameRate = 300;
        m_FPSIndicatorText.text = "OFF";
        break;

      default:
        break;
    }
  }

  private void OnDestroy()
  {
    GlobalData.FPSCapSwitched -= SwitchFPS;
  }

  /*
  * FPS Counter, unimplemented since I don't wanna mess 
  * with the UI too much right now
  * */
  private void Update()
  {
    if (Time.unscaledTime > m_timer)
    {
      int fps = (int)(1f / Time.unscaledDeltaTime);
      StatusBar.Print("FPS: " + fps);
      m_timer = Time.unscaledTime + 1;
    }
  }
  


}

