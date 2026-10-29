/***************************************************
Authors:        Brenden Epp
Last Updated:   5/6/2025

Copyright 2018-2025, DigiPen Institute of Technology
***************************************************/

using UnityEngine;
using UnityEngine.InputSystem;

public class ExportAsDialog : PromptFileNameDialog
{
  private void OnEnable()
  {
    InputSystem.actions["Cancel"].performed += OnCancel;
  }

  private void OnDisable()
  {
    InputSystem.actions["Cancel"].performed -= OnCancel;
  }

  private void OnCancel(InputAction.CallbackContext context)
  {
    Close();
  }

  public override void Confirm()
  {
    if (!FileDirUtilities.IsFileNameValid(m_InputField.text))
      return;

    Close();
    FileSystem.Instance.TryStartExportSavingThread(m_InputField.text);
  }
}
