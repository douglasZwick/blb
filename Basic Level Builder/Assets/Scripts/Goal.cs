using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Goal : MonoBehaviour
{
  [System.Serializable]
  public class Events
  {
    public GoalEvent GoalActivated;
  }

  public Events m_Events;

  bool m_Activated = false;


  private void Awake()
  {
    GlobalData.PlayModeToggled += OnPlayModeToggled;
  }


  public void AttemptActivate(GoalActivator activator)
  {
    if (!m_Activated)
      Activate(activator);
  }


  void OnPlayModeToggled(bool isInPlayMode)
  {
    if (isInPlayMode)
      m_Activated = false;
  }


  void Activate(GoalActivator activator)
  {
    m_Activated = true;

    var eventData = new GoalEventData()
    {
      m_Goal = this,
      m_Activator = activator,
    };

    activator.Activated(eventData);
    m_Events.GoalActivated.Invoke(eventData);
  }


  private void OnDestroy()
  {
    GlobalData.PlayModeToggled -= OnPlayModeToggled;
  }
}


[System.Serializable]
public class GoalEvent : UnityEvent<GoalEventData> { }

public class GoalEventData
{
  public Goal m_Goal;
  public GoalActivator m_Activator;
}
