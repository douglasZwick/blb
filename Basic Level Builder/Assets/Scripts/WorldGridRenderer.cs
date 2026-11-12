/***************************************************
Authors:        Brenden Epp
Last Updated:   11/11/2026

Copyright 2018-2026, DigiPen Institute of Technology
***************************************************/

using UnityEngine;

public class WorldGridRenderer : MonoBehaviour
{
  [SerializeField] private float m_GridPlaneZ;

  private Camera m_Camera;

  void Awake()
  {
    if (m_Camera == null)
    {
      m_Camera = Camera.main;
    }
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
}
