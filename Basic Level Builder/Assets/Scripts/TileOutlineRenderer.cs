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
  const int c_DistanceFieldPadding = 1;
  const int c_DistanceFieldSamplesPerTile = 15;

  static readonly int s_TileDistanceTextureId = Shader.PropertyToID("_TileDistanceTex");
  static readonly int s_TileGridStartPositionId = Shader.PropertyToID("_TileGridStartPosition");
  static readonly int s_TileDistanceScaleId = Shader.PropertyToID("_TileDistanceScale");

  [SerializeField] float m_GridPlaneZ;
  [SerializeField] Camera m_EditModeCamera;
  [SerializeField] Camera m_PlayModeCamera;
  [SerializeField] TileGrid m_TileGrid;
  [SerializeField] Shader m_TileOutlinerShader;

  readonly List<SpriteRenderer> m_SpriteRenderers = new();
  readonly List<Material> m_Materials = new();
  List<Texture2D> m_DistanceFieldTextures = new();

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
    foreach (var texture in m_DistanceFieldTextures)
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
    var distanceFields = new List<TileDistanceField>();
    var gridStartPosition = new Vector2(m_TileGrid.m_MinBounds.x, m_TileGrid.m_MinBounds.y);
    AddDistanceFields(solidTileMap, false, gridStartPosition, distanceFields);
    AddDistanceFields(bgTileMap, true, gridStartPosition, distanceFields);

    EnsureSpriteRendererCount(distanceFields.Count);
    var backgroundCount = 0;
    foreach (var distanceField in distanceFields)
    {
      if (distanceField.IsBackground)
        ++backgroundCount;
    }

    var nextBackgroundOrder = m_BaseSortingOrder - backgroundCount;
    var nextSolidOrder = m_BaseSortingOrder;
    for (var i = 0; i < distanceFields.Count; ++i)
    {
      var distanceField = distanceFields[i];
      var renderer = m_SpriteRenderers[i];
      renderer.enabled = true;
      renderer.sortingOrder = distanceField.IsBackground ? nextBackgroundOrder++ : nextSolidOrder++;

      var material = m_Materials[i];
      material.SetTexture(s_TileDistanceTextureId, distanceField.Texture);
      material.SetVector(s_TileGridStartPositionId, distanceField.StartPosition);
      material.SetFloat(s_TileDistanceScaleId, c_DistanceFieldSamplesPerTile);
    }

    for (var i = distanceFields.Count; i < m_SpriteRenderers.Count; ++i)
      m_SpriteRenderers[i].enabled = false;

    foreach (var texture in m_DistanceFieldTextures)
      Destroy(texture);

    m_DistanceFieldTextures.Clear();
    foreach (var distanceField in distanceFields)
      m_DistanceFieldTextures.Add(distanceField.Texture);

    Destroy(solidTileMap);
    Destroy(bgTileMap);
  }

  void AddDistanceFields(Texture2D tileMap, bool isBackground, Vector2 gridStartPosition,
    List<TileDistanceField> distanceFields)
  {
    if (tileMap == null)
      return;

    var pixels = tileMap.GetPixels();
    var pathCount = 0;
    foreach (var pixel in pixels)
    {
      if (pixel.r > 0.01f)
        pathCount = Mathf.Max(pathCount, Mathf.RoundToInt(pixel.g) + 1);
    }

    var pathOccupancy = new bool[pixels.Length];
    for (var pathIndex = 0; pathIndex < pathCount; ++pathIndex)
    {
      var pathMinX = tileMap.width;
      var pathMinY = tileMap.height;
      var pathMaxX = -1;
      var pathMaxY = -1;
      for (var i = 0; i < pixels.Length; ++i)
      {
        pathOccupancy[i] = pixels[i].r > 0.01f && Mathf.RoundToInt(pixels[i].g) == pathIndex;
        if (!pathOccupancy[i])
          continue;

        var x = i % tileMap.width;
        var y = i / tileMap.width;
        pathMinX = Mathf.Min(pathMinX, x);
        pathMinY = Mathf.Min(pathMinY, y);
        pathMaxX = Mathf.Max(pathMaxX, x);
        pathMaxY = Mathf.Max(pathMaxY, y);
      }

      if (pathMaxX < 0)
        continue;

      var pathWidth = pathMaxX - pathMinX + 1;
      var pathHeight = pathMaxY - pathMinY + 1;
      var croppedOccupancy = new bool[pathWidth * pathHeight];
      for (var y = 0; y < pathHeight; ++y)
      {
        for (var x = 0; x < pathWidth; ++x)
        {
          croppedOccupancy[y * pathWidth + x] =
            pathOccupancy[(y + pathMinY) * tileMap.width + x + pathMinX];
        }
      }

      var boundary = GetBoundarySegments(croppedOccupancy, pathWidth, pathHeight);
      if (boundary.Count == 0)
        continue;

      var width = (pathWidth + c_DistanceFieldPadding * 2) * c_DistanceFieldSamplesPerTile;
      var height = (pathHeight + c_DistanceFieldPadding * 2) * c_DistanceFieldSamplesPerTile;
      var distances = new float[width * height];

      for (var y = 0; y < height; ++y)
      {
        var pointY = (float)y / c_DistanceFieldSamplesPerTile - c_DistanceFieldPadding;
        for (var x = 0; x < width; ++x)
        {
          var pointX = (float)x / c_DistanceFieldSamplesPerTile - c_DistanceFieldPadding;
          var point = new Vector2(pointX, pointY);
          var minimumDistanceSquared = float.MaxValue;

          foreach (var segment in boundary)
            minimumDistanceSquared = Mathf.Min(minimumDistanceSquared, DistanceSquaredToSegment(point, segment));

          var cellX = Mathf.FloorToInt(pointX);
          var cellY = Mathf.FloorToInt(pointY);
          var isInside = cellX >= 0 && cellX < pathWidth
            && cellY >= 0 && cellY < pathHeight
            && croppedOccupancy[cellY * pathWidth + cellX];
          var distance = Mathf.Sqrt(minimumDistanceSquared);
          distances[y * width + x] = isInside ? -distance : distance;
        }
      }

      var distanceTexture = new Texture2D(width, height, TextureFormat.RFloat, false, true)
      {
        filterMode = FilterMode.Bilinear,
        wrapMode = TextureWrapMode.Clamp,
      };
      distanceTexture.SetPixelData(distances, 0);
      distanceTexture.Apply(false, false);

      var padding = Vector2.one * c_DistanceFieldPadding;
      var pathOffset = new Vector2(pathMinX, pathMinY);
      distanceFields.Add(new TileDistanceField(
        distanceTexture,
        new Vector4(
          gridStartPosition.x + pathOffset.x - padding.x,
          gridStartPosition.y + pathOffset.y - padding.y,
          0,
          0),
        isBackground));
    }
  }

  void EnsureSpriteRendererCount(int requiredCount)
  {
    while (m_SpriteRenderers.Count < requiredCount)
    {
      var sourceRenderer = m_SpriteRenderers[0];
      var child = new GameObject($"Tile Distance Field {m_SpriteRenderers.Count}");
      child.transform.SetParent(transform, false);

      var spriteRenderer = child.AddComponent<SpriteRenderer>();
      spriteRenderer.sprite = sourceRenderer.sprite;
      spriteRenderer.color = sourceRenderer.color;
      spriteRenderer.sortingLayerID = sourceRenderer.sortingLayerID;
      spriteRenderer.maskInteraction = sourceRenderer.maskInteraction;
      m_SpriteRenderers.Add(spriteRenderer);
      AddMaterial(spriteRenderer);
    }
  }

  static List<BoundarySegment> GetBoundarySegments(bool[] occupancy, int width, int height)
  {
    var segments = new List<BoundarySegment>();

    for (var y = 0; y <= height; ++y)
    {
      var runStart = -1;
      for (var x = 0; x <= width; ++x)
      {
        var hasBoundary = x < width && IsOccupied(occupancy, width, height, x, y - 1)
          != IsOccupied(occupancy, width, height, x, y);
        if (hasBoundary && runStart < 0)
          runStart = x;
        else if (!hasBoundary && runStart >= 0)
        {
          segments.Add(new BoundarySegment(new Vector2(runStart, y), new Vector2(x, y)));
          runStart = -1;
        }
      }
    }

    for (var x = 0; x <= width; ++x)
    {
      var runStart = -1;
      for (var y = 0; y <= height; ++y)
      {
        var hasBoundary = y < height && IsOccupied(occupancy, width, height, x - 1, y)
          != IsOccupied(occupancy, width, height, x, y);
        if (hasBoundary && runStart < 0)
          runStart = y;
        else if (!hasBoundary && runStart >= 0)
        {
          segments.Add(new BoundarySegment(new Vector2(x, runStart), new Vector2(x, y)));
          runStart = -1;
        }
      }
    }

    return segments;
  }

  static bool IsOccupied(bool[] occupancy, int width, int height, int x, int y)
  {
    return x >= 0 && x < width && y >= 0 && y < height && occupancy[y * width + x];
  }

  static float DistanceSquaredToSegment(Vector2 point, BoundarySegment segment)
  {
    var direction = segment.End - segment.Start;
    var t = Mathf.Clamp01(Vector2.Dot(point - segment.Start, direction) / direction.sqrMagnitude);
    return (point - (segment.Start + direction * t)).sqrMagnitude;
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

  readonly struct TileDistanceField
  {
    public readonly Texture2D Texture;
    public readonly Vector4 StartPosition;
    public readonly bool IsBackground;

    public TileDistanceField(Texture2D texture, Vector4 startPosition, bool isBackground)
    {
      Texture = texture;
      StartPosition = startPosition;
      IsBackground = isBackground;
    }
  }

  readonly struct BoundarySegment
  {
    public readonly Vector2 Start;
    public readonly Vector2 End;

    public BoundarySegment(Vector2 start, Vector2 end)
    {
      Start = start;
      End = end;
    }
  }
}
