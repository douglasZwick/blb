using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class TeleporterMaster : ScriptableObject
{
  //[HideInInspector]
  public List<List<TeleporterTileLogic>> m_Teleporters;

  int m_ColorCount;


  private void OnEnable()
  {
    var colors = System.Enum.GetNames(typeof(TileColor));
    m_ColorCount = colors.Length;

    Initialize();

    GlobalData.PlayModePreToggle += OnPlayModePreToggle;
  }


  void OnPlayModePreToggle(bool isInPlayMode)
  {
    if (isInPlayMode)
      Initialize();
  }


  void Initialize()
  {
    m_Teleporters = new List<List<TeleporterTileLogic>>(m_ColorCount);

    for (var i = 0; i < m_ColorCount; ++i)
      m_Teleporters.Add(new List<TeleporterTileLogic>());
  }


  public void Add(TeleporterTileLogic teleporter, TileColor color)
  {
    m_Teleporters[(int)color].Add(teleporter);
  }


  private void OnDestroy()
  {
    GlobalData.PlayModePreToggle -= OnPlayModePreToggle;
  }
}
