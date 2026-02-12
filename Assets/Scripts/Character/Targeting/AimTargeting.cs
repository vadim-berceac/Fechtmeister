using Unity.Behavior;
using UnityEngine;

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

    [SerializeField, Tooltip("Transition duration in seconds")]
    private float transitionDuration = 0.3f;

    private BlackboardVariable<HealthComponent> _targetHealthBlackBoard;
    private HealthComponent _targetHealth;
    
    private float[] _currentBoneWeights;
    private float[] _targetBoneWeights;

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
        
        // Применяем начальные оффсеты
        ApplyRotationOffsets();
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
            PlayerMovement();
            return;
        }
        AIMovement();
    }

    private void AIMovement()
    {
        //двигаем цель для aim (transform этого объекта) к _targetHealth (если он есть) - без камеры
    }

    private void PlayerMovement()
    {
        Debug.Log(characterInfo.CharacterInfo.IsPlayerControlled);
        //двигаем цель для aim (transform этого объекта) с помощью камеры
    }
    
    private void OnDestroy()
    {
        if (_targetHealthBlackBoard != null)
        {
            _targetHealthBlackBoard.OnValueChanged -= OnTargetChanged;
        }
    }

    private void OnTargetChanged()
    {
        _targetHealth = _targetHealthBlackBoard.Value;
    }

    private void UpdateTargetWeights()
    {
        if (graphCore == null) return;
        if (characterCore?.CurrentState == null) return;

        var desiredWeight = characterCore.CurrentState.AimRigWeight;
       
        if (_targetHealth == null && !characterInfo.CharacterInfo.IsPlayerControlled)
        {
            desiredWeight = 0f;
        }

        // Обновляем целевые веса
        for (var i = 0; i < aimBones.Length; i++)
        {
            _targetBoneWeights[i] = desiredWeight * aimBones[i].weightMultiplier;
        }
    }

    private void InterpolateWeights()
    {
        float speed = 1f / transitionDuration;
        
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
    // Применяем оффсеты при изменении в инспекторе
    private void OnValidate()
    {
        if (Application.isPlaying && graphCore != null)
        {
            ApplyRotationOffsets();
        }
    }
#endif
}