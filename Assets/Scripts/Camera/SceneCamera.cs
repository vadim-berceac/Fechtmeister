using System;
using Unity.Cinemachine;
using UnityEngine;
using Zenject;

public class SceneCamera : MonoBehaviour, IInputHandler
{
    [field: SerializeField] public SceneCameraData SceneCameraData { get; private set; }
    private PlayerInput _playerInput;
    private Transform _trackingTarget;
    public Transform Target { get; private set; }
    private float _targetYaw;
    private float _targetPitch;
    public IInputSet InputSet { get; private set; }
    public bool HasTarget { get; private set; }
    public Action OnTargetChanged;

    [Inject]
    private void Construct(PlayerInput playerInput)
    {
        _playerInput = playerInput;
    }
    
    private void Awake()
    {
        if (_trackingTarget == null)
        {
            var trackingObject = new GameObject("Tracking Object");
            _trackingTarget = trackingObject.transform;
        }
        SceneCameraData.CharacterCameraController.Target.TrackingTarget = _trackingTarget;
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
        if (Target == null)
        {
            return;
        }
        _trackingTarget.transform.position = Target.transform.position;
    }

    private void Rotate(Vector2 value)
    {
        if (!SceneCameraData.CharacterCamera.gameObject.activeInHierarchy)
        {
            return;
        }
        
        if (value.sqrMagnitude >= SceneCameraData.Threshold)
        {
            _targetYaw += value.x * SceneCameraData.RotationCoefficient;
            _targetPitch += value.y * SceneCameraData.RotationCoefficient;
        }
        _targetYaw = _targetYaw.ClampAngle(float.MinValue, float.MaxValue);
        _targetPitch = _targetPitch.ClampAngle(SceneCameraData.BottomClamp, SceneCameraData.TopClamp);
        SceneCameraData.CharacterCameraController.Target.TrackingTarget.transform.localRotation 
            = Quaternion.Euler(_targetPitch, _targetYaw, 0.0f);
    }

    public void SetTarget(Transform target)
    {
        if (target == null)
        {
            HasTarget = false;
            Target = null;
            SceneCameraData.SceneCamera.gameObject.SetActive(true);
            SceneCameraData.CharacterCamera.gameObject.SetActive(false);
            OnTargetChanged?.Invoke();
            return;
        }
        HasTarget = true;
        Target = target;
        SceneCameraData.SceneCamera.gameObject.SetActive(false);
        SceneCameraData.CharacterCamera.gameObject.SetActive(true);
        OnTargetChanged?.Invoke();
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
    [field: SerializeField] public Camera MainCameraCam { get; private set; }
    
    [field: Header("Character Camera Settings")]
    [field: SerializeField] public CinemachineCamera CharacterCameraController { get; private set; }
    [field: SerializeField] public float TopClamp { get; private set; }
    [field: SerializeField] public float BottomClamp { get; private set; }
    [field: SerializeField] public float RotationCoefficient { get; private set; }
    [field: SerializeField] public float Threshold { get; private set; }
}
