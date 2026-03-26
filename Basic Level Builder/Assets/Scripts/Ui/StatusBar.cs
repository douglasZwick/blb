/***************************************************
Authors:        Douglas Zwick, Brenden Epp
Last Updated:   5/24/2026

Copyright 2018-2026, DigiPen Institute of Technology
***************************************************/

using UnityEngine;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(TextMeshProUGUI))]
public class StatusBar : MonoBehaviour
{
  public static string s_PreAwakeMessage = null;

  static TextMeshProUGUI s_Text;
  static Image s_Foreground;
  static string s_WarningColorCode = "#FFF820";
  static string s_ErrorColorCode = "#FF2028";
  static Color s_ForegroundColor;
  static Color s_FlashColor = new(0.25f, 0.25f, 0.25f, 0.5f);
  static float s_MessageFlashTime = 0;

  private void Awake()
  {
    s_Text = GetComponent<TextMeshProUGUI>();
    s_Foreground = GetComponentInParent<Image>();
    s_ForegroundColor = s_Foreground.color;
  }


  private void Start()
  {
    if (s_PreAwakeMessage != null)
      Print(s_PreAwakeMessage);
  }

  private void Update()
  {
    if (s_MessageFlashTime > 0)
    {
      s_MessageFlashTime -= Time.deltaTime;
      var flashAmount = Mathf.Clamp01(s_MessageFlashTime);
      s_Foreground.color = Color.Lerp(s_ForegroundColor, s_FlashColor, flashAmount);
    }
  }


  public static void  Print(object messageObject, bool highPriority = false, float duration = 0)
  {
    if (messageObject == null)
      return;

    PrintHelper(messageObject.ToString(), highPriority, duration);
  }

  public static void SilentPrint(object messageObject, bool highPriority = false, float duration = 0)
  {
    if (messageObject == null)
      return;

    PrintHelper(messageObject.ToString(), highPriority, duration, isSilent: true);
  }

  public static void Log(object messageObject, object extendedMessageObject = null)
  {
    if (messageObject == null)
      return;

    var message = messageObject.ToString();
    var formattedMessage = $"<color={s_WarningColorCode}>" + message + "</color>";
    PrintHelper(formattedMessage, highPriority: true, duration: 0);
    var extendedMessage = extendedMessageObject != null ? " " + extendedMessageObject.ToString() : "";
    Debug.Log(message + extendedMessage);
  }

  public static void Warning(object messageObject, object extendedMessageObject = null)
  {
    if (messageObject == null)
      return;

    var message = messageObject.ToString();
    var formattedMessage = $"<color={s_WarningColorCode}>" + message + "</color>";
    PrintHelper(formattedMessage, highPriority: true, duration: 0);
    var extendedMessage = extendedMessageObject != null ? " " + extendedMessageObject.ToString() : "";
    Debug.LogWarning(message + extendedMessage);
  }


  public static void Error(object messageObject, object extendedMessageObject = null)
  {
    if (messageObject == null)
      return;

    var message = messageObject.ToString();
    var formattedMessage = $"<color={s_ErrorColorCode}>" + message + "</color>";
    PrintHelper(formattedMessage, highPriority: true, duration: 0);
    var extendedMessage = extendedMessageObject != null ? " " + extendedMessageObject.ToString() : "";
    Debug.LogError(message + extendedMessage);
  }


  static void PrintHelper(string message, bool highPriority, float duration, bool isSilent = false)
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

    if (!isSilent)
      s_MessageFlashTime = 1f;
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
