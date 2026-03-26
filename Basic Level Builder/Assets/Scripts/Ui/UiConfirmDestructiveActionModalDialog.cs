/***************************************************
Authors:        Brenden Epp
Last Updated:   11/17/2025

Copyright 2018-2025, DigiPen Institute of Technology
***************************************************/

using UnityEngine;
using UnityEngine.UIElements;

public class UiConfirmDestructiveActionModalDialog : ModalDialog
{
  public TMPro.TextMeshProUGUI m_PromptTxt;
  public delegate void ConfirmDestructiveAction();
  public static event ConfirmDestructiveAction OnConfirmDestructiveAction;
  public delegate void DenyDestructiveAction();
  public static event DenyDestructiveAction OnDenyDestructiveAction;
  public delegate void RemoveSub();
  public static event RemoveSub OnRemoveSub;

  private void Update()
  {
    if (Input.GetButtonDown("Cancel"))
    {
      Cancel();
    }
  }

  public override void StringsSetup(string[] strings = null)
  {
    foreach (string s in strings)
    {
      m_PromptTxt.text = s;
    }
  }

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


  public void Confirm()
  {
    OnConfirmDestructiveAction?.Invoke();
    OnRemoveSub?.Invoke();
    Close();
  }


  public void Cancel()
  {
    OnDenyDestructiveAction?.Invoke();
    OnRemoveSub?.Invoke();
    Close();
  }
}
