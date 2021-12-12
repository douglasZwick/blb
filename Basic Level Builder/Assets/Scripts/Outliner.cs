using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Outliner : MonoBehaviour
{
  static Vector3 s_One = new Vector3(1, 1, 0);
  static Vector3 s_Half = new Vector3(0.5f, 0.5f, 0);
  static float s_BoundsChangeDuration = 0.06f;

  public SpriteRenderer m_BackgroundRenderer;
  public SpriteMask m_SpriteMask;
  public SpriteRenderer m_InnerOutlineRenderer;
  public SpriteRenderer m_OuterOutlineRenderer;
  public Camera m_Camera;


  Transform m_Root;
  Transform m_BackgroundTransform;
  Transform m_MaskTransform;
  Transform m_InnerOutlineTransform;
  Transform m_OuterOutlineTransform;
  [HideInInspector]
  public Vector3 m_DisplayMinBounds = Vector3.zero;
  [HideInInspector]
  public Vector3 m_DisplayMaxBounds = Vector3.zero;
  Vector3 m_TargetMinBounds = Vector3.zero;
  Vector3 m_TargetMaxBounds = Vector3.zero;
  ActionGroup m_BoundsChangeGroup;
  

  private void Awake()
  {
    m_Root = transform;

    if (m_BackgroundRenderer != null)
      m_BackgroundTransform = m_BackgroundRenderer.transform;
    if (m_SpriteMask != null)
      m_MaskTransform = m_SpriteMask.transform;
    if (m_InnerOutlineRenderer != null)
      m_InnerOutlineTransform = m_InnerOutlineRenderer.transform;
    if (m_OuterOutlineRenderer != null)
      m_OuterOutlineTransform = m_OuterOutlineRenderer.transform;

    m_BoundsChangeGroup = ActionMaster.Actions.Group();
  }


  // just to appease the editor
  private void Start() { }


  public void OutlineWithBounds(Vector3 minBounds, Vector3 maxBounds)
  {
    enabled = true;

    if (minBounds != m_TargetMinBounds || maxBounds != m_TargetMaxBounds)
    {
      m_BoundsChangeGroup.Cancel();
      m_BoundsChangeGroup = ActionMaster.Actions.Group();
      m_BoundsChangeGroup.OutlinerMin(gameObject, minBounds, s_BoundsChangeDuration, new Ease(Ease.Quad.InOut));
      m_BoundsChangeGroup.OutlinerMax(gameObject, maxBounds, s_BoundsChangeDuration, new Ease(Ease.Quad.InOut));
    }

    m_TargetMinBounds = minBounds;
    m_TargetMaxBounds = maxBounds;
  }


  public void OutlineSinglePosition(Vector3 position)
  {
    enabled = true;

    m_DisplayMinBounds = m_DisplayMaxBounds = position;

    Outline();
  }


  public void Outline()
  {
    if (!enabled)
      return;

    var xUninitialized = m_DisplayMinBounds.x > m_DisplayMaxBounds.x;

    if (xUninitialized)
    {
      Disable();
    }
    else
    {
      Enable();

      var center = (m_DisplayMinBounds + m_DisplayMaxBounds) / 2;
      var difference = m_DisplayMaxBounds - m_DisplayMinBounds;

      m_Root.position = center;
      var maskScale = difference + s_One;

      var screenSpaceMinBounds = m_Camera.WorldToScreenPoint(m_DisplayMinBounds - s_Half);
      var screenSpaceMaxBounds = m_Camera.WorldToScreenPoint(m_DisplayMaxBounds + s_Half);

      var innerOutlineScreenSpaceLowerLeft = screenSpaceMinBounds - s_One;
      var innerOutlineScreenSpaceUpperRight = screenSpaceMaxBounds + s_One;
      var innerOutlineWorldSpaceLowerLeft = m_Camera.ScreenToWorldPoint(innerOutlineScreenSpaceLowerLeft);
      var innerOutlineWorldSpaceUpperRight = m_Camera.ScreenToWorldPoint(innerOutlineScreenSpaceUpperRight);
      var innerOutlineScale = innerOutlineWorldSpaceUpperRight - innerOutlineWorldSpaceLowerLeft;

      var outerOutlineScreenSpaceLowerLeft = innerOutlineScreenSpaceLowerLeft - s_One;
      var outerOutlineScreenSpaceUpperRight = innerOutlineScreenSpaceUpperRight + s_One;
      var outerOutlineWorldSpaceLowerLeft = m_Camera.ScreenToWorldPoint(outerOutlineScreenSpaceLowerLeft);
      var outerOutlineWorldSpaceUpperRight = m_Camera.ScreenToWorldPoint(outerOutlineScreenSpaceUpperRight);
      var outerOutlineScale = outerOutlineWorldSpaceUpperRight - outerOutlineWorldSpaceLowerLeft;

      if (m_BackgroundTransform != null)
        m_BackgroundTransform.localScale = outerOutlineScale;
      if (m_OuterOutlineTransform != null)
        m_OuterOutlineTransform.localScale = outerOutlineScale;
      if (m_InnerOutlineTransform != null)
        m_InnerOutlineTransform.localScale = innerOutlineScale;
      if (m_MaskTransform != null)
        m_MaskTransform.localScale = maskScale;
    }
  }


  public void Disable()
  {
    enabled = false;

    m_Root.position = Vector3.zero;

    if (m_BackgroundTransform != null)
      m_BackgroundTransform.localScale = Vector3.one;
    if (m_MaskTransform != null)
      m_MaskTransform.localScale = Vector3.one;
    if (m_InnerOutlineTransform != null)
      m_InnerOutlineTransform.localScale = Vector3.one;
    if (m_OuterOutlineTransform != null)
      m_OuterOutlineTransform.localScale = Vector3.one;

    DisableRenderers();
  }


  void Enable()
  {
    EnableRenderers();
  }


  void DisableRenderers()
  {
    if (m_BackgroundRenderer != null)
      m_BackgroundRenderer.enabled = false;
    if (m_SpriteMask != null)
      m_SpriteMask.enabled = false;
    if (m_InnerOutlineRenderer != null)
      m_InnerOutlineRenderer.enabled = false;
    if (m_OuterOutlineRenderer != null)
      m_OuterOutlineRenderer.enabled = false;
  }


  void EnableRenderers()
  {
    if (m_BackgroundRenderer != null)
      m_BackgroundRenderer.enabled = true;
    if (m_SpriteMask != null)
      m_SpriteMask.enabled = true;
    if (m_InnerOutlineRenderer != null)
      m_InnerOutlineRenderer.enabled = true;
    if (m_OuterOutlineRenderer != null)
      m_OuterOutlineRenderer.enabled = true;
  }
}
