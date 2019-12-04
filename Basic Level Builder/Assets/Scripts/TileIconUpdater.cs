using System.Collections;
using System.Collections.Generic;
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
