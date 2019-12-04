using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionMaster : MonoBehaviour
{
  public static ActionGroup Actions = new ActionGroup();


  private void Update()
  {
    var dt = Time.deltaTime;
    Actions.Update(dt);
  }
}
