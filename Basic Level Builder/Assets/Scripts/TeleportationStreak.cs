using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class TeleportationStreak : MonoBehaviour
{
  public float m_InitialThickness = 1;
  public float m_Duration = 1;
  public float m_Exponent = 3;
  public float m_Saturation = 0.8f;

  Transform m_Transform;
  SpriteRenderer m_SpriteRenderer;


  private void Awake()
  {
    m_Transform = transform;
    m_SpriteRenderer = GetComponent<SpriteRenderer>();
  }


  public void Setup(Vector3 from, Vector3 to, Color teleporterColor)
  {
    var center = (from + to) / 2;
    var difference = to - from;
    var direction = difference.normalized;
    var distance = difference.magnitude;
    var angle = Vector3.SignedAngle(Vector3.right, direction, Vector3.forward);
    var rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    var initialScale = new Vector3(distance, 1, 1);
    var finalScale = new Vector3(distance, 0, 1);

    Color.RGBToHSV(teleporterColor, out float h, out float s, out float v);
    s = m_Saturation;
    var color = Color.HSVToRGB(h, s, v);
    m_SpriteRenderer.color = color;

    m_Transform.position = center;
    m_Transform.rotation = rotation;
    m_Transform.localScale = initialScale;

    var ease = new Ease(Ease.Power.Out, m_Exponent);
    var seq = ActionMaster.Actions.Sequence();
    var grp = seq.Group();
    grp.Scale(gameObject, finalScale, m_Duration, ease);
    grp.SpriteAlpha(gameObject, 1, m_Duration, ease);
    seq.Call(Cleanup, gameObject);
  }


  void Cleanup()
  {
    Destroy(gameObject);
  }
}
