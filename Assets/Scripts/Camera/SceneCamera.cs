using Unity.Cinemachine;
using UnityEngine;
using Zenject;

public class SceneCamera : MonoBehaviour, IInputHandler
{
    [field: SerializeField] private SceneCameraData sceneCameraData;
    private PlayerInput _playerInput;
    private Transform _trackingTarget;
    private Transform _target;
    private float _targetYaw;
    private float _targetPitch;
    public IInputSet InputSet { get; private set; }
    public bool HasTarget { get; private set; }

    [Inject]
    private void Construct(PlayerInput playerInput)
    {
        _playerInput = playerInput;
    }
    
    private void Awake()
    {
#if UNITY_EDITOR
        Debug.LogWarning("SceneCamera: Awake");
#endif
        if (_trackingTarget == null)
        {
            var trackingObject = new GameObject("Tracking Object");
            _trackingTarget = trackingObject.transform;
        }
        sceneCameraData.CharacterCameraController.Target.TrackingTarget = _trackingTarget;
    }

    private void OnEnable()
    {
        SetupInputSet(_playerInput);
        _playerInput.OnLook += Rotate;
    }

    private void LateUpdate()
    {
        UpdateTrackingTarget();
    }

    private void UpdateTrackingTarget()
    {
        if (_target == null)
        {
            return;
        }
        _trackingTarget.transform.position = _target.transform.position;
    }

    private void Rotate(Vector2 value)
    {
        if (!sceneCameraData.CharacterCamera.gameObject.activeInHierarchy)
        {
            return;
        }
        
        if (value.sqrMagnitude >= sceneCameraData.Threshold)
        {
            _targetYaw += value.x * sceneCameraData.RotationCoefficient;
            _targetPitch += value.y * sceneCameraData.RotationCoefficient;
        }
        _targetYaw = _targetYaw.ClampAngle(float.MinValue, float.MaxValue);
        _targetPitch = _targetPitch.ClampAngle(sceneCameraData.BottomClamp, sceneCameraData.TopClamp);
        sceneCameraData.CharacterCameraController.Target.TrackingTarget.transform.localRotation 
            = Quaternion.Euler(_targetPitch, _targetYaw, 0.0f);
    }

    public void SetTarget(Transform target)
    {
        if (target == null)
        {
            HasTarget = false;
            _target = null;
            sceneCameraData.SceneCamera.gameObject.SetActive(true);
            sceneCameraData.CharacterCamera.gameObject.SetActive(false);
            return;
        }
        HasTarget = true;
        _target = target;
        sceneCameraData.SceneCamera.gameObject.SetActive(false);
        sceneCameraData.CharacterCamera.gameObject.SetActive(true);
    }

    public void SetupInputSet(IInputSet inputSet)
    {
        InputSet = inputSet;
    }

    private void OnDisable()
    {
        _playerInput.OnLook -= Rotate;
        SetupInputSet(null);
    }
}

[System.Serializable]
public struct SceneCameraData
{
    [field: Header("Cameras")]
    [field: SerializeField] public Transform MainCamera { get; private set; }
    [field: SerializeField] public Transform CharacterCamera { get; private set; }
    [field: SerializeField] public Transform SceneCamera { get; private set; }
    
    [field: Header("Character Camera Settings")]
    [field: SerializeField] public CinemachineCamera CharacterCameraController { get; private set; }
    [field: SerializeField] public float TopClamp { get; private set; }
    [field: SerializeField] public float BottomClamp { get; private set; }
    [field: SerializeField] public float RotationCoefficient { get; private set; }
    [field: SerializeField] public float Threshold { get; private set; }
}
