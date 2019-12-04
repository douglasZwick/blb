using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HopController : MonoBehaviour
{
  public Transform m_HopNode;
  public float m_HopHeight = 0.25f;
  public float m_HopDuration = 0.15f;
  public bool m_ClampHeight = true;

  ActionSequence m_HopSequence;
  float m_PreviousX;
  float m_HopDefaultY;


  private void Awake()
  {
    m_PreviousX = m_HopNode.position.x;
    m_HopDefaultY = m_HopNode.localPosition.y;

    m_HopSequence = ActionMaster.Actions.Sequence();
  }


  public void OnMovedOnGround(MovementEventData eventData)
  {
    if (!enabled || m_HopSequence.Active)
      return;

    Hop(eventData.m_NormalizedDelta);
  }


  public void Hop(float normalizedDelta = 1)
  {
    if (m_ClampHeight)
      normalizedDelta = Mathf.Clamp(normalizedDelta, -1, 1);

    var normalizedMagnitude = Mathf.Abs(normalizedDelta);
    var height = m_HopHeight * normalizedMagnitude;
    var inTarget = m_HopDefaultY + height;
    var outTarget = m_HopDefaultY;
    var duration = (m_HopDuration / 2) * Mathf.Sqrt(normalizedMagnitude);

    m_HopSequence = ActionMaster.Actions.Sequence();
    m_HopSequence.MoveLocalY(m_HopNode.gameObject, inTarget, duration, new Ease(Ease.Quad.Out));
    m_HopSequence.MoveLocalY(m_HopNode.gameObject, outTarget, duration, new Ease(Ease.Quad.In));
  }


  public void OnSquashStarted()
  {
    enabled = false;
  }


  public void OnSquashEnded()
  {
    enabled = true;
  }


  public void OnUsedTeleporter(TeleportEventData eventData)
  {
    m_PreviousX = eventData.m_ToPosition.x;
  }
}
