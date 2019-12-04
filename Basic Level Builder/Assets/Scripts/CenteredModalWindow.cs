using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CenteredModalWindow : ModalDialog
{
  public override void Open()
  {
    var anchoredX = 0;
    var anchoredY = 0;
    var anchoredPosition = new Vector2(anchoredX, anchoredY);
    m_RectTransform.anchoredPosition = anchoredPosition;

    base.Open();
  }


  public override void Close()
  {
    base.Close();

    Destroy(gameObject);
  }
}
