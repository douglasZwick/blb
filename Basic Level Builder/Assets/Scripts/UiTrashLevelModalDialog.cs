using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiTrashLevelModalDialog : ModalDialog
{
  TileGrid m_TileGrid;


  private void Awake()
  {
    m_TileGrid = FindObjectOfType<TileGrid>();
  }


  public override void Open()
  {
    var anchoredX = 0;
    var anchoredY = 0;
    var anchoredPosition = new Vector2(anchoredX, anchoredY);
    m_RectTransform.anchoredPosition = anchoredPosition;

    base.Open();

    // scaling effects
  }


  public override void Close()
  {
    base.Close();

    // scaling effects

    Destroy(gameObject);
  }


  public void Confirm()
  {
    TrashLevel();
    Close();
  }


  public void Cancel()
  {
    Close();
  }


  void TrashLevel()
  {
    m_TileGrid.ClearGrid();
  }
}
