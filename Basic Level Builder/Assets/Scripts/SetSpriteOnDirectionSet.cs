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
    Sprite sprite;

    switch (direction)
    {
      default:  // Direction.RIGHT
        sprite = m_RightSprite;
        break;

      case Direction.UP:
        sprite = m_UpSprite;
        break;

      case Direction.LEFT:
        sprite = m_LeftSprite;
        break;

      case Direction.DOWN:
        sprite = m_DownSprite;
        break;
    }

    m_SpriteRenderer.sprite = sprite;
  }


  private void OnDestroy()
  {
    m_TileDirection.DirectionSet -= OnDirectionSet;
  }
}
