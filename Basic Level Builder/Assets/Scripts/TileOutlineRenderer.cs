/***************************************************
Authors:        Brenden Epp
Last Updated:   11/15/2026

Copyright 2018-2026, DigiPen Institute of Technology
***************************************************/

using UnityEngine;

public class TileOutlineRenderer : MonoBehaviour
{
  [SerializeField] private float m_GridPlaneZ;

  private Camera m_Camera;
  [SerializeField] private Camera m_EditModeCamera;
  [SerializeField] private Camera m_PlayModeCamera;
  [SerializeField] private TileGrid m_TileGrid;

  void Awake()
  {
    m_Camera = m_EditModeCamera;
  }

  void OnEnable()
  {
    GlobalData.PlayModePreToggle += OnPlayModeToggled;
    GlobalData.TileGridChanged += OnTileGridChanged;
  }

  void OnDisable()
  {
    GlobalData.PlayModePreToggle -= OnPlayModeToggled;
    GlobalData.TileGridChanged -= OnTileGridChanged;
  }

  void LateUpdate()
  {
    var cameraPosition = m_Camera.transform.position;
    transform.SetPositionAndRotation(
      new Vector3(cameraPosition.x, cameraPosition.y, m_GridPlaneZ),
      Quaternion.identity);

    var height = m_Camera.orthographicSize * 2;
    transform.localScale = new Vector3(height * m_Camera.aspect, height, 1);

    
  }

  void OnTileGridChanged()
  {
    // Pass tile grid to shader
    m_TileGrid.grid
  }

  void OnPlayModeToggled(bool isPlayMode)
  {
    if (isPlayMode)
      m_Camera = m_PlayModeCamera;
    else
      m_Camera = m_EditModeCamera;
  }
}
