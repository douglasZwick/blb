using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class DirectionPicker : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
{
  public Direction m_Direction;
  public DirectionPickerDialog m_Dialog;
  public bool m_UseImageAlpha = true;

  Image m_Image;


  private void Awake()
  {
    m_Image = GetComponent<Image>();

    if (m_UseImageAlpha)
      m_Image.alphaHitTestMinimumThreshold = 0.9f;
  }


  public void OnPointerClick(PointerEventData eventData)
  {
    m_Dialog.SetAndClose(m_Direction);
  }


  public void OnPointerEnter(PointerEventData eventData)
  {
    m_Dialog.Set(m_Direction);
  }
}
