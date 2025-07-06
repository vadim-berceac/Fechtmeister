using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PickupItem))]
public class PickupItemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var itemDataProperty = serializedObject.FindProperty("itemData");
        var newItemData = EditorGUILayout.ObjectField(
            "Item Data",
            itemDataProperty.objectReferenceValue,
            typeof(ScriptableObject),
            false
        ) as ScriptableObject;
        
        if (newItemData != null && newItemData is IItemData)
        {
            itemDataProperty.objectReferenceValue = newItemData;
        }
        else if (newItemData != null)
        {
            Debug.LogWarning("Selected object must implement IItemData.");
            itemDataProperty.objectReferenceValue = null;
        }
        else
        {
            itemDataProperty.objectReferenceValue = null;
        }

        serializedObject.ApplyModifiedProperties();
    }
}