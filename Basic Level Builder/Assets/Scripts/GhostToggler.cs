using UnityEngine;

public class GhostToggler : MonoBehaviour
{
  public static bool s_IsInGhostMode { get; private set; }

  void EnableGhostMode()
  {
    s_IsInGhostMode = true;

    GlobalData.DispatchGhostModeEnabled();
  }


  void DisableGhostMode()
  {
    s_IsInGhostMode = false;

    GlobalData.DispatchGhostModeDisabled();
  }


  public void ToggleGhostMode()
  {
    if (s_IsInGhostMode)
      DisableGhostMode();
    else
      EnableGhostMode();
  }
}
