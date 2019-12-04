/***************************************************
File:           ColorCode.cs
Authors:        Doug Zwick, Christopher Onorati
Last Updated:   6/19/2019
Last Version:   2019.1.4

Description:
  Color code manager for indivitual tiles.

Copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ColorCode : MonoBehaviour
{
  public static Dictionary<TileColor, Color> Colors = new Dictionary<TileColor, Color>()
  {
    { TileColor.RED,      Color.HSVToRGB(  7 / 360.0f, 0.80f, 0.95f) },
    { TileColor.ORANGE,   Color.HSVToRGB( 33 / 360.0f, 0.80f, 0.90f) },
    { TileColor.YELLOW,   Color.HSVToRGB( 55 / 360.0f, 0.90f, 0.95f) },
    { TileColor.GREEN,    Color.HSVToRGB(158 / 360.0f, 0.90f, 0.85f) },
    { TileColor.CYAN,     Color.HSVToRGB(183 / 360.0f, 0.85f, 0.90f) },
    { TileColor.BLUE,     Color.HSVToRGB(210 / 360.0f, 0.80f, 0.95f) },
    { TileColor.PURPLE,   Color.HSVToRGB(290 / 360.0f, 0.75f, 0.90f) },
    { TileColor.MAGENTA,  Color.HSVToRGB(332 / 360.0f, 0.70f, 0.95f) },
  };

  public static Dictionary<TileColor, string> Strings = new Dictionary<TileColor, string>()
  {
    { TileColor.RED,      "A" },
    { TileColor.ORANGE,   "B" },
    { TileColor.YELLOW,   "C" },
    { TileColor.GREEN,    "D" },
    { TileColor.CYAN,     "E" },
    { TileColor.BLUE,     "F" },
    { TileColor.PURPLE,   "G" },
    { TileColor.MAGENTA,  "H" },
  };

  public static Dictionary<KeyCode, TileColor> KeyCodes = new Dictionary<KeyCode, TileColor>()
  {
    { KeyCode.A, TileColor.RED },
    { KeyCode.B, TileColor.ORANGE },
    { KeyCode.C, TileColor.YELLOW },
    { KeyCode.D, TileColor.GREEN },
    { KeyCode.E, TileColor.CYAN },
    { KeyCode.F, TileColor.BLUE },
    { KeyCode.G, TileColor.PURPLE },
    { KeyCode.H, TileColor.MAGENTA },
  };

  public delegate void TileColorEvent(TileColor tileColor);
  public event TileColorEvent ColorSet;

  /************************************************************************************/

  //Current color of the tile.
  [HideInInspector]
  public TileColor m_TileColor;
  [HideInInspector]
  public TileGrid.Element m_Element;

  [System.Serializable]
  public class Events
  {
    public ColorCodeEvent ColorCodeSet;
  }

  public Events m_Events;


  /**
  * FUNCTION NAME: Set
  * DESCRIPTION  : Set the color code.
  * INPUTS       : _tileColor - Color to set of the tile.
  * OUTPUTS      : None
  **/
  public void Set(TileColor _tileColor)
  {
    m_TileColor = _tileColor;
    m_Element.m_TileColor = _tileColor;

    ColorSet?.Invoke(_tileColor);

    var eventData = new ColorCodeEventData()
    {
      m_Color = m_TileColor,
    };

    m_Events.ColorCodeSet.Invoke(eventData);
  }


  public Color GetCurrentColor()
  {
    return Colors[m_TileColor];
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

[System.Serializable]
public class ColorCodeEvent : UnityEvent<ColorCodeEventData> { }

public class ColorCodeEventData
{
  public TileColor m_Color;
}
