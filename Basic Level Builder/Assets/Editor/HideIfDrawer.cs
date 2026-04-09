using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(HideIfAttribute))]
public class HideIfDrawer : PropertyDrawer
{
  public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
  {
    HideIfAttribute hideIf = (HideIfAttribute)attribute;
    SerializedProperty boolProp = property.serializedObject.FindProperty(hideIf.boolField);

    if (boolProp != null && boolProp.propertyType == SerializedPropertyType.Boolean)
    {
      bool condition = boolProp.boolValue;
      if (condition == hideIf.hideWhenTrue)
        return; // don’t draw
    }

    EditorGUI.PropertyField(position, property, label, true);
  }

  public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
  {
    HideIfAttribute hideIf = (HideIfAttribute)attribute;
    SerializedProperty boolProp = property.serializedObject.FindProperty(hideIf.boolField);

    if (boolProp != null && boolProp.propertyType == SerializedPropertyType.Boolean)
    {
      bool condition = boolProp.boolValue;
      if (condition == hideIf.hideWhenTrue)
        return 0; // no space
    }

    return EditorGUI.GetPropertyHeight(property, label, true);
  }
}
