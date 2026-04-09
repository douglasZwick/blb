/***************************************************
Authors:        Brenden Epp
Last Updated:   11/22/2025

Copyright 2018-2025, DigiPen Institute of Technology
***************************************************/

using UnityEngine;
using System.IO;

public class ErrorLogger : MonoBehaviour
{
  static public bool m_LogDefaultLogs = false;

  void OnEnable()
  {
    // Subscribe to the log message event
    Application.logMessageReceived += HandleLog;
  }

  void OnDisable()
  {
    // Unsubscribe when the object is disabled or destroyed
    Application.logMessageReceived -= HandleLog;
  }

  void HandleLog(string logString, string stackTrace, LogType type)
  {
    // Only log errors and exceptions
    if (type is LogType.Error or LogType.Exception or LogType.Warning || (m_LogDefaultLogs && type is LogType.Log))
    {
      // Format the log entry
      string logEntry = $"[{System.DateTime.Now}] [{type}] {logString}\n{stackTrace}\n";

      // Append the log entry to the file
      File.AppendAllText(Path.Combine(FileSystem.Instance.GetDirectoryPath(), "ErrorLog.txt"), logEntry);

      // Log exception so it will be sent to the cloud
      // This will make a duplicate log in the txt, but we got to do what we got to do to make things work
      // Don't log if the log type is an exception since exeptions are auto logged to cloud and otherwise would make this an infinte loop
      if (type is not LogType.Exception and not LogType.Log)
        Debug.LogException(new System.Exception(logEntry));
    }
  }
}