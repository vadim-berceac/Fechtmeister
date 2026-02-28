using System;
using Unity.Cinemachine;
using UnityEngine;
using Zenject;

public class SceneCamera : MonoBehaviour, IInputHandler
{
    [field: SerializeField] public SceneCameraData SceneCameraData { get; private set; }
    private PlayerInput _playerInput;
    private Sight _sight;
    private Transform _trackingTarget;
    public Transform Target { get; private set; }
    private float _targetYaw;
    private float _targetPitch;
    public IInputSet InputSet { get; private set; }
    public bool HasTarget { get; private set; }
    public Action OnTargetChanged;

    private CameraMode _currentMode;
    private CameraSettings _currentSettings;

    private const int HighPriority = 10;
    private const int LowPriority = 0;

    [Inject]
    private void Construct(PlayerInput playerInput, Sight sight)
    {
        _playerInput = playerInput;
        _sight = sight;
        _currentSettings = SceneCameraData.SceneCameraSettings;

        if (_trackingTarget == null)
        {
            var trackingObject = new GameObject("Tracking Object");
            _trackingTarget = trackingObject.transform;
        }
        SceneCameraData.CharacterCameraController.Target.TrackingTarget = _trackingTarget;
        SceneCameraData.AimCameraController.Target.TrackingTarget = _trackingTarget;

        _sight.Disable();

        SceneCameraData.SceneCamera.gameObject.SetActive(true);
        SceneCameraData.CharacterCamera.gameObject.SetActive(true);
        SceneCameraData.AimCamera.gameObject.SetActive(true);

        SetCameraMode(CameraMode.SceneCamera);
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
        if (Target == null) return;
        _trackingTarget.transform.position = Target.transform.position + _currentSettings.TrackedTargetOffset;
    }

    private void Rotate(Vector2 value)
    {
        if (Target == null) return;

        if (value.sqrMagnitude >= _currentSettings.Threshold)
        {
            _targetYaw += value.x * _currentSettings.RotationCoefficient;
            _targetPitch += value.y * _currentSettings.RotationCoefficient;
        }
        _targetYaw = _targetYaw.ClampAngle(float.MinValue, float.MaxValue);
        _targetPitch = _targetPitch.ClampAngle(_currentSettings.BottomClamp, _currentSettings.TopClamp);
        _trackingTarget.transform.localRotation = Quaternion.Euler(_targetPitch, _targetYaw, 0.0f);
    }

    public void SetTarget(Transform target)
    {
        if (target == null)
        {
            HasTarget = false;
            Target = null;
            SetCameraMode(CameraMode.SceneCamera);
            OnTargetChanged?.Invoke();
            return;
        }
        HasTarget = true;
        Target = target;
        SetCameraMode(CameraMode.FollowCamera);
        OnTargetChanged?.Invoke();
    }

    public void SetCameraMode(CameraMode mode)
    {
        if (Target == null)
            mode = CameraMode.SceneCamera;

        var isFastTransition =
            (_currentMode == CameraMode.AimCamera && mode == CameraMode.FollowCamera) ||
            (_currentMode == CameraMode.FollowCamera && mode == CameraMode.AimCamera);

        SceneCameraData.Brain.DefaultBlend.Time = isFastTransition
            ? SceneCameraData.FastBlendTime
            : SceneCameraData.DefaultBlendTime;

        ResetPriorities();

        switch (mode)
        {
            case CameraMode.SceneCamera:
                SetPriorities(scene: HighPriority, character: LowPriority, aim: LowPriority);
                _currentSettings = SceneCameraData.SceneCameraSettings;
                _sight.Disable();
                break;

            case CameraMode.FollowCamera:
                SetPriorities(scene: LowPriority, character: HighPriority, aim: LowPriority);
                _currentSettings = SceneCameraData.FollowCameraSettings;
                _sight.Disable();
                break;

            case CameraMode.AimCamera:
                SetPriorities(scene: LowPriority, character: LowPriority, aim: HighPriority);
                _currentSettings = SceneCameraData.AimCameraSettings;
                _sight.Enable();
                break;

            default:
                Debug.LogWarning($"Неизвестный режим камеры: {mode}.");
                SetPriorities(scene: HighPriority, character: LowPriority, aim: LowPriority);
                _currentSettings = SceneCameraData.SceneCameraSettings;
                _sight.Disable();
                break;
        }

        _currentMode = mode;
    }

    private void ResetPriorities()
    {
        SceneCameraData.SceneCameraController.Priority = LowPriority;
        SceneCameraData.CharacterCameraController.Priority = LowPriority;
        SceneCameraData.AimCameraController.Priority = LowPriority;
    }

    private void SetPriorities(int scene, int character, int aim)
    {
        SceneCameraData.SceneCameraController.Priority = scene;
        SceneCameraData.CharacterCameraController.Priority = character;
        SceneCameraData.AimCameraController.Priority = aim;
    }

    public void SetupInputSet(IInputSet inputSet) => InputSet = inputSet;

    private void OnDisable()
    {
        _playerInput.OnLook -= Rotate;
        SetupInputSet(null);
    }
}

public enum CameraMode
{
    SceneCamera,
    FollowCamera,
    AimCamera
}

[System.Serializable]
public struct CameraSettings
{
    [field: SerializeField] public Vector3 TrackedTargetOffset { get; private set; }
    [field: SerializeField] public float TopClamp { get; private set; }
    [field: SerializeField] public float BottomClamp { get; private set; }
    [field: SerializeField] public float RotationCoefficient { get; private set; }
    [field: SerializeField] public float Threshold { get; private set; }
}

[System.Serializable]
public struct SceneCameraData
{
    [field: Header("Cameras")]
    [field: SerializeField] public Transform MainCamera { get; private set; }
    [field: SerializeField] public Transform CharacterCamera { get; private set; }
    [field: SerializeField] public Transform AimCamera { get; private set; }
    [field: SerializeField] public Transform SceneCamera { get; private set; }
    [field: SerializeField] public Camera MainCameraCam { get; private set; }
    [field: SerializeField] public CinemachineBrain Brain { get; private set; }
    [field: SerializeField] public float DefaultBlendTime { get; private set; }
    [field: SerializeField] public float FastBlendTime { get; private set; }

    [field: Header("Camera Controllers")]
    [field: SerializeField] public CinemachineCamera SceneCameraController { get; private set; }
    [field: SerializeField] public CinemachineCamera CharacterCameraController { get; private set; }
    [field: SerializeField] public CinemachineCamera AimCameraController { get; private set; }

    [field: Header("Scene Camera Settings")]
    [field: SerializeField] public CameraSettings SceneCameraSettings { get; private set; }

    [field: Header("Follow Camera Settings")]
    [field: SerializeField] public CameraSettings FollowCameraSettings { get; private set; }

    [field: Header("Aim Camera Settings")]
    [field: SerializeField] public CameraSettings AimCameraSettings { get; private set; }
}