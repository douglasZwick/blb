using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ColorCode))]
public class Key : MonoBehaviour
{
  public TileColor m_Color { get; private set; }
  bool m_Collected = false;


  private void Awake()
  {
    GlobalData.KeyCollected += OnKeyCollected;
  }


  public void OnColorCodeSet(ColorCodeEventData eventData)
  {
    m_Color = eventData.m_Color;
  }


  public void AttemptCollect(KeyCollector collector)
  {
    if (!m_Collected)
      Collect(collector);
  }

  
  void Collect(KeyCollector collector)
  {
    m_Collected = true;

    collector.Collected(this);

    Destroy(gameObject);
  }


  void OnKeyCollected(TileColor keyColor)
  {
    if (keyColor == m_Color)
      Destroy(gameObject);
  }


  private void OnDestroy()
  {
    GlobalData.KeyCollected -= OnKeyCollected;
  }
}
