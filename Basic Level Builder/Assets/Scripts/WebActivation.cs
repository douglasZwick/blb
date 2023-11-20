using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebActivation : MonoBehaviour
{
  public enum WebActivationStatus
  {
    Default,
    InactiveOnWeb,
    InactiveOffWeb,
  }

  public WebActivationStatus m_ActivationStatus = WebActivationStatus.Default;


  void Start()
  {
    var onWeb = Application.platform == RuntimePlatform.WebGLPlayer;
    var inactiveOnWeb = m_ActivationStatus == WebActivationStatus.InactiveOnWeb;
    var inactiveOffWeb = m_ActivationStatus == WebActivationStatus.InactiveOffWeb;

    if (onWeb && inactiveOnWeb || !onWeb && inactiveOffWeb)
      gameObject.SetActive(false);
  }
}
