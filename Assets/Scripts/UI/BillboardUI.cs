using UnityEngine;
using Zenject;

public class BillboardUI : MonoBehaviour
{
     private Transform _cameraTransform;
     private Transform _cashedTransform;

     [Inject]
     private void Construct(SceneCamera sceneCamera)
     {
          _cameraTransform = sceneCamera.SceneCameraData.CharacterCamera;
     }

     private void OnEnable()
     {
          _cashedTransform = transform;
     }
     private void LateUpdate()
     {
         FollowCamera();
     }

     private void FollowCamera()
     {
          _cashedTransform.LookAt(_cashedTransform.position + _cameraTransform.rotation * Vector3.forward,
               _cameraTransform.rotation * Vector3.up);
     }
}
