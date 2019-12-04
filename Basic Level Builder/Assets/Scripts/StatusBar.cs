using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class StatusBar : MonoBehaviour
{
  public static string m_PreAwakeMessage = null;

  static TextMeshProUGUI m_Text;


  private void Awake()
  {
    m_Text = GetComponent<TextMeshProUGUI>();
  }


  private void Start()
  {
    if (m_PreAwakeMessage != null)
      Print(m_PreAwakeMessage);
  }


  public static void Print(object messageObject, bool highPriority = false, float duration = 0)
  {
    if (m_Text == null)
    {
      if (highPriority)
        PreAwakePrint(messageObject);

      return;
    }

    m_Text.text = messageObject == null ? "" : messageObject.ToString();

    if (duration > 0)
    {
      var clearSeq = ActionMaster.Actions.Sequence();
      clearSeq.Delay(duration);
      clearSeq.Call(Clear, m_Text.gameObject);
    }
  }


  public static void Clear()
  {
    m_Text.text = "";
  }


  static void PreAwakePrint(object messageObject)
  {
    m_PreAwakeMessage = messageObject.ToString();
  }
}
