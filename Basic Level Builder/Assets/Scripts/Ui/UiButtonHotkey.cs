using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class UiButtonHotkey : MonoBehaviour
{
  [System.Serializable]
  public class Hotkey
  {
    public InputActionReference m_Action;
    public UnityEvent m_Event;
    public bool m_SubjectToHotkeyDisabling;
    public bool m_AllowedInPlayMode;
    public bool m_AllowedInUiPopups = false;
  }

  [SerializeField]
  private List<Hotkey> m_List = new();

  private void OnEnable()
  {
    foreach (Hotkey hotkey in m_List)
    {
      if (hotkey.m_Action == null || hotkey.m_Action.action == null)
        continue;

      hotkey.m_Action.action.performed += OnHotkeyPressed;
    }
  }

  private void OnDisable()
  {
    foreach (Hotkey hotkey in m_List)
    {
      if (hotkey.m_Action == null || hotkey.m_Action.action == null)
        continue;

      hotkey.m_Action.action.performed -= OnHotkeyPressed;
    }
  }

  public string GetHotkeyString()
  {
    if (m_List == null || m_List.Count == 0)
      return null;
      
    InputAction action = m_List.FirstOrDefault().m_Action.action;
    if (action == null)
      return string.Empty;

    List<string> bindings = new();
    List<string> compositeKeys = new();

    foreach (InputBinding binding in action.bindings)
    {
      if (binding.isComposite)
        continue;

      string display = binding.ToDisplayString();
      if (string.IsNullOrWhiteSpace(display))
        continue;

      if (binding.isPartOfComposite)
      {
        compositeKeys.Add(display);
        continue;
      }

      if (compositeKeys.Count > 0)
      {
        bindings.Add(string.Join(" + ", compositeKeys));
        compositeKeys.Clear();
      }

      bindings.Add(display);
    }

    if (compositeKeys.Count > 0)
    {
      bindings.Add(string.Join(" + ", compositeKeys));
      compositeKeys.Clear();
    }

    if (bindings.Count == 0)
      return string.Empty;

    return string.Join(" / ", bindings.Distinct());
  }

  private void OnHotkeyPressed(InputAction.CallbackContext context)
  {
    Hotkey hotkey = m_List.Find(h => h.m_Action.action == context.action);
    if (hotkey != null)
      InvokeHotkeyEvent(hotkey);
  }

  private void InvokeHotkeyEvent(Hotkey hotkey)
  {
    if (hotkey.m_SubjectToHotkeyDisabling && !HotkeyMaster.s_HotkeysEnabled)
      return;

    if (!hotkey.m_AllowedInPlayMode && GlobalData.IsInPlayMode())
      return;

    if (!hotkey.m_AllowedInUiPopups && GlobalData.IsInUiPopup())
      return;

    hotkey.m_Event.Invoke();
  }
}
