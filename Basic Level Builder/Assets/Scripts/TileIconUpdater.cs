/***************************************************
Authors:        Douglas Zwick, Brenden Epp
Last Updated:   7/20/2026

Copyright 2018-2026, DigiPen Institute of Technology
***************************************************/

using UnityEngine;
using UnityEngine.UI;

public class TileIconUpdater : MonoBehaviour
{
  public Image m_PrimaryImage;
  public Image m_SecondaryImage;


  private void Awake()
  {
    GlobalData.PrimaryTileChanged += OnPrimaryTileChanged;
    GlobalData.SecondaryTileChanged += OnSecondaryTileChanged;
    GlobalData.TileRotated += OnTileRotated;
  }

  void OnTileRotated(Direction direction)
  {
    int rotation = 0;
    switch (direction)
    {
      case Direction.RIGHT:
        rotation = 0;
        break;
      case Direction.LEFT:
        rotation = 180;
        break;
      case Direction.UP:
        rotation = -90;
        break;
      case Direction.DOWN:
        rotation = 90;
        break;
    }
    
    m_PrimaryImage.transform.localRotation = Quaternion.Euler(0,0,rotation);
    m_SecondaryImage.transform.localRotation = Quaternion.Euler(0,0,rotation);
  }


  void OnPrimaryTileChanged(TileType type)
  {
    var icon = TilePicker.s_Icons[type];
    m_PrimaryImage.sprite = icon.sprite;
    m_PrimaryImage.color = icon.color;
  }


  void OnSecondaryTileChanged(TileType type)
  {
    var icon = TilePicker.s_Icons[type];
    m_SecondaryImage.sprite = icon.sprite;
    m_SecondaryImage.color = icon.color;
  }


  private void OnDestroy()
  {
    GlobalData.PrimaryTileChanged -= OnPrimaryTileChanged;
    GlobalData.SecondaryTileChanged -= OnSecondaryTileChanged;
  }
}
