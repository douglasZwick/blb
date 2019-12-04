using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class OverwriteConfirmer : ModalDialog
{
  public TextMeshProUGUI m_BodyText;

  FileSystem m_FS;


  private void Awake()
  {
    m_FS = FindObjectOfType<FileSystem>();
  }


  public override void Open()
  {
    var anchoredX = 0;
    var anchoredY = 0;
    var anchoredPosition = new Vector2(anchoredX, anchoredY);
    m_RectTransform.anchoredPosition = anchoredPosition;

    base.Open();

    // scaling effects
  }


  public override void Close()
  {
    base.Close();

    // scaling effects

    Destroy(gameObject);
  }


  public override void StringsSetup(string[] strings = null)
  {
    var fileName = strings[0];
    var currentText = m_BodyText.text;
    var newText = currentText.Replace("FILE_NAME", fileName);
    m_BodyText.text = newText;
  }


  public void Overwrite()
  {
    if (!enabled)
      return;

    m_FS.ConfirmOverwrite();
    Close();
  }


  public void Cancel()
  {
    if (!enabled)
      return;

    m_FS.CancelOverwrite();
    Close();
  }
}
