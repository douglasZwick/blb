using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayTimer : MonoBehaviour
{
  DateTime m_StartTime;


  private void Awake()
  {
    GlobalData.ModeStarted += OnModeStarted;
  }


  void OnModeStarted(bool isInPlayMode)
  {
    if (isInPlayMode)
      StartClock();
  }


  void StartClock()
  {
    m_StartTime = DateTime.Now;
  }


  public void StopClock()
  {
    var endTime = DateTime.Now;
    var difference = endTime - m_StartTime;
    var hours = difference.Hours;
    var minutes = difference.Minutes;
    var seconds = difference.TotalSeconds;

    var formatSpecifier = "";

    if (hours > 0)
      formatSpecifier = @"h\:mm\:ss\.fff";
    else
      formatSpecifier = @"mm\:ss\.fff";

    var timeString = difference.ToString(formatSpecifier);
    var fullMessage = $"Level completed in <b><color=#ffff00>{timeString}</color></b>";

    StatusBar.Print(fullMessage);
  }


  private void OnDestroy()
  {
    GlobalData.ModeStarted -= OnModeStarted;
  }
}
