/***************************************************
Authors:        Brenden Epp
Last Updated:   11/15/2026

Copyright 2018-2026, DigiPen Institute of Technology
***************************************************/

using UnityEngine;
using System.Collections.Generic;
using NewestFileData;

public class TileOutlineRenderer : MonoBehaviour
{
  static readonly int s_TileGridTextureId = Shader.PropertyToID("_TileGridTex");
  static readonly int s_TileGridStartPositionId = Shader.PropertyToID("_TileGridStartPosition");
  static readonly int s_TileWorldPositionOffsetId = Shader.PropertyToID("_TileWorldPositionOffset");

  [SerializeField] TileGrid m_TileGrid;
  [SerializeField] Shader m_TileOutlinerShader;

  Material m_Material;
  Texture2D m_TileMapTexture;
  readonly Dictionary<SpriteRenderer, Material> m_OriginalMaterials = new();
  readonly Dictionary<SpriteRenderer, MaterialPropertyBlock> m_OriginalPropertyBlocks = new();
  readonly Dictionary<SpriteRenderer, Vector2> m_GridPositions = new();
  MaterialPropertyBlock m_PropertyBlock;

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

    if (m_TileOutlinerShader == null)
      m_TileOutlinerShader = Shader.Find("BLB/TileOutliner");

    if (m_TileOutlinerShader == null)
    {
      Debug.LogError("Could not find the BLB/TileOutliner shader.", this);
      enabled = false;
      return;
    }

    m_Material = new Material(m_TileOutlinerShader);
    m_PropertyBlock = new MaterialPropertyBlock();
  }

  void OnEnable()
  {
    GlobalData.TileGridChanged += OnTileGridChanged;
    OnTileGridChanged();
  }

  void OnDisable()
  {
    GlobalData.TileGridChanged -= OnTileGridChanged;
    foreach (var entry in m_OriginalMaterials)
    {
      if (entry.Key != null && entry.Key.sharedMaterial == m_Material)
        entry.Key.sharedMaterial = entry.Value;
      if (entry.Key != null)
        entry.Key.SetPropertyBlock(m_OriginalPropertyBlocks[entry.Key]);
    }
    m_OriginalMaterials.Clear();
    m_OriginalPropertyBlocks.Clear();
    m_GridPositions.Clear();
  }

  void OnDestroy()
  {
    Destroy(m_TileMapTexture);
    Destroy(m_Material);
  }

  void LateUpdate()
  {
    foreach (var entry in m_GridPositions)
    {
      var spriteRenderer = entry.Key;
      if (spriteRenderer == null)
        continue;

      spriteRenderer.GetPropertyBlock(m_PropertyBlock);
      var currentPosition = spriteRenderer.transform.position;
      var gridPosition = entry.Value;
      m_PropertyBlock.SetVector(s_TileWorldPositionOffsetId, new Vector2(
        gridPosition.x - currentPosition.x,
        gridPosition.y - currentPosition.y));
      spriteRenderer.SetPropertyBlock(m_PropertyBlock);
    }
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

    m_Material.SetTexture(s_TileGridTextureId, m_TileMapTexture != null ? m_TileMapTexture : Texture2D.blackTexture);
    m_Material.SetVector(s_TileGridStartPositionId, startPosition);

    var activeRenderers = new HashSet<SpriteRenderer>();
    foreach (var element in m_TileGrid.GetElements())
    {
      if (!IsOutlinableTile(element.m_Type) || element.m_GameObject == null)
        continue;

      if (!element.m_GameObject.TryGetComponent<SpriteRenderer>(out var spriteRenderer))
        continue;

      activeRenderers.Add(spriteRenderer);
      if (!m_OriginalMaterials.ContainsKey(spriteRenderer))
      {
        m_OriginalMaterials.Add(spriteRenderer, spriteRenderer.sharedMaterial);
        var originalPropertyBlock = new MaterialPropertyBlock();
        spriteRenderer.GetPropertyBlock(originalPropertyBlock);
        m_OriginalPropertyBlocks.Add(spriteRenderer, originalPropertyBlock);
      }
      m_GridPositions[spriteRenderer] = element.m_GridIndex;
      spriteRenderer.sharedMaterial = m_Material;
    }

    var removedRenderers = new List<SpriteRenderer>();
    foreach (var entry in m_OriginalMaterials)
    {
      if (activeRenderers.Contains(entry.Key))
        continue;

      if (entry.Key != null && entry.Key.sharedMaterial == m_Material)
        entry.Key.sharedMaterial = entry.Value;
      if (entry.Key != null)
        entry.Key.SetPropertyBlock(m_OriginalPropertyBlocks[entry.Key]);
      removedRenderers.Add(entry.Key);
    }
    foreach (var renderer in removedRenderers)
    {
      m_OriginalMaterials.Remove(renderer);
      m_OriginalPropertyBlocks.Remove(renderer);
      m_GridPositions.Remove(renderer);
    }
  }

  static bool IsOutlinableTile(TileType tileType)
  {
    return tileType == TileType.SOLID ||
      tileType == TileType.SLOPE ||
      tileType == TileType.FALSE_SOLID ||
      tileType == TileType.BG ||
      tileType == TileType.BG_SLOPE;
  }
}
