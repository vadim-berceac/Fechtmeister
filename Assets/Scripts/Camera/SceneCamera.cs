using UnityEngine;

public class SceneCamera : MonoBehaviour
{
    [field: SerializeField] private SceneCameraData sceneCameraData;
    private void Awake()
    {
#if UNITY_EDITOR
        Debug.LogWarning("SceneCamera: Awake");
#endif
    }
}

[System.Serializable]
public struct SceneCameraData
{
    [field: SerializeField] public Transform MainCamera { get; private set; }
    [field: SerializeField] public Transform CharacterCamera { get; private set; }
    [field: SerializeField] public Transform SceneCamera { get; private set; }
}
