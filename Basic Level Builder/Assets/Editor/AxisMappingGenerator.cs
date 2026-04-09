#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Globalization;
using System.Linq;

public static class AxisMappingGenerator
{
  [MenuItem("Tools/Generate Axis Mapping")]
  public static void Generate()
  {
    Object inputManager = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/InputManager.asset")[0];
    SerializedObject obj = new(inputManager);
    SerializedProperty axesProperty = obj.FindProperty("m_Axes");

    AxisMapping mapping = ScriptableObject.CreateInstance<AxisMapping>();

    for (int i = 0; i < axesProperty.arraySize; i++)
    {
      SerializedProperty axis = axesProperty.GetArrayElementAtIndex(i);
      SerializedProperty nameProp = axis.FindPropertyRelative("m_Name");
      SerializedProperty posButtonProp = axis.FindPropertyRelative("positiveButton");
      SerializedProperty altPosButtonProp = axis.FindPropertyRelative("altPositiveButton");

      // look for an existing entry with the same axisName
      AxisMappingEntry entry = mapping.m_entries.FirstOrDefault(e => e.m_axisName == nameProp.stringValue);
      if (entry == null)
      {
        entry = new AxisMappingEntry
        {
          m_axisName = nameProp.stringValue
        };
        mapping.m_entries.Add(entry);
      }

      // If the button exists and is not a numpad button (as we only want to print the number button)
      // Also blacklists "return" as "enter" is prefered to print as the return key 
      if (!string.IsNullOrEmpty(posButtonProp.stringValue) && posButtonProp.stringValue[0] != '[' && !posButtonProp.stringValue.Equals("return"))
      {
        string button = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(posButtonProp.stringValue.ToLower());
        if (!entry.m_buttons.Contains(button))
          entry.m_buttons.Add(button);
      }

      // If the button exists and is not a numpad button (as we only want to print the number button)
      // Also blacklists "return" as "enter" is prefered to print as the return key 
      if (!string.IsNullOrEmpty(altPosButtonProp.stringValue) && altPosButtonProp.stringValue[0] != '[' && !posButtonProp.stringValue.Equals("return"))
      {
        string button = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(altPosButtonProp.stringValue.ToLower());
        if (!entry.m_buttons.Contains(button))
          entry.m_buttons.Add(button);
      }
    }

    AssetDatabase.CreateAsset(mapping, "Assets/Resources/AxisMapping.asset");
    AssetDatabase.SaveAssets();

    Debug.Log("Axis mapping generated at Assets/Resources/AxisMapping.asset");
  }
}
#endif
