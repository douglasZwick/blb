/***************************************************
Authors:        Brenden Epp
Last Updated:   5/6/2025

Copyright 2018-2025, DigiPen Institute of Technology
***************************************************/

using UnityEngine;

public class ExportAsDialog : PromptFileNameDialog
{
  private void Update()
  {
    if (Input.GetButtonDown("Cancel"))
    {
      Close();
    }
  }

  public override void Confirm()
  {
    if (!FileDirUtilities.IsFileNameValid(m_InputField.text))
      return;

    Close();
    FileSystem.Instance.TryStartExportSavingThread(m_InputField.text);
  }
}
