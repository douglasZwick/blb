using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ModalDialogAdder : MonoBehaviour
{
  public List<ModalDialog> m_ModalDialogPrefabs;

  ModalDialogMaster m_ModalDialogMaster;


  private void Awake()
  {
    m_ModalDialogMaster = FindObjectOfType<ModalDialogMaster>();
  }

  public void RequestDialogsAtTransform()
  {
    RequestDialogsAtTransformWithStrings();
  }


  public void RequestDialogsAtCenter()
  {
    RequestDialogsAtCenterWithStrings();
  }

  public void RequestDialogsAtTransformWithStrings(params string[] strings)
  {
    if (GlobalData.AreEffectsUnderway())
      return;

    var worldPoint = transform.position;

    foreach (var prefab in m_ModalDialogPrefabs)
      m_ModalDialogMaster.RequestDialogAtWorldPoint(prefab, worldPoint, strings);

    m_ModalDialogMaster.Begin(true);
  }

  public void RequestDialogsAtCenterWithStrings(params string[] strings)
  {
    if (GlobalData.AreEffectsUnderway())
      return;

    foreach (var prefab in m_ModalDialogPrefabs)
      m_ModalDialogMaster.RequestDialogAtCenter(prefab, strings);

    m_ModalDialogMaster.Begin(false);
  }

  public Task<ModalDialog.DialogResult> RequestAskToSaveDialogAsync(params string[] strings)
  {
    var tcs = new TaskCompletionSource<ModalDialog.DialogResult>();
    void onConfirm() => tcs.SetResult(ModalDialog.DialogResult.Confirm);
    void onDeny() => tcs.SetResult(ModalDialog.DialogResult.Deny);
    void onCancel() => tcs.SetResult(ModalDialog.DialogResult.Cancel);
    void removeHandler()
    {
      UiAskToSaveModalDialog.OnConfirmSave -= onConfirm;
      UiAskToSaveModalDialog.OnDenySave -= onDeny;
      UiAskToSaveModalDialog.OnCancelAction -= onCancel;
      UiAskToSaveModalDialog.OnRemoveSub -= removeHandler;
    }
    UiAskToSaveModalDialog.OnConfirmSave += onConfirm;
    UiAskToSaveModalDialog.OnDenySave += onDeny;
    UiAskToSaveModalDialog.OnCancelAction += onCancel;
    UiAskToSaveModalDialog.OnRemoveSub += removeHandler;
    RequestDialogsAtCenterWithStrings(strings);
    return tcs.Task;
  }

  public Task<ModalDialog.DialogResult> RequestConfirmDestructiveDialogAsync(params string[] strings)
  {
    var tcs = new TaskCompletionSource<ModalDialog.DialogResult>();
    void onConfirm() => tcs.SetResult(ModalDialog.DialogResult.Confirm);
    void onDeny() => tcs.SetResult(ModalDialog.DialogResult.Deny);
    void removeHandler()
    {
      UiConfirmDestructiveActionModalDialog.OnConfirmDestructiveAction -= onConfirm;
      UiConfirmDestructiveActionModalDialog.OnDenyDestructiveAction -= onDeny;
      UiConfirmDestructiveActionModalDialog.OnRemoveSub -= removeHandler;
    }
    UiConfirmDestructiveActionModalDialog.OnConfirmDestructiveAction += onConfirm;
    UiConfirmDestructiveActionModalDialog.OnDenyDestructiveAction += onDeny;
    UiConfirmDestructiveActionModalDialog.OnRemoveSub += removeHandler;
    RequestDialogsAtCenterWithStrings(strings);
    return tcs.Task;
  }
}
