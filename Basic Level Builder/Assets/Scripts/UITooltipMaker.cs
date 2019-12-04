/***************************************************
File:           UIToolTip.cs
Authors:        Christopher Onorati
Last Updated:   6/17/2019
Last Version:   2019.1.4

Description:
  Class used to create a tooltip when the mouse is hovering
  over a given UI element.

Copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using UnityEngine;
using TMPro;  //Text mesh pro.
using UnityEngine.EventSystems; //Pointer over and exit.

public class UITooltipMaker : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
  /************************************************************************************/

  [Tooltip("Text to show within the tooltip.")]
  public string m_Text;

  public GameObject m_TooltipPrefab;
  public RectTransform m_TooltipRoot;

  UiSafePositioner m_RootPositioner;

  /************************************************************************************/

  //Flag to check if the mouse is over the UI element.
  bool m_IsHoveredOver;

  //How long the user has had the mouse cursor hovering over the given UI element.
  float m_HoverDuration;

  //Tooltip game object.  We destroy this as soon as the user is no longer hovering over the UI element.
  GameObject m_Tooltip;

  /************************************************************************************/

  //Set for how long the user must be hovering over the UI element before a tooltip appears.
  static readonly float s_DelayTime = 0.5f;
  static readonly Vector2 s_OffsetVector = new Vector2(0, -30); // in pixels

  /************************************************************************************/

  private void Awake()
  {
    m_RootPositioner = m_TooltipRoot.GetComponent<UiSafePositioner>();
  }

  public void OnPointerEnter(PointerEventData eventData)
  {
    m_IsHoveredOver = true;
  }

  void Update()
  {
    if (!m_IsHoveredOver)
      return;

    m_HoverDuration += Time.deltaTime;

    if (m_HoverDuration >= s_DelayTime && m_Tooltip == null)
      CreateToolTip();
  }

  void CreateToolTip()
  {
    m_Tooltip = Instantiate(m_TooltipPrefab);
    var tooltipRT = m_Tooltip.GetComponent<RectTransform>();
    var tooltipPosition = ScreenPointToRectPoint(Input.mousePosition);
    var desiredPosition = tooltipPosition + s_OffsetVector;
    m_RootPositioner.AttachAtSafePosition(tooltipRT, desiredPosition);

    var uiTooltip = m_Tooltip.GetComponent<UiTooltip>();
    uiTooltip.SetText(m_Text);
  }

  public void OnPointerExit(PointerEventData eventData)
  {
    if (m_Tooltip)
      Destroy(m_Tooltip);

    m_IsHoveredOver = false;
    m_HoverDuration = 0.0f;
  }


  public void UpdateText(string newText)
  {
    m_Text = newText;

    if (m_Tooltip != null)
    {
      var uiTooltip = m_Tooltip.GetComponent<UiTooltip>();

      if (uiTooltip != null)
      {
        uiTooltip.SetText(newText);
      }
    }
  }


  public Vector2 ScreenPointToRectPoint(Vector3 screenPoint)
  {
    RectTransformUtility.ScreenPointToLocalPointInRectangle(m_TooltipRoot,
      screenPoint, null, out Vector2 localPoint);
    return m_TooltipRoot.TransformPoint(localPoint);
  }
}
