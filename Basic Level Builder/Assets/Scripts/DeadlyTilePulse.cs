using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DeadlyTilePulse : MonoBehaviour
{
  SpriteRenderer m_SpriteRenderer;


  void Start()
  {
    m_SpriteRenderer = GetComponent<SpriteRenderer>();
  }


  void Update()
  {
    m_SpriteRenderer.color = DeadlyTilePulseMaster.GetCurrentColor();
  }
}
