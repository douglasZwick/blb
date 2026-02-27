/***************************************************
Authors:        Brenden Epp
Last Updated:   2/23/2026

Copyright 2018-2026, DigiPen Institute of Technology
***************************************************/

using UnityEngine;

public class UiAskToSaveModalDialog : ModalDialog
{
  public TMPro.TextMeshProUGUI m_PromptTxt;
  public delegate void ConfirmSave();
  public static event ConfirmSave OnConfirmSave;
  public delegate void DenySave();
  public static event DenySave OnDenySave;
  public delegate void CancelAction();
  public static event CancelAction OnCancelAction;
  public delegate void RemoveSub();
  public static event RemoveSub OnRemoveSub;
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

  public void Yes()
  {
    Close();
    OnConfirmSave?.Invoke();
    OnRemoveSub?.Invoke();
  }

  public void No()
  {
    Close();
    OnDenySave?.Invoke();
    OnRemoveSub?.Invoke();
  }


  public void Cancel()
  {
    Close();
    OnCancelAction?.Invoke();
    OnRemoveSub?.Invoke();
  }
}
