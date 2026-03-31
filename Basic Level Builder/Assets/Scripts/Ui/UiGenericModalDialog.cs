/***************************************************
Authors:        Brenden Epp
Last Updated:   3/30/2026

Copyright 2018-2026, DigiPen Institute of Technology

Description:    Generic modal dialog that supports Confirm, Deny, and Cancel buttons.
               Uses a single prefab with dynamic button visibility configuration.
***************************************************/

using UnityEngine;

public class UiGenericModalDialog : ModalDialog
{
  public enum ButtonOptions
  {
    None = 0,
    Confirm = 1,
    Deny = 2,
    Cancel = 4,
    ConfirmAndCancel = Confirm | Cancel,
    ConfirmAndDeny = Confirm | Deny,
    ConfirmDenyCancel = Confirm | Deny | Cancel,
    DenyAndCancel = Deny | Cancel,
  }

  [SerializeField]
  private TMPro.TextMeshProUGUI m_PromptTxt;
  [SerializeField]
  private UnityEngine.UI.Button m_ConfirmButton;
  [SerializeField]
  private UnityEngine.UI.Button m_DenyButton;
  [SerializeField]
  private UnityEngine.UI.Button m_CancelButton;

  public delegate void ConfirmAction();
  public static event ConfirmAction OnConfirm;
  public delegate void DenyAction();
  public static event DenyAction OnDeny;
  public delegate void CancelAction();
  public static event CancelAction OnCancel;
  public delegate void RemoveSub();
  public static event RemoveSub OnRemoveSub;

  private ButtonOptions m_ButtonOptions = ButtonOptions.ConfirmDenyCancel;

  private void Update()
  {
    if (Input.GetButtonDown("Cancel"))
      CancelPressed();
  }

  public void SetButtonOptions(ButtonOptions options)
  {
    m_ButtonOptions = options;
  }

  public override void StringsSetup(string[] strings = null)
  {
    if (strings != null && strings.Length > 0)
    {
      m_PromptTxt.text = strings[0];
    }
  }

  public override void Open()
  {
    var anchoredX = 0;
    var anchoredY = 0;
    var anchoredPosition = new Vector2(anchoredX, anchoredY);
    m_RectTransform.anchoredPosition = anchoredPosition;

    ConfigureButtons();

    base.Open();
  }

  public override void Close()
  {
    base.Close();
    Destroy(gameObject);
  }

  private void ConfigureButtons()
  {
    bool showConfirm = (m_ButtonOptions & ButtonOptions.Confirm) != 0;
    bool showDeny = (m_ButtonOptions & ButtonOptions.Deny) != 0;
    bool showCancel = (m_ButtonOptions & ButtonOptions.Cancel) != 0;

    m_ConfirmButton.gameObject.SetActive(showConfirm);
    m_DenyButton.gameObject.SetActive(showDeny);
    m_CancelButton.gameObject.SetActive(showCancel);
  }

  public void ConfirmPressed()
  {
    Close();
    OnConfirm?.Invoke();
    OnRemoveSub?.Invoke();
  }

  public void DenyPressed()
  {
    Close();
    OnDeny?.Invoke();
    OnRemoveSub?.Invoke();
  }

  public void CancelPressed()
  {
    Close();
    OnCancel?.Invoke();
    OnRemoveSub?.Invoke();
  }
}
