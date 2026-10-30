using UnityEngine;

public class ScoreSprite : MonoBehaviour
{
  public Rigidbody2D m_Rigidbody;
  public SpriteRenderer m_SpriteRenderer;
  public Vector2 m_InitialVelocity = new(0, 5);
  public Vector2 m_FinalVelocity = new(0, 1);
  public float m_VelocityActionDuration = 0.1f;
  public float m_SpriteFadeDelay = 4;
  public float m_SpriteFadeDuration = 0.5f;


  void Start()
  {
    m_Rigidbody.linearVelocity = m_InitialVelocity;

    var endColor = m_SpriteRenderer.color;
    endColor.a = 0;

    var actionSeq = ActionMaster.Actions.Sequence();
    {
      var actionGrp = actionSeq.Group();
      {
        actionGrp.Velocity2D(m_Rigidbody.gameObject, m_FinalVelocity,
          m_VelocityActionDuration, new Ease(Ease.Quad.Out));
        var fadeSeq = actionGrp.Sequence();
        {
          fadeSeq.Delay(m_SpriteFadeDelay);
          fadeSeq.SpriteColor(m_SpriteRenderer.gameObject, endColor,
            m_SpriteFadeDuration, new Ease(Ease.Quad.InOut));
        }
      }
      actionSeq.Destroy(gameObject);
    }
  }
}
