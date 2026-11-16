/***************************************************
Authors:        Brenden Epp
Last Updated:   11/15/2026

Copyright 2018-2026, DigiPen Institute of Technology
***************************************************/

using System.Collections.Generic;
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

  readonly List<SpriteRenderer> m_SpriteRenderers = new();
  readonly List<Material> m_Materials = new();
  List<Texture2D> m_TileMapTextures = new();

  Camera m_Camera;
  int m_BaseSortingOrder;

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

    var spriteRenderer = GetComponent<SpriteRenderer>();
    if (spriteRenderer.sprite == null)
    {
      Debug.LogError("TileOutlineRenderer requires a SpriteRenderer with a sprite assigned.", this);
      enabled = false;
      return;
    }

    m_SpriteRenderers.Add(spriteRenderer);
    m_BaseSortingOrder = spriteRenderer.sortingOrder;
    AddMaterial(spriteRenderer);

    var backgroundObject = new GameObject("Background Tile Map");
    backgroundObject.transform.SetParent(transform, false);
    var backgroundRenderer = backgroundObject.AddComponent<SpriteRenderer>();
    backgroundRenderer.sprite = spriteRenderer.sprite;
    backgroundRenderer.color = spriteRenderer.color;
    backgroundRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
    backgroundRenderer.sortingOrder = m_BaseSortingOrder - 1;
    backgroundRenderer.maskInteraction = spriteRenderer.maskInteraction;
    m_SpriteRenderers.Add(backgroundRenderer);
    AddMaterial(backgroundRenderer);

    m_Materials[0].SetFloat("_OutlineAlphaCap", 1.0f);
    m_Materials[1].SetFloat("_OutlineAlphaCap", 0.1f);

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
    foreach (var texture in m_TileMapTextures)
      Destroy(texture);

    foreach (var material in m_Materials)
      Destroy(material);
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
    var solidTileMap = m_TileGrid.GetSolidTileMap();
    var bgTileMap = m_TileGrid.GetBgTileMap();
    var tileMapTextures = new List<Texture2D> { solidTileMap, bgTileMap };

    var minBounds = m_TileGrid.m_MinBounds;
    var maxBounds = m_TileGrid.m_MaxBounds;
    var startPosition = minBounds.x <= maxBounds.x && minBounds.y <= maxBounds.y
      ? new Vector4(minBounds.x, minBounds.y, 0, 0)
      : Vector4.zero;

    for (var i = 0; i < m_SpriteRenderers.Count; ++i)
    {
      var renderer = m_SpriteRenderers[i];
      renderer.enabled = tileMapTextures[i] != null;
      renderer.sortingOrder = i == 0 ? m_BaseSortingOrder : m_BaseSortingOrder - 1;

      var material = m_Materials[i];
      material.SetTexture(s_TileGridTextureId, tileMapTextures[i]);
      material.SetVector(s_TileGridStartPositionId, startPosition);
    }

    foreach (var texture in m_TileMapTextures)
      Destroy(texture);

    m_TileMapTextures = tileMapTextures;
  }

  void AddMaterial(SpriteRenderer spriteRenderer)
  {
    var material = new Material(m_TileOutlinerShader);
    spriteRenderer.sharedMaterial = material;
    m_Materials.Add(material);
  }

  void OnPlayModeToggled(bool isPlayMode)
  {
    m_Camera = isPlayMode ? m_PlayModeCamera : m_EditModeCamera;
  }
}
