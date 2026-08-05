using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TileDirection))]
public class SetSpriteOnDirectionSet : MonoBehaviour
{
  public SpriteRenderer m_SpriteRenderer;
  public Sprite m_RightSprite;
  public Sprite m_UpSprite;
  public Sprite m_LeftSprite;
  public Sprite m_DownSprite;

  TileDirection m_TileDirection;


  private void Awake()
  {
    m_TileDirection = GetComponent<TileDirection>();

    m_TileDirection.DirectionSet += OnDirectionSet;
  }


  void OnDirectionSet(Direction direction)
  {
    m_SpriteRenderer.sprite = direction switch
    {
      Direction.UP => m_UpSprite,
      Direction.LEFT => m_LeftSprite,
      Direction.DOWN => m_DownSprite,
      // Direction.RIGHT
      _ => m_RightSprite,
    };
  }


  private void OnDestroy()
  {
    m_TileDirection.DirectionSet -= OnDirectionSet;
  }
}
