using UnityEngine;
using UnityEditor;

namespace EditorTools
{
    [CustomEditor(typeof(AnimationBlendConfig))]
    public class AnimationBlendConfigEditor : Editor
    {
        private SerializedProperty _paramValueProp;
        private SerializedProperty _clipsProp;
        private bool[] _foldouts;

        private void OnEnable()
        {
            _paramValueProp = serializedObject.FindProperty("<ParamValue>k__BackingField");
            _clipsProp = serializedObject.FindProperty("<Clips>k__BackingField");
            InitializeFoldouts();
        }
        
        private void InitializeFoldouts()
        {
            if (_clipsProp != null)
            {
                _foldouts = new bool[_clipsProp.arraySize];
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_paramValueProp);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Clips", EditorStyles.boldLabel);
        
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Element"))
            {
                _clipsProp.arraySize++;
                InitializeFoldouts();
            }
            if (GUILayout.Button("Clear All") && _clipsProp.arraySize > 0)
            {
                if (EditorUtility.DisplayDialog("Clear All", "Are you sure you want to remove all elements?", "Yes", "No"))
                {
                    _clipsProp.arraySize = 0;
                    InitializeFoldouts();
                }
            }
            EditorGUILayout.EndHorizontal();
        
            EditorGUILayout.Space(5);
        
            if (_foldouts == null || _foldouts.Length != _clipsProp.arraySize)
            {
                InitializeFoldouts();
            }
            
            EditorGUI.indentLevel++;
        
            for (var i = 0; i < _clipsProp.arraySize; i++)
            {
                var element = _clipsProp.GetArrayElementAtIndex(i);
                var clipProp = element.FindPropertyRelative("<Clip>k__BackingField");
            
                var label = "missed";
                if (clipProp.objectReferenceValue != null)
                {
                    label = clipProp.objectReferenceValue.name;
                }

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
                EditorGUILayout.BeginHorizontal();
                _foldouts[i] = EditorGUILayout.Foldout(_foldouts[i], $"Element {i}: {label}", true);
            
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    _clipsProp.DeleteArrayElementAtIndex(i);
                    InitializeFoldouts();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    break;
                }
                EditorGUILayout.EndHorizontal();
            
                if (_foldouts[i])
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(clipProp, new GUIContent("Clip"));
                    EditorGUILayout.PropertyField(element.FindPropertyRelative("<ParamValue>k__BackingField"), new GUIContent("Param Value"));
                    EditorGUILayout.PropertyField(element.FindPropertyRelative("<ParamPosition>k__BackingField"), new GUIContent("Param Position"));
                    EditorGUILayout.PropertyField(element.FindPropertyRelative("<Speed>k__BackingField"), new GUIContent("Speed"));
                    EditorGUILayout.PropertyField(element.FindPropertyRelative("<ActionTime>k__BackingField"), new GUIContent("Action Time"));
                    EditorGUI.indentLevel--;
                }
            
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(3);
            }

            EditorGUI.indentLevel--;

            serializedObject.ApplyModifiedProperties();
        }
   }
}