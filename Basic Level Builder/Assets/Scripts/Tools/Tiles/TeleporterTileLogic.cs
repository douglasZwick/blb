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
  Transform m_Transform;
  // This object's ColorCode
  [HideInInspector] public ColorCode m_ColorCode;
  // The Transform of the destination teleporter
  public Transform m_Destination { get; private set; }
  public TeleporterTileLogic m_DestinationTeleporter { get; private set; }

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
      FindDestination();
  }


  void FindDestination()
  {
    var leastSqDistance = float.PositiveInfinity;

    var allTeleporters = FindObjectsOfType<TeleporterTileLogic>();

    foreach (var otherTeleporter in allTeleporters)
    {
      if (otherTeleporter == this)
        continue;

      if (otherTeleporter.m_ColorCode.m_TileColor != m_ColorCode.m_TileColor)
        continue;

      var difference = otherTeleporter.m_Transform.position - m_Transform.position;
      var sqDistance = Vector3.SqrMagnitude(difference);

      if (sqDistance < leastSqDistance)
      {
        leastSqDistance = sqDistance;
        m_Destination = otherTeleporter.m_Transform;
        m_DestinationTeleporter = otherTeleporter;
      }
    }
  }


  private void OnDestroy()
  {
    GlobalData.PlayModeToggled -= OnPlayModeToggled;
  }
}
