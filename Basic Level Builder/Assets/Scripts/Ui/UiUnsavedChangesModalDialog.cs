/***************************************************
Authors:        Brenden Epp
Last Updated:   2/23/2026

Copyright 2018-2026, DigiPen Institute of Technology
***************************************************/

using UnityEngine;

public class UiUnsavedChangesModalDialog : ModalDialog
{
  public TMPro.TextMeshProUGUI m_PromptTxt;

  public override void Open()
  {
    var anchoredX = 0;
    var anchoredY = 0;
    var anchoredPosition = new Vector2(anchoredX, anchoredY);
    m_RectTransform.anchoredPosition = anchoredPosition;

    base.Open();

    // scaling effects
  }

  public override void StringsSetup(string[] strings = null)
  {
    // Add the level name to the string
    m_PromptTxt.text = $"The level <i>{strings[0]}</i> has unsaved changes. Would you like to save?";
  }

  public override void Close()
  {
    base.Close();

    // scaling effects
    Destroy(gameObject);
  }

  public void SaveAndQuit()
  {
    FileSystem.Instance.SaveAndQuit();
    Close();
  }

  public void QuitWithoutSave()
  {
    FileSystem.Instance.QuitWithoutSave();
    Close();
  }
}
