using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ButtonFpsCapSwitch : MonoBehaviour
{
  public TextMeshProUGUI m_FpsIndicatorText;
  static int s_FpsCap = 0; // Current FPS cap | 0 = OFF | 1 = 60 | 2 = 144

  private void Awake()
  {
    GlobalData.FpsCapSwitched += OnFpsCapSwitched;

    // Ensure vsync is off to use frame rate caps
    QualitySettings.vSyncCount = 0;
    Application.targetFrameRate = 300;
  }

  // Called by Unity UI Button
  public void SwitchFPSButton()
  {
    GlobalData.DispatchFpsCapSwitched();
  }

  /*
  * Toggles between different FPS caps
  * 0 = OFF | 1 = 60 | 2 = 144
  */
  private void OnFpsCapSwitched()
  {
    switch (s_FpsCap)
    {
      case 0:
        // Set to 60 cap
        s_FpsCap = 1;
        Application.targetFrameRate = 60;
        m_FpsIndicatorText.text = "OFF";
        m_FpsIndicatorText.text = "60";
        break;

      case 1:
        // Set to 144 cap
        s_FpsCap = 2;
        Application.targetFrameRate = 144;
        m_FpsIndicatorText.text = "144";
        break;

      case 2:
        // Set to OFF and return targetFrameRate to it's default
        s_FpsCap = 0;
        Application.targetFrameRate = 300;
        m_FpsIndicatorText.text = "OFF";
        break;

      default:
        break;
    }
  }

  private void OnDestroy()
  {
    GlobalData.FpsCapSwitched -= OnFpsCapSwitched;
  }

  /*
  * FPS Counter, unimplemented since I don't wanna mess 
  * with the UI too much right now
  float m_timer;
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

