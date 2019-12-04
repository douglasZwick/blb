using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class Greeter : MonoBehaviour
{
  public enum MessageType
  {
    OrdinaryGreeting,
    SystemInfo,
    PageUrl,
  }

  public MessageType m_MessageType;
  public string m_Message = "Welcome to the Basic Level Builder";

  [DllImport("__Internal")]
  private static extern string GetUrl();


  void Start()
  {
    switch (m_MessageType)
    {
      default:  // MessageType.OrdinaryGreeting
        PrintOrdinaryGreeting();
        break;
      case MessageType.SystemInfo:
        PrintSystemInfo();
        break;
      case MessageType.PageUrl:
        PrintPageUrl();
        break;
    }
  }


  void PrintOrdinaryGreeting()
  {
    var ordinaryGreeting = m_Message + " v" + GlobalData.s_Version;
    StatusBar.Print(ordinaryGreeting);
  }


  void PrintSystemInfo()
  {
    var systemInfo = $"Device type: {SystemInfo.deviceType} | Device name: {SystemInfo.deviceName} | " +
      $"Device model: {SystemInfo.deviceModel}";
    StatusBar.Print(systemInfo);
  }


  void PrintPageUrl()
  {
    if (Application.platform != RuntimePlatform.WebGLPlayer)
    {
      Debug.LogWarning("The message type of MessageType.PageUrl is only supported on the WebGL platform. " +
        "Defaulting to printing the ordinary greeting.");
      PrintOrdinaryGreeting();

      return;
    }

    var url = GetUrl();
    var urlMessage = $"Current page URL: {url}";
    StatusBar.Print(urlMessage);
  }
}
