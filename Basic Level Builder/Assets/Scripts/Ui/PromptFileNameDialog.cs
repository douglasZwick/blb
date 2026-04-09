/***************************************************
Authors:        Brenden Epp, Douglas Zwick
Last Updated:   12/6/2025

Summary: Base class to use for dialogs that request a file name.
         This class will handle checking for file validation and the input field.
         The Confirm function is to be overwitten with functionality using the file name, such as saving.

Copyright 2018-2025, DigiPen Institute of Technology
***************************************************/

using UnityEngine;
using TMPro;

public class PromptFileNameDialog : ModalDialog
{
  public TMP_InputField m_InputField;

  bool m_FirstUpdate = true;


  private void Awake()
  {
    m_InputField.onSubmit.AddListener(OnSubmit);
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

  void Update()
  {
    if (m_FirstUpdate)
    {
      m_FirstUpdate = false;
      m_InputField.ActivateInputField();
    }

    if (Input.GetButtonDown("Cancel"))
      Cancel();
  }

  public virtual void Confirm() {}

  public void Cancel()
  {
    Close();
  }

  void OnSubmit(string value)
  {
    Confirm();
  }

  private void OnDestroy()
  {
    m_InputField.onSubmit.RemoveListener(OnSubmit);
  }
}
