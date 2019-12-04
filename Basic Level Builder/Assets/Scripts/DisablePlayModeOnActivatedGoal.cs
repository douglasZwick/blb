using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisablePlayModeOnActivatedGoal : MonoBehaviour
{
  public void OnActivatedGoal(GoalEventData eventData)
  {
    GlobalData.DisablePlayMode();
  }
}
