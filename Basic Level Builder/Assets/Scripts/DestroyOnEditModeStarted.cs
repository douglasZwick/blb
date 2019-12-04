using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnEditModeStarted : MonoBehaviour
{
  private void Awake()
  {
    GlobalData.PlayModeToggled += OnPlayModeToggled;
  }


  void OnPlayModeToggled(bool isInPlayMode)
  {
    Destroy(gameObject);
  }


  private void OnDestroy()
  {
    GlobalData.PlayModeToggled -= OnPlayModeToggled;
  }
}
