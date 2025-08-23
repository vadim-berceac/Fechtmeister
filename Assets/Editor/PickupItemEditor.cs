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
        
        var prefabProperty = serializedObject.FindProperty("namePlatePrefab");
        var newPrefab = EditorGUILayout.ObjectField(
            "NamePlatePrefab",
            prefabProperty.objectReferenceValue,
            typeof(GameObject),
            false
        ) as GameObject;

        if (newPrefab != null)
        {
            
            if (PrefabUtility.GetPrefabAssetType(newPrefab) != PrefabAssetType.NotAPrefab)
            {
                prefabProperty.objectReferenceValue = newPrefab;
            }
            else
            {
                Debug.LogWarning("Selected object must be a prefab.");
                prefabProperty.objectReferenceValue = null;
            }
        }
        else
        {
            prefabProperty.objectReferenceValue = null;
        }

        serializedObject.ApplyModifiedProperties();
    }
}