/***************************************************
Authors:        Douglas Zwick, Brenden Epp
Last Updated:   7/20/2026

Copyright 2018-2026, DigiPen Institute of Technology
***************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class TileSpriteRotations : RotationSprites
{
  public TileType m_tileType;
}

public class TileIconUpdater : MonoBehaviour
{
  [SerializeField]
  private Image m_PrimaryImage, m_SecondaryImage;
  private TileType m_PrimaryTileType, m_SecondaryTileType;
  [SerializeField]
  private List<TileSpriteRotations> m_TileRoationSprites;
  private TilesPalette m_TilesPalette;

  private void Awake()
  {
    GlobalData.PrimaryTileChanged += OnPrimaryTileChanged;
    GlobalData.SecondaryTileChanged += OnSecondaryTileChanged;
    GlobalData.TileRotated += OnTileRotated;
    m_TilesPalette = FindObjectOfType<TilesPalette>();
  }

  void OnTileRotated()
  {
    ApplyTileVisual(m_PrimaryTileType, m_PrimaryImage, GetTileDirection(m_PrimaryTileType));
    ApplyTileVisual(m_SecondaryTileType, m_SecondaryImage, GetTileDirection(m_SecondaryTileType));
  }

  private void ApplyTileVisual(TileType tileType, Image img, Direction direction)
  {
    var icon = TilePicker.s_Icons[tileType];
    img.color = icon.color;

    foreach (TileSpriteRotations tileSpriteRotations in m_TileRoationSprites)
    {
      if (tileSpriteRotations.m_tileType == tileType)
      {
        img.sprite = SetSpriteOnDirectionSet.GetSpriteFromDirection(direction, tileSpriteRotations);
        img.transform.localRotation = Quaternion.identity;
        return;
      }
    }

    img.sprite = icon.sprite;
    RotateTransform(direction, img);
  }

  private void RotateTransform(Direction direction, Image img)
  {
    int rotation = direction switch
    {
      Direction.LEFT => 180,
      Direction.UP => 90,
      Direction.DOWN => -90,
      //Direction.RIGHT => 0,
      _ => 0,
    };

    img.transform.localRotation = Quaternion.Euler(0, 0, rotation);
  }


  void OnPrimaryTileChanged(TileType type)
  {
    m_PrimaryTileType = type;
    ApplyTileVisual(type, m_PrimaryImage, GetTileDirection(type));
  }


  void OnSecondaryTileChanged(TileType type)
  {
    m_SecondaryTileType = type;
    ApplyTileVisual(type, m_SecondaryImage, GetTileDirection(type));
  }

  private Direction GetTileDirection(TileType type)
  {
    GameObject tilePrefab = m_TilesPalette.GetPrefabFromType(type);
    if (tilePrefab != null && tilePrefab.TryGetComponent(out TileDirection tileDirection)){
      return GlobalData.GetTileState(tileDirection.m_DirectionType).Direction;
    }

    // If the tile dosn't have a tileDirection then don't rotate the preview
    return Direction.RIGHT;
  }


  private void OnDestroy()
  {
    GlobalData.PrimaryTileChanged -= OnPrimaryTileChanged;
    GlobalData.SecondaryTileChanged -= OnSecondaryTileChanged;
    GlobalData.TileRotated -= OnTileRotated;
  }
}
