using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoonTileLogic : MonoBehaviour
{
  public GameObject m_GoonPrefab;
  public List<SpriteRenderer> m_SpriteRenderers;

  Transform m_Transform;
  TileDirection m_TileDirection;
  GameObject m_Goon;


  private void Awake()
  {
    m_Transform = transform;
    m_TileDirection = GetComponent<TileDirection>();

    GlobalData.PlayModePreToggle += OnPlayModePreToggle;
    GlobalData.PlayModeToggled += OnPlayModeToggled;
  }


  void OnPlayModePreToggle(bool isInPlayMode)
  {
    if (isInPlayMode)
    {
      m_Goon = Instantiate(m_GoonPrefab, m_Transform.position, Quaternion.identity);

      // set direction stuff here
      var tileDirection = m_Goon.GetComponent<TileDirection>();
      if (tileDirection != null)
        tileDirection.Initialize(m_TileDirection.m_Direction);
    }
  }


  void OnPlayModeToggled(bool isInPlayMode)
  {
    foreach (var spriteRenderer in m_SpriteRenderers)
    {
      spriteRenderer.enabled = !isInPlayMode;
    }

    if (!isInPlayMode && m_Goon != null)
      Destroy(m_Goon);
  }


  private void OnDestroy()
  {
    GlobalData.PlayModePreToggle -= OnPlayModePreToggle;
    GlobalData.PlayModeToggled -= OnPlayModeToggled;
  }
}
