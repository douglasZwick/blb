using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteColorCycler : MonoBehaviour
{
  public float m_Period = 3;
  [Range(0, 1)]
  public float m_InitialH = 0;
  [Range(0, 1)]
  public float m_S = 0.95f;
  [Range(0, 1)]
  public float m_V = 0.9f;

  SpriteRenderer m_SpriteRenderer;
  float m_Timer;


  void Start()
  {
    m_SpriteRenderer = GetComponent<SpriteRenderer>();

    m_Timer = m_Period * m_InitialH;
  }


  void Update()
  {
    var h = m_Timer / m_Period;
    var color = Color.HSVToRGB(h, m_S, m_V);
    color.a = m_SpriteRenderer.color.a;
    m_SpriteRenderer.color = color;

    m_Timer = (m_Timer + Time.deltaTime) % m_Period;
  }
}
