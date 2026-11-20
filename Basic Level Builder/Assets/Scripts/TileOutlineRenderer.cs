/***************************************************
Authors:        Brenden Epp
Last Updated:   11/15/2026

Copyright 2018-2026, DigiPen Institute of Technology
***************************************************/

using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class TileOutlineRenderer : MonoBehaviour
{
  static readonly int s_TileGridTextureId = Shader.PropertyToID("_TileGridTex");
  static readonly int s_TileGridStartPositionId = Shader.PropertyToID("_TileGridStartPosition");

  [SerializeField] float m_GridPlaneZ;
  [SerializeField] Camera m_EditModeCamera;
  [SerializeField] Camera m_PlayModeCamera;
  [SerializeField] TileGrid m_TileGrid;
  [SerializeField] Shader m_TileOutlinerShader;

  SpriteRenderer m_SpriteRenderer;
  Material m_Material;
  Texture2D m_TileMapTexture;

  Camera m_Camera;
  [SerializeField] int m_BaseSortingOrder;

  void Awake()
  {
    if (m_TileGrid == null)
      m_TileGrid = GetComponentInParent<TileGrid>();

    if (m_TileGrid == null)
    {
      Debug.LogError("TileOutlineRenderer requires a TileGrid in its parent hierarchy.", this);
      enabled = false;
      return;
    }

    if (m_EditModeCamera == null || m_PlayModeCamera == null)
    {
      Debug.LogError("TileOutlineRenderer requires edit-mode and play-mode cameras.", this);
      enabled = false;
      return;
    }

    if (m_TileOutlinerShader == null)
      m_TileOutlinerShader = Shader.Find("BLB/TileOutliner");

    if (m_TileOutlinerShader == null)
    {
      Debug.LogError("Could not find the BLB/TileOutliner shader.", this);
      enabled = false;
      return;
    }

    m_SpriteRenderer = GetComponent<SpriteRenderer>();
    if (m_SpriteRenderer.sprite == null)
    {
      Debug.LogError("TileOutlineRenderer requires a SpriteRenderer with a sprite assigned.", this);
      enabled = false;
      return;
    }

    m_Material = new Material(m_TileOutlinerShader);
    m_SpriteRenderer.sharedMaterial = m_Material;

    m_Camera = m_EditModeCamera;
  }

  void OnEnable()
  {
    GlobalData.PlayModePreToggle += OnPlayModeToggled;
    GlobalData.TileGridChanged += OnTileGridChanged;
    OnTileGridChanged();
  }

  void OnDisable()
  {
    GlobalData.PlayModePreToggle -= OnPlayModeToggled;
    GlobalData.TileGridChanged -= OnTileGridChanged;
  }

  void OnDestroy()
  {
    Destroy(m_TileMapTexture);
    Destroy(m_Material);
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
    Destroy(m_TileMapTexture);
    m_TileMapTexture = m_TileGrid.GetTileMap();

    var minBounds = m_TileGrid.m_MinBounds;
    var maxBounds = m_TileGrid.m_MaxBounds;
    var startPosition = minBounds.x <= maxBounds.x && minBounds.y <= maxBounds.y
      ? new Vector4(minBounds.x, minBounds.y, 0, 0)
      : Vector4.zero;


    m_SpriteRenderer.enabled = m_TileMapTexture != null;

    var material = m_Material;
    material.SetTexture(s_TileGridTextureId, m_TileMapTexture);
    material.SetVector(s_TileGridStartPositionId, startPosition);
  }

  void OnPlayModeToggled(bool isPlayMode)
  {
    m_Camera = isPlayMode ? m_PlayModeCamera : m_EditModeCamera;
  }
}
