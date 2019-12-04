using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleActiveStateOnEditingToggled : MonoBehaviour
{
  private void Awake()
  {
    GlobalData.EditingEnabled += OnEditingEnabled;
    GlobalData.EditingDisabled += OnEditingDisabled;
  }


  void OnEditingEnabled()
  {
    gameObject.SetActive(true);
  }


  void OnEditingDisabled()
  {
    gameObject.SetActive(false);
  }


  private void OnDestroy()
  {
    GlobalData.EditingEnabled -= OnEditingEnabled;
    GlobalData.EditingDisabled -= OnEditingDisabled;
  }
}
