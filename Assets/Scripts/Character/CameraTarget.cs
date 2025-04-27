using UnityEngine;

public class CameraTarget
{
    private readonly CameraTargetSettings _cameraTargetSettings;
    private readonly SceneCamera _sceneCamera;
    
    public CameraTarget(CameraTargetSettings cameraTargetSettings, SceneCamera sceneCamera)
    {
        _cameraTargetSettings = cameraTargetSettings;
        _sceneCamera = sceneCamera;
    }

    public void SetTarget()
    {
        if (_cameraTargetSettings.Target == null)
        {
            Debug.LogWarning("Camera Target in CameraTargetSettings is null");
            return;
        }
        _sceneCamera.SetTarget(_cameraTargetSettings.Target);
    }

    public void ResetTarget()
    {
        _sceneCamera.SetTarget(null);
    }
}

[System.Serializable]
public struct CameraTargetSettings
{
    [field: SerializeField] public Transform Target { get; private set; }
}
