/***************************************************
File:           UITrashButton.cs
Authors:        Christopher Onorati
Last Updated:   6/17/2019
Last Version:   2019.1.4

Description:
  Logic for the trash button on the UI

Copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using UnityEngine;

public sealed class UITrashButton : MonoBehaviour
{
  /************************************************************************************/

  // The main grid component, found on Start elsewhere in the scene, that manages all the tiles
  TileGrid m_TileGrid;

  static GameObject m_pTrashUI;

  /**
  * FUNCTION NAME: Start
  * DESCRIPTION  : Cache the tile grid.
  * INPUTS       : None
  * OUTPUTS      : None
  **/
  void Start()
  {
    m_TileGrid = FindObjectOfType<TileGrid>();
  }

  /**
  * FUNCTION NAME: CreateTrashConfirmation
  * DESCRIPTION  : Create a trash dialogue prompt.
  * INPUTS       : None
  * OUTPUTS      : None
  **/
  public async void CreateTrashConfirmation()
  {
    //Prevent mass spawning.
    if (m_pTrashUI)
      return;

    string trashMessage = "Are you sure you want to clear the entire level?" +
      $"{System.Environment.NewLine}This action cannot be undone.";
    var result = await DialogManager.ShowGenericDialog(UiGenericModalDialog.ButtonOptions.ConfirmAndDeny, trashMessage);
    if (result == ModalDialog.DialogResult.Confirm)
    {
      TrashTheLevel();
    }
  }

  /**
  * FUNCTION NAME: TrashTheLevel
  * DESCRIPTION  : I'm the trash man.
  * INPUTS       : None
  * OUTPUTS      : None
  **/
  public void TrashTheLevel()
  {
    if (m_TileGrid == null)
    {
      Debug.LogError("UI Trash Button cannot find the tile grid!");
      return;
    }

    m_TileGrid.ClearGrid();
  }
}
