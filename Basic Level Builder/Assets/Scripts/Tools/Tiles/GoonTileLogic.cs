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
  bool m_HeroHasReturned = false;


  private void Awake()
  {
    m_Transform = transform;
    m_TileDirection = GetComponent<TileDirection>();

    GlobalData.PlayModePreToggle += OnPlayModePreToggle;
    GlobalData.PlayModeToggled += OnPlayModeToggled;
    GlobalData.HeroReturned += OnHeroReturned;
  }


  void OnPlayModePreToggle(bool isInPlayMode)
  {
    m_HeroHasReturned = false;

    if (isInPlayMode)
      CreateGoon();
  }


  void OnPlayModeToggled(bool isInPlayMode)
  {
    foreach (var spriteRenderer in m_SpriteRenderers)
      spriteRenderer.enabled = !isInPlayMode;

    if (!isInPlayMode)
      AttemptDestroyGoon();
  }


  void OnHeroReturned()
  {
    m_HeroHasReturned = true;

    AttemptDestroyGoon();
    CreateGoon();
  }


  void CreateGoon()
  {
    m_Goon = Instantiate(m_GoonPrefab, m_Transform.position, Quaternion.identity);

    // set direction stuff here
    var tileDirection = m_Goon.GetComponent<TileDirection>();
    if (tileDirection != null)
      tileDirection.Initialize(m_TileDirection.m_Direction);

    if (m_HeroHasReturned)
    {
      var platformerMover = m_Goon.GetComponent<PlatformerMover>();
      platformerMover.OnModeStarted(true);
    }
  }


  void AttemptDestroyGoon()
  {
    if (m_Goon != null)
      Destroy(m_Goon);
  }


  private void OnDestroy()
  {
    GlobalData.PlayModePreToggle -= OnPlayModePreToggle;
    GlobalData.PlayModeToggled -= OnPlayModeToggled;
    GlobalData.HeroReturned -= OnHeroReturned;
  }
}
