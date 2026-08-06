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


  private void Awake()
  {
    GlobalData.PrimaryTileChanged += OnPrimaryTileChanged;
    GlobalData.SecondaryTileChanged += OnSecondaryTileChanged;
    GlobalData.TileRotated += OnTileRotated;
  }

  void OnTileRotated(Direction direction)
  {
    ApplyTileVisual(m_PrimaryTileType, m_PrimaryImage, direction);
    ApplyTileVisual(m_SecondaryTileType, m_SecondaryImage, direction);
  }

  private void ApplyTileVisual(TileType tileType, Image img, Direction direction)
  {
    foreach (TileSpriteRotations tileSpriteRotations in m_TileRoationSprites)
    {
      if (tileSpriteRotations.m_tileType == tileType)
      {
        img.sprite = SetSpriteOnDirectionSet.GetSpriteFromDirection(direction, tileSpriteRotations);
        img.transform.localRotation = Quaternion.identity;
        return;
      }
    }

    var icon = TilePicker.s_Icons[tileType];
    img.sprite = icon.sprite;
    img.color = icon.color;
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
    ApplyTileVisual(type, m_PrimaryImage, GlobalData.GetTileState().Direction);
  }


  void OnSecondaryTileChanged(TileType type)
  {
    m_SecondaryTileType = type;
    ApplyTileVisual(type, m_SecondaryImage, GlobalData.GetTileState().Direction);
  }


  private void OnDestroy()
  {
    GlobalData.PrimaryTileChanged -= OnPrimaryTileChanged;
    GlobalData.SecondaryTileChanged -= OnSecondaryTileChanged;
    GlobalData.TileRotated -= OnTileRotated;
  }
}
