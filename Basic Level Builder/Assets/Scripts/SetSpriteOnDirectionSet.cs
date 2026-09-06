using System;
using UnityEngine;
using NewestFileData;

[Serializable]
public class RotationSprites
{
  public Sprite m_RightSprite;
  public Sprite m_UpSprite;
  public Sprite m_LeftSprite;
  public Sprite m_DownSprite;
}

[RequireComponent(typeof(TileDirection))]
public class SetSpriteOnDirectionSet : MonoBehaviour
{
  public SpriteRenderer m_SpriteRenderer;
  [SerializeField]
  private RotationSprites m_SpriteRotations;
  

  TileDirection m_TileDirection;

  private void Awake()
  {
    m_TileDirection = GetComponent<TileDirection>();

    m_TileDirection.DirectionSet += OnDirectionSet;
  }


  void OnDirectionSet(Direction direction)
  {
    m_SpriteRenderer.sprite = GetSpriteFromDirection(direction, m_SpriteRotations);
  }

  public static Sprite GetSpriteFromDirection(Direction direction, RotationSprites rotSprites)
  {
    Sprite[] sprites = { rotSprites.m_RightSprite, rotSprites.m_DownSprite, rotSprites.m_LeftSprite, rotSprites.m_UpSprite };
    for (int i = 0; i < sprites.Length; i++)
    {
      int index = ((int)direction + i) % sprites.Length;

      if (sprites[index] != null)
      {
        return sprites[index];
      }
    }

    return null;
  }


  private void OnDestroy()
  {
    m_TileDirection.DirectionSet -= OnDirectionSet;
  }
}
