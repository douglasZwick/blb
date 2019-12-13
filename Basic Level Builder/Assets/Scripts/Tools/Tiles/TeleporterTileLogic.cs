/***************************************************
File:           TeleporterTileLogic.cs
Authors:        Christopher Onorati
Last Updated:   6/12/2019
Last Version:   2019.1.4

Description:
  Logic for the tile that spawns teleporters.

Copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using UnityEngine;

[RequireComponent(typeof(ColorCode))]
public class TeleporterTileLogic : MonoBehaviour
{
  // This object's Transform
  [HideInInspector] public Transform m_Transform;
  // This object's ColorCode
  [HideInInspector] public ColorCode m_ColorCode;
  // The Transform of the destination teleporter
  public TeleporterMaster m_Master;

  [System.Serializable]
  public class Events
  {
    public TeleportEvent TeleportedFrom;
    public TeleportEvent TeleportedTo;
  }

  public Events m_Events;


  void Awake()
  {
    m_Transform = transform;
    m_ColorCode = GetComponent<ColorCode>();

    GlobalData.PlayModeToggled += OnPlayModeToggled;
  }


  void OnPlayModeToggled(bool isInPlayMode)
  {
    if (isInPlayMode)
      Register();
  }


  void Register()
  {
    m_Master.Add(this, m_ColorCode.m_TileColor);
  }


  public TeleporterTileLogic FindNearest()
  {
    var thisPosition = m_Transform.position;
    var leastSqDistance = float.PositiveInfinity;
    var matches = m_Master.m_Teleporters[(int)m_ColorCode.m_TileColor];
    TeleporterTileLogic nearestTeleporter = null;
    
    foreach (var other in matches)
    {
      if (other == this)
        continue;

      var otherPosition = other.m_Transform.position;
      var difference = otherPosition - thisPosition;
      var sqDistance = difference.sqrMagnitude;

      if (sqDistance < leastSqDistance)
      {
        leastSqDistance = sqDistance;
        nearestTeleporter = other;
      }
    }

    return nearestTeleporter;
  }


  private void OnDestroy()
  {
    GlobalData.PlayModeToggled -= OnPlayModeToggled;
  }
}
