using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class ColorCodePicker : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
{
  public TileColor m_TileColor;
  public ColorCodePickerDialog m_Dialog;
  Image m_Image;


  void Awake()
  {
    m_Image = GetComponent<Image>();
    m_Image.color = ColorCode.Colors[m_TileColor];
    m_Image.alphaHitTestMinimumThreshold = 0.9f;
  }


  public void OnPointerClick(PointerEventData eventData)
  {
    m_Dialog.SetAndClose(m_TileColor);
  }


  public void OnPointerEnter(PointerEventData eventData)
  {
    m_Dialog.Set(m_TileColor);
  }
}
