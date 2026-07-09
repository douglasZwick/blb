using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class AdaptablePreferredScrollBox : LayoutElement, ILayoutElement {

#if UNITY_EDITOR
    [CustomEditor(typeof(AdaptablePreferredScrollBox))]
    public class Drawer : Editor {
        public override void OnInspectorGUI() {

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Preferred Height");
            var prop = serializedObject.FindProperty("_usePreferredHeight");
            EditorGUILayout.PropertyField(prop, new GUIContent(""), GUILayout.Width(EditorGUIUtility.singleLineHeight));
            if(prop.boolValue) {
                var tmp = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 35;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_preferredHeightMax"), new GUIContent("Max"));
                EditorGUIUtility.labelWidth = tmp;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Preferred Width");
            prop = serializedObject.FindProperty("_usePreferredWidth");
            EditorGUILayout.PropertyField(prop, new GUIContent(""), GUILayout.Width(EditorGUIUtility.singleLineHeight + 2));
            if(prop.boolValue) {
                var tmp = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 35;
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_preferredWidthMax"), new GUIContent("Max"));
                EditorGUIUtility.labelWidth = tmp;
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_contentToGrowWith"));
            if(EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
#endif

    [SerializeField] private RectTransform _contentToGrowWith;

    [SerializeField] private bool _usePreferredHeight;
    [SerializeField] private float _preferredHeightMax;
    [SerializeField] private bool _usePreferredWidth;
    [SerializeField] private float _preferredWidthMax;

    private float _preferredHeight;
    private float _preferredWidth;

    public override float preferredHeight => _preferredHeight;
    public override float preferredWidth => _preferredWidth;

    public override void CalculateLayoutInputVertical() {
        if(_contentToGrowWith == null) {
            return;
        }
        if(_usePreferredHeight) {
            var height = LayoutUtility.GetPreferredHeight(_contentToGrowWith);
            _preferredHeight = _preferredHeightMax > height ? height : _preferredHeightMax;
        } else {
            _preferredHeight = -1;
        }
    }

    public override void CalculateLayoutInputHorizontal() {
        if(_contentToGrowWith == null) {
            return;
        }
        if(_usePreferredWidth) {
            var width = LayoutUtility.GetPreferredWidth(_contentToGrowWith);
            _preferredWidth = _preferredWidthMax > width ? width : _preferredWidthMax;
        } else {
            _preferredWidth = -1;
        }
    }
}