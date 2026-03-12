using Unity.Behavior;
using UnityEngine;
using Zenject;

[System.Serializable]
public struct AimBoneConfig
{
    public HumanBodyBones bone;
    [Range(0f, 1f)]
    public float weightMultiplier;
    [Tooltip("Rotation offset in degrees (X, Y, Z)")]
    public Vector3 rotationOffset;
}

public class AimTargeting : ManagedUpdatableObject
{
    [SerializeField] private BehaviorGraphAgent agent;
    [SerializeField] private CharacterCore characterCore;
    [SerializeField] private PlayableGraphCore graphCore;
    [SerializeField] private CharacterInfoComponent characterInfo;

    [Header("Aim Bones Configuration")]
    [SerializeField] private AimBoneConfig[] aimBones = new AimBoneConfig[]
    {
        new AimBoneConfig { bone = HumanBodyBones.Spine, weightMultiplier = 0.3f, rotationOffset = Vector3.zero },
        new AimBoneConfig { bone = HumanBodyBones.Chest, weightMultiplier = 0.5f, rotationOffset = Vector3.zero },
        new AimBoneConfig { bone = HumanBodyBones.UpperChest, weightMultiplier = 1f, rotationOffset = Vector3.zero }
    };
    
    [SerializeField] private Vector3 botsAimOffset = new Vector3(1.5f, 1.5f, 0f);

    [SerializeField, Tooltip("Transition duration in seconds")]
    private float transitionDuration = 0.3f;

    private BlackboardVariable<HealthComponent> _targetHealthBlackBoard;
    private HealthComponent _targetHealth;
    private Transform _cameraTransform;
    private Transform _aimTargetTransform;

    private const float AimDistance = 10f;
    private const float AimSmoothSpeed = 15f;
    
    private float[] _currentBoneWeights;
    private float[] _targetBoneWeights;

    [Inject]
    private void Construct(SceneCamera sceneCamera)
    {
        _cameraTransform = sceneCamera.SceneCameraData.MainCamera;
        _aimTargetTransform = transform;
    }

    private void Start()
    {
        _currentBoneWeights = new float[aimBones.Length];
        _targetBoneWeights = new float[aimBones.Length];

        var blackboard = agent.BlackboardReference;
        if (blackboard.GetVariable("CurrentTarget", out _targetHealthBlackBoard))
        {
            _targetHealth = _targetHealthBlackBoard.Value;
            _targetHealthBlackBoard.OnValueChanged += OnTargetChanged;
        }

        if (graphCore.IsLookAtInitialized)
            ApplyRotationOffsets();
        else
            graphCore.OnLookAtInitialized += ApplyRotationOffsets;
    }

    public override void OnManagedUpdate()
    {
        UpdateTargetWeights();
        InterpolateWeights();
        ApplyBoneWeights();
        MoveRigTarget();
    }

    private void MoveRigTarget()
    {
        if (characterInfo.CharacterInfo.IsPlayerControlled)
        {
            PlayerAimMovement();
            return;
        }
        AIAimMovement();
    }

    private void AIAimMovement()
    {
        if (_targetHealth == null)
        {
            var neutralPosition = transform.position + transform.forward * AimDistance;
    
            _aimTargetTransform.position = Vector3.Lerp(
                _aimTargetTransform.position,
                neutralPosition, 
                AimSmoothSpeed * Time.deltaTime
            );
    
            _aimTargetTransform.rotation = Quaternion.Slerp(
                _aimTargetTransform.rotation,
                transform.rotation, 
                AimSmoothSpeed * Time.deltaTime
            );
    
            return;
        }

        var targetPosition = _targetHealth.transform.position + 
                             _targetHealth.transform.TransformDirection(botsAimOffset);

        _aimTargetTransform.position = Vector3.Lerp(
            _aimTargetTransform.position,
            targetPosition, 
            AimSmoothSpeed * Time.deltaTime
        );
    }

    private void PlayerAimMovement()
    {
        var targetPosition = _cameraTransform.position + _cameraTransform.forward * AimDistance;
    
        _aimTargetTransform.position = Vector3.Lerp(transform.position,
            targetPosition, AimSmoothSpeed * Time.deltaTime);
        _aimTargetTransform.rotation = Quaternion.Slerp(transform.rotation,
            _cameraTransform.rotation, AimSmoothSpeed * Time.deltaTime);
    }
    
    private void OnDestroy()
    {
        if (_targetHealthBlackBoard != null)
            _targetHealthBlackBoard.OnValueChanged -= OnTargetChanged;

        if (graphCore != null)
            graphCore.OnLookAtInitialized -= ApplyRotationOffsets;
    }

    private void OnTargetChanged()
    {
        _targetHealth = _targetHealthBlackBoard.Value;
    }

    private void UpdateTargetWeights()
    {
        if (graphCore == null) return;
        if (characterCore?.CurrentState == null) return;

        var desiredWeight = characterCore.Inventory.WeaponSystem.AnimationType == 7 
            ? characterCore.CurrentSubState.AimRigWeight
            : characterCore.CurrentState.AimRigWeight;
       
        if (_targetHealth == null && !characterInfo.CharacterInfo.IsPlayerControlled)
        {
            desiredWeight = 0f;
        }

        for (var i = 0; i < aimBones.Length; i++)
        {
            _targetBoneWeights[i] = desiredWeight * aimBones[i].weightMultiplier;
        }
    }

    private void InterpolateWeights()
    {
        var speed = 1f / transitionDuration;
        
        for (var i = 0; i < _currentBoneWeights.Length && i < _targetBoneWeights.Length; i++)
        {
            _currentBoneWeights[i] = Mathf.Lerp(
                _currentBoneWeights[i], 
                _targetBoneWeights[i], 
                speed * Time.deltaTime
            );
        }
    }

    private void ApplyBoneWeights()
    {
        if (graphCore == null) return;
        
        for (var i = 0; i < aimBones.Length && i < _currentBoneWeights.Length; i++)
        {
            graphCore.SetLookAtBoneWeight(aimBones[i].bone, _currentBoneWeights[i]);
        }
    }
    
    private void ApplyRotationOffsets()
    {
        if (graphCore == null) return;
        
        for (var i = 0; i < aimBones.Length; i++)
        {
            graphCore.SetLookAtBoneRotationOffset(aimBones[i].bone, aimBones[i].rotationOffset);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying && graphCore != null)
        {
            ApplyRotationOffsets();
        }
    }
#endif
}