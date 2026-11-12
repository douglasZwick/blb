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

  private bool m_GridWasActive = false;

  private void OnEnable()
  {
    GlobalData.PlayModePreToggle += OnPlayModePreToggle;
  }

  private void OnDisable()
  {
    GlobalData.PlayModePreToggle -= OnPlayModePreToggle;
  }

  private void OnPlayModePreToggle(bool enteringPlay)
  {
    if (enteringPlay)
      SetGridActive(false);
    else
      SetGridActive(m_GridWasActive);
  }

  private void SetGridActive(bool value)
  {
    m_Grid.SetActive(value);
    var color = m_GridToggleImage.color;
    color.a = value ? 0.75f : 0.25f;
    m_GridToggleImage.color = color;
  }

  public void ToggleGrid()
  {
    SetGridActive(!m_Grid.activeSelf);
    m_GridWasActive = m_Grid.activeSelf;
  }
}
