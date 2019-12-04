/***************************************************
File:           SwitchLogic.cs
Authors:        Christopher Onorati
Last Updated:   6/10/2019
Last Version:   2019.1.4

Description:
  Holds info on what color the switch is.

Copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(ColorCode))]
public class SwitchLogic : MonoBehaviour
{
  public Transform m_Lever;
  public float m_ActivatedLeverY = -0.25f;
  public float m_LeverMoveDuration = 0.5f;
  public float m_ActivatedOpacity = 0.25f;
  public float m_FadeDuration = 0.25f;

  [System.Serializable]
  public class Events
  {
    public SwitchEvent SwitchActivated;
  }

  public Events m_Events;

  ColorCode m_ColorCode;
  bool m_Activated = false;
  List<DoorLogic> m_Doors = new List<DoorLogic>();


  void Awake()
  {
    m_ColorCode = GetComponent<ColorCode>();

    GlobalData.PlayModeToggled += OnPlayModeToggled;
  }


  void OnPlayModeToggled(bool isInPlayMode)
  {
    FindDoors();
  }


  public void AttemptActivate()
  {
    if (m_Activated)
      return;

    Activate();
  }


  void Activate()
  {
    m_Activated = true;

    foreach (var door in m_Doors)
    {
      if (door != null)
        door.OpenThisPart();
    }

    var activateSeq = ActionMaster.Actions.Sequence();
    var leverEase = new Ease(Ease.Power.InOut, 6);
    activateSeq.MoveLocalY(m_Lever.gameObject, m_ActivatedLeverY, m_LeverMoveDuration, leverEase);
    var fadeGrp = activateSeq.Group();
    var fadeColor = new Color(1, 1, 1, m_ActivatedOpacity);
    fadeGrp.SpriteColor(gameObject, fadeColor, m_FadeDuration);
    fadeGrp.SpriteColor(m_Lever.gameObject, fadeColor, m_FadeDuration);
  }


  void FindDoors()
  {
    var allDoors = FindObjectsOfType<DoorLogic>();

    foreach (var door in allDoors)
    {
      var colorCode = door.GetComponent<ColorCode>();

      if (colorCode.m_TileColor == m_ColorCode.m_TileColor)
      {
        m_Doors.Add(door);
      }
    }
  }


  private void OnDestroy()
  {
    GlobalData.PlayModeToggled -= OnPlayModeToggled;
  }
}


[System.Serializable]
public class SwitchEvent : UnityEvent<SwitchEventData> { }

public class SwitchEventData
{
  public SwitchLogic m_Switch;
  public SwitchActivator m_Activator;
}
