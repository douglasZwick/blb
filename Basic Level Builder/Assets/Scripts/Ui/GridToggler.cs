/***************************************************
Authors:        Brenden Epp
Last Updated:   10/30/2026

Copyright 2018-2026, DigiPen Institute of Technology
***************************************************/

using UnityEngine;
using UnityEngine.UI;

public class GridToggler: MonoBehaviour
{
  [SerializeField] private GameObject m_Grid;
  [SerializeField] private Image m_GridToggleImage;

  public void ToggleGrid()
  {
    m_Grid.SetActive(!m_Grid.activeSelf);
    var color = m_GridToggleImage.color;
    color.a = m_Grid.activeSelf ? 0.75f : 0.25f;
    m_GridToggleImage.color = color;
  }
}
