/***************************************************
Authors:        Brenden Epp
Last Updated:   3/30/2026

Copyright 2018-2026, DigiPen Institute of Technology

Description:    Global dialog manager providing easy access to modal dialogs
                without requiring ModalDialogAdder components on every object.
***************************************************/

using System.Threading.Tasks;
using UnityEngine;

public static class DialogManager
{
  private static ModalDialogMaster s_ModalDialogMaster;
  private static UiGenericModalDialog s_GenericDialogPrefab;

  /// <summary>
  /// Initialize the DialogManager with the required prefab.
  /// Call this once at startup (e.g., in a manager script or scene initialization).
  /// </summary>
  public static void Initialize(ModalDialogMaster master)
  {
    s_ModalDialogMaster = master;
    s_GenericDialogPrefab = Resources.Load<UiGenericModalDialog>("Prefabs/Ui/GenericModalDialogUi");
  }

  /// <summary>
  /// Shows an "Ask to Save" dialog for unsaved changes.
  /// </summary>
  /// <param name="levelName">The name of the level with unsaved changes</param>
  /// <returns>A task that completes with the dialog result</returns>
  public static Task<ModalDialog.DialogResult> ShowAskToSaveDialog(string levelName)
  {
    string dialogMessage = $"The level <i>{levelName}</i> has unsaved changes.{System.Environment.NewLine}Would you like to save?";
    return ShowGenericDialog(UiGenericModalDialog.ButtonOptions.ConfirmDenyCancel, dialogMessage);
  }

  /// <summary>
  /// Shows a "Confirm Destructive Action" dialog.
  /// </summary>
  /// <param name="actionDescription">Description of the destructive action</param>
  /// <returns>A task that completes with the dialog result</returns>
  public static Task<ModalDialog.DialogResult> ShowConfirmDestructiveDialog(string actionDescription)
  {
    return ShowGenericDialog(UiGenericModalDialog.ButtonOptions.ConfirmAndDeny, actionDescription);
  }

  /// <summary>
  /// Shows a confirmation dialog for overwriting a file.
  /// </summary>
  /// <param name="fileName">The name of the file that will be overwritten</param>
  /// <returns>A task that completes with the dialog result</returns>
  public static Task<ModalDialog.DialogResult> ShowConfirmOverwriteDialog(string fileName)
  {
    string overwriteMessage = $"<b>\"{fileName}\"</b>{System.Environment.NewLine}" +
      $"A file with this name already exists{System.Environment.NewLine}" +
      "Do you want to overwrite it?";
    return ShowGenericDialog(UiGenericModalDialog.ButtonOptions.ConfirmAndDeny, overwriteMessage);
  }

  /// <summary>
  /// Shows a generic modal dialog with custom button options.
  /// </summary>
  /// <param name="buttonOptions">Flags indicating which buttons should be displayed</param>
  /// <param name="message">The text to display in the dialog</param>
  /// <returns>A task that completes with the dialog result</returns>
  public static Task<ModalDialog.DialogResult> ShowGenericDialog(UiGenericModalDialog.ButtonOptions buttonOptions, string message)
  {
    if (GlobalData.AreEffectsUnderway())
      return Task.FromResult(ModalDialog.DialogResult.Cancel);

    TaskCompletionSource<ModalDialog.DialogResult> tcs = new();

    var x = Screen.width / 2f;
    var y = Screen.height / 2f;
    var rectPoint = new Vector2(x, y);

    var dialog = Object.Instantiate(s_GenericDialogPrefab);
    dialog.SetUpGeneric(buttonOptions, tcs);
    dialog.Setup(s_ModalDialogMaster, rectPoint, new string[] { message });
    dialog.Open();

    return tcs.Task;
  }
}