using UnityEngine;
using Zenject;

public class BillboardUI : MonoBehaviour
{
    private Transform _cameraTransform;
    private Transform _cashedTransform;
    private Transform _parentTransform;

    [Inject]
    private void Construct(SceneCamera sceneCamera)
    {
        _cameraTransform = sceneCamera.SceneCameraData.CharacterCamera;
        _cashedTransform = transform;
        _parentTransform = _cashedTransform.parent;
    }


    private void LateUpdate()
    {
        var targetWorldRotation = _cameraTransform.rotation;
    
        if (_parentTransform != null)
        {
            _cashedTransform.localRotation = Quaternion.Inverse(_parentTransform.rotation) * targetWorldRotation;
        }
        else
        {
            _cashedTransform.rotation = targetWorldRotation;
        }
    }
}