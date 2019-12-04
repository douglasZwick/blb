/***************************************************
File:           TeleporterLogic.cs
Authors:        Christopher Onorati
Last Updated:   6/12/2019
Last Version:   2019.1.4

Description:
  Holds info on what color the teleporter is.

Copyright 2018-2019, DigiPen Institute of Technology
***************************************************/

using UnityEngine;

public class TeleporterLogic : MonoBehaviour
{
  /************************************************************************************/

  //Destination of the teleporter.
  [HideInInspector]
  public Transform m_pDestination;

  /************************************************************************************/

  Transform m_cTransform;
  ColorCode m_ColorCode;
  TeleporterLogic m_DestinationTeleporter;


  private void Awake()
  {
    m_cTransform = GetComponent<Transform>();
    m_ColorCode = GetComponent<ColorCode>();

    GlobalData.PlayModeToggled += OnPlayModeToggled;
  }


  void OnPlayModeToggled(bool isInPlayMode)
  {
    FindDestination();
  }


  void FindDestination()
  {
    //Lowest distance between two teleporters is what we use.
    float flLeastDistance = Mathf.Infinity;

    //Get all teleporters.
    TeleporterLogic[] teleporters = FindObjectsOfType<TeleporterLogic>();

    //Check all of the LPK_TagManagers for the tags.
    foreach (var teleporter in teleporters)
    {
      var colorCode = teleporter.GetComponent<ColorCode>();

      if (colorCode.m_TileColor == m_ColorCode.m_TileColor)
      {
        //Do not count ourselves.
        if (teleporter.gameObject == gameObject)
          continue;

        var distance = Vector3.Distance(m_cTransform.position, teleporter.transform.position);
        //New destination found!
        if (distance < flLeastDistance)
        {
          flLeastDistance = distance;
          m_pDestination = teleporter.transform;
          m_DestinationTeleporter = teleporter;
        }
      }
    }
  }


  private void OnDestroy()
  {
    GlobalData.PlayModeToggled -= OnPlayModeToggled;
  }
}
