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
  * DESCRIPTION  : Create a trash dialouge prompt.
  * INPUTS       : None
  * OUTPUTS      : None
  **/
  public void CreateTrashConfirmation()
  {
    //Prevent mass spawning.
    if (m_pTrashUI)
      return;

    GameObject prefab = Resources.Load<GameObject>("Prefabs/ConfirmTrashUI");
    m_pTrashUI = (GameObject)Instantiate(prefab, new Vector3(Screen.width / 2, Screen.height / 2, 0), Quaternion.identity);
    m_pTrashUI.transform.SetParent(GameObject.FindGameObjectWithTag("EditorCanvas").transform);
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
