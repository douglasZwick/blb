using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CameraController))]
public class ActionZoomOnBoostTriggered : MonoBehaviour
{
  CameraController m_CameraController;


  private void Awake()
  {
    m_CameraController = GetComponent<CameraController>();
  }


  // Just to get the check box
  private void Start() { }


  public void OnFoundTarget(CameraControllerEventData eventData)
  {
    var boostResponder = eventData.m_CameraTarget.GetComponent<BoostResponder>();

    if (boostResponder == null)
      return;

    boostResponder.m_Events.BoostTriggered.AddListener(OnBoostTriggered);
  }


  public void OnBoostTriggered(BoostLogicEventData eventData)
  {
    if (!enabled)
      return;

    m_CameraController.AttemptActionZoom();
  }
}
