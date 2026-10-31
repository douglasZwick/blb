/***************************************************
Authors:        Brenden Epp
Last Updated:   10/30/2026

Copyright 2018-2026, DigiPen Institute of Technology
***************************************************/

using UnityEngine;

public class GridToggler: MonoBehaviour
{
  [SerializeField] private GameObject m_Grid;

  public void ToggleGrid()
  {
    m_Grid.SetActive(!m_Grid.activeSelf);
  }
}
