using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PathNodeIcon : MonoBehaviour
{
  public SpriteRenderer m_IconSprite;
  public TextMeshPro m_Text;

  Transform m_Transform;
  public Vector2Int m_GridIndex { get; private set; }


  private void Awake()
  {
    m_Transform = transform;
  }


  public void Initialize(int index)
  {
    m_Text.text = index.ToString();
  }


  public void Merge(PathNodeIcon other)
  {
    var otherText = other.m_Text.text;

    m_Text.text = $"{otherText}, {m_Text.text}";
  }


  public void MoveTo(Vector2Int gridIndex)
  {
    m_GridIndex = gridIndex;
    m_Transform.position = new Vector3(gridIndex.x, gridIndex.y, 0);
  }


  public void Show()
  {
    m_IconSprite.enabled = true;
    m_Text.enabled = true;
  }


  public void Hide()
  {
    m_IconSprite.enabled = false;
    m_Text.enabled = false;
  }
}
