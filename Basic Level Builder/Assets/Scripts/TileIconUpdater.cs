/***************************************************
Authors:        Douglas Zwick, Brenden Epp
Last Updated:   7/20/2026

Copyright 2018-2026, DigiPen Institute of Technology
***************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NewestFileData;

[Serializable]
public class TileSpriteRotations : RotationSprites
{
  public TileType m_tileType;
}

public class TileIconUpdater : MonoBehaviour
{
  [SerializeField]
  private Image m_PrimaryImage, m_SecondaryImage;
  private TileType m_PrimaryTileType = TileType.SOLID, m_SecondaryTileType;
  [SerializeField]
  private List<TileSpriteRotations> m_TileRoationSprites;
  [SerializeField]
  private GameObject m_RotateArrowLeft, m_RotateArrowRight;
  private TilesPalette m_TilesPalette;
  private Quaternion m_PrimaryBaseRotation;
  private ActionSequence m_JiggleSequence;

  private void Awake()
  {
    GlobalData.PrimaryTileChanged += OnPrimaryTileChanged;
    GlobalData.SecondaryTileChanged += OnSecondaryTileChanged;
    GlobalData.TileRotated += OnTileRotated;
    m_TilesPalette = FindObjectOfType<TilesPalette>();
    m_PrimaryBaseRotation = m_PrimaryImage.transform.localRotation;
  }

  private void Start()
  {
    ToggleRotateArrows();
  }

  public void JigglePrimaryTile(int direction)
  {
    direction = Mathf.Clamp(direction, -1, 1);
    if (direction == 0)
      return;

    CancelPrimaryTileJiggle();

    float baseRotation = m_PrimaryBaseRotation.eulerAngles.z;
    float offsetRotation = direction * 10.0f;
    m_JiggleSequence = ActionMaster.Actions.Sequence();
    Ease easeIn = new(Ease.Sin.In);
    Ease easeOut = new(Ease.Sin.Out);
    m_JiggleSequence.Rotate2D(m_PrimaryImage.gameObject, baseRotation + offsetRotation, 0.15f, easeIn);
    m_JiggleSequence.Rotate2D(m_PrimaryImage.gameObject, baseRotation, 0.15f, easeOut);
    m_JiggleSequence.Rotate2D(m_PrimaryImage.gameObject, baseRotation + offsetRotation, 0.15f, easeIn);
    m_JiggleSequence.Rotate2D(m_PrimaryImage.gameObject, baseRotation, 0.15f, easeOut);
  }

  public void CancelPrimaryTileJiggle()
  {
    m_JiggleSequence?.Cancel();
    m_JiggleSequence = null;
    m_PrimaryImage.transform.localRotation = m_PrimaryBaseRotation;
  }

  void OnTileRotated()
  {
    ApplyTileVisual(m_PrimaryTileType, m_PrimaryImage, GetTileDirection(m_PrimaryTileType), true);
    ApplyTileVisual(m_SecondaryTileType, m_SecondaryImage, GetTileDirection(m_SecondaryTileType), true);
  }

  public void RotateTileLeft()
  {
    GlobalData.RotateSelectedTileCounterClockwise();
  }

  public void RotateTileRight()
  {
    GlobalData.RotateSelectedTileClockwise();
  }

  private void ApplyTileVisual(TileType tileType, Image img, Direction direction, bool animateRotation = false)
  {
    var icon = TilePicker.s_Icons[tileType];
    img.color = icon.color;
    Quaternion targetRotation = Quaternion.identity;
    bool hasRotatedSprite = false;

    foreach (TileSpriteRotations tileSpriteRotations in m_TileRoationSprites)
    {
      if (tileSpriteRotations.m_tileType == tileType)
      {
        img.sprite = SetSpriteOnDirectionSet.GetSpriteFromDirection(direction, tileSpriteRotations);
        hasRotatedSprite = true;
        break;
      }
    }

    if (!hasRotatedSprite)
    {
      img.sprite = icon.sprite;

      int rotation = direction switch
      {
        Direction.LEFT => 180,
        Direction.UP => 90,
        Direction.DOWN => -90,
        _ => 0,
      };
      targetRotation = Quaternion.Euler(0, 0, rotation);
    }

    if (animateRotation)
    {
      ActionMaster.Actions.CancelAllInTreeRegarding(img.gameObject);
      ActionMaster.Actions.Rotate2D(img.gameObject, targetRotation.eulerAngles.z, 0.2f, new Ease(Ease.Back.Out));
      if (img == m_PrimaryImage)
        m_PrimaryBaseRotation = targetRotation;
    }
    else
    {
      img.transform.localRotation = targetRotation;
      SetPrimaryBaseRotation(img);
    }
  }

  private void SetPrimaryBaseRotation(Image img)
  {
    if (img == m_PrimaryImage)
      m_PrimaryBaseRotation = img.transform.localRotation;
  }

  void OnPrimaryTileChanged(TileType type)
  {
    m_PrimaryTileType = type;
    ApplyTileVisual(type, m_PrimaryImage, GetTileDirection(type));
    ToggleRotateArrows();
  }

  void ToggleRotateArrows()
  {
    // Remove rotate arrows on tiles without a rotator
    GameObject tilePrefab = m_TilesPalette.GetPrefabFromType(m_PrimaryTileType);
    if (tilePrefab != null && !tilePrefab.TryGetComponent(out TileDirection _))
    {
      m_RotateArrowLeft.SetActive(false);
      m_RotateArrowRight.SetActive(false);
    }
    else
    {
      m_RotateArrowLeft.SetActive(true);
      m_RotateArrowRight.SetActive(true);
    }
  }


  void OnSecondaryTileChanged(TileType type)
  {
    m_SecondaryTileType = type;
    ApplyTileVisual(type, m_SecondaryImage, GetTileDirection(type));
  }

  private Direction GetTileDirection(TileType type)
  {
    GameObject tilePrefab = m_TilesPalette.GetPrefabFromType(type);
    if (tilePrefab != null && tilePrefab.TryGetComponent(out TileDirection tileDirection))
    {
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
