using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class StatusBar : MonoBehaviour
{
  public static string s_PreAwakeMessage = null;

  static TextMeshProUGUI s_Text;
  static string s_WarningColorCode = "#FFF820";
  static string s_ErrorColorCode = "#FF2028";


  private void Awake()
  {
    s_Text = GetComponent<TextMeshProUGUI>();
  }


  private void Start()
  {
    if (s_PreAwakeMessage != null)
      Print(s_PreAwakeMessage);
  }


  public static void Print(object messageObject, bool highPriority = false, float duration = 0)
  {
    if (messageObject == null)
      return;

    PrintHelper(messageObject.ToString(), highPriority, duration);
  }


  public static void Warning(object messageObject)
  {
    if (messageObject == null)
      return;

    var message = messageObject.ToString();
    var formattedMessage = $"<color={s_WarningColorCode}>" + message + "</color>";
    PrintHelper(formattedMessage, highPriority: true, duration: 0);
    Debug.LogWarning(message);
  }


  public static void Error(object messageObject)
  {
    if (messageObject == null)
      return;

    var message = messageObject.ToString();
    var formattedMessage = $"<color={s_ErrorColorCode}>" + message + "</color>";
    PrintHelper(formattedMessage, highPriority: true, duration: 0);
    Debug.LogError(message);
  }


  static void PrintHelper(string message, bool highPriority, float duration)
  {
    if (s_Text == null)
    {
      if (highPriority)
        PreAwakePrint(message);

      return;
    }

    s_Text.text = message;

    if (duration > 0)
    {
      var clearSeq = ActionMaster.Actions.Sequence();
      clearSeq.Delay(duration);
      clearSeq.Call(Clear, s_Text.gameObject);
    }
  }


  public static void Clear()
  {
    s_Text.text = "";
  }


  static void PreAwakePrint(object messageObject)
  {
    s_PreAwakeMessage = messageObject.ToString();
  }
}
