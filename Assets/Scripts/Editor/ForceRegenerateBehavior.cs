#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public class ForceRegenerateBehavior
{
    [MenuItem("Tools/Behavior/Force Regenerate All Nodes")]
    static void RegenerateNodes()
    {
        AssetDatabase.Refresh();
        Debug.Log("Behavior nodes regenerated!");
    }
}
#endif