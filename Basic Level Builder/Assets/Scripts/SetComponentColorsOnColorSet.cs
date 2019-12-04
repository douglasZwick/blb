using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ColorCode))]
public class SetComponentColorsOnColorSet : MonoBehaviour
{
  public List<SpriteRenderer> m_SpritesToColor;
  public List<Image> m_UiImagesToColor;

  ColorCode m_ColorCode;


  private void Awake()
  {
    m_ColorCode = GetComponent<ColorCode>();

    m_ColorCode.ColorSet += OnColorSet;
  }


  void OnColorSet(TileColor tileColor)
  {
    var color = ColorCode.Colors[tileColor];

    foreach (var sprite in m_SpritesToColor)
      ApplyRGBtoSpriteRenderer(sprite, color);
    foreach (var image in m_UiImagesToColor)
      ApplyRGBtoUiImage(image, color);
  }


  private void OnDestroy()
  {
    m_ColorCode.ColorSet -= OnColorSet;
  }


  void ApplyRGBtoSpriteRenderer(SpriteRenderer sprite, Color color)
  {
    var oldColor = sprite.color;
    color.a = oldColor.a;
    sprite.color = color;
  }


  void ApplyRGBtoUiImage(Image image, Color color)
  {
    var oldColor = image.color;
    color.a = oldColor.a;
    image.color = color;
  }
}
