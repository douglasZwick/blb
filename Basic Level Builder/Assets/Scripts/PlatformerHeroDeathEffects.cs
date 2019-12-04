using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlatformerHeroDeathEffects : MonoBehaviour
{
  public float m_DeathSpeed = 50;
  public float m_DeathAngularVelocity = 180;
  public float m_DeathDuration = 0.5f;
  public SpriteRenderer m_BodySpriteRenderer;

  [System.Serializable]
  public class Events
  {
    public HealthEvent DeathEffectsFinished;
  }

  public Events m_Events;

  Rigidbody2D m_Rigidbody;
  ActionSequence m_DeathSequence;
  Color m_DefaultBodyColor;


  private void Awake()
  {
    m_Rigidbody = GetComponent<Rigidbody2D>();
    m_DefaultBodyColor = m_BodySpriteRenderer.color;

    m_DeathSequence = ActionMaster.Actions.Sequence();
  }


  public void OnDied(HealthEventData eventData)
  {
    if (!enabled)
      return;

    enabled = false;

    GlobalData.DispatchHeroDied();

    m_Rigidbody.freezeRotation = false;
    var velocity = m_Rigidbody.velocity;
    velocity.y = m_DeathSpeed;
    var angularVelocity = Random.Range(-m_DeathAngularVelocity, m_DeathAngularVelocity);
    m_Rigidbody.velocity = velocity;
    m_Rigidbody.angularVelocity = angularVelocity;

    m_DeathSequence.Cancel();
    m_DeathSequence = ActionMaster.Actions.Sequence();
    m_DeathSequence.SpriteAlpha(m_BodySpriteRenderer.gameObject, 0, m_DeathDuration, new Ease(Ease.Quad.Out));
    m_DeathSequence.Call(SequenceEnd, gameObject);
  }


  void SequenceEnd()
  {
    m_Events.DeathEffectsFinished.Invoke(new HealthEventData());
  }


  public void OnReturned(HealthEventData eventData)
  {
    enabled = true;

    m_Rigidbody.velocity = Vector2.zero;
    m_Rigidbody.angularVelocity = 0;
    m_Rigidbody.freezeRotation = true;
    m_Rigidbody.SetRotation(0);

    m_BodySpriteRenderer.color = m_DefaultBodyColor;
  }
}
