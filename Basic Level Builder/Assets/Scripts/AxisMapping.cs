using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AxisMappingEntry
{
  public string m_axisName;
  public List<string> m_buttons = new();
}

public class AxisMapping : ScriptableObject
{
  public List<AxisMappingEntry> m_entries = new();

  private static AxisMapping _instance;

  public static AxisMapping Instance
  {
    get
    {
      if (_instance == null)
        _instance = Resources.Load<AxisMapping>("AxisMapping"); // looks for Resources/AxisMapping.asset
      return _instance;
    }
  }

  public IReadOnlyList<string> GetButtons(string axisName)
  {
    foreach (var entry in m_entries)
    {
      if (entry.m_axisName == axisName)
        return entry.m_buttons;
    }
    return new List<string>();
  }
}
