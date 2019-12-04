using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonSwitchboard : MonoBehaviour
{
  public void AttemptUndo()
  {
    OperationSystem.AttemptUndo();
  }


  public void AttemptRedo()
  {
    OperationSystem.AttemptRedo();
  }
}
