using Unity.Behavior;
using UnityEngine;

public class AimTargeting : ManagedUpdatableObject
{
    [SerializeField] private BehaviorGraphAgent agent;
    [SerializeField] private CharacterCore characterCore;
    [SerializeField] private PlayableGraphCore graphCore;
    [SerializeField] private CharacterInfoComponent characterInfo;

    [Header("LookAt Bones")]
    [SerializeField] private HumanBodyBones[] lookAtBones = new HumanBodyBones[]
    {
        HumanBodyBones.Spine,
        HumanBodyBones.Chest,
        HumanBodyBones.UpperChest
    };

    [SerializeField] private float[] boneWeightMultipliers = new float[]
    {
        0.3f,
        0.5f,
        1f
    };

    [SerializeField, Tooltip("Transition duration in seconds")]
    private float transitionDuration = 0.3f;

    private BlackboardVariable<HealthComponent> _targetHealthBlackBoard;
    private HealthComponent _targetHealth;
    
    private float[] _currentBoneWeights;
    private float[] _targetBoneWeights;

    private void Start()
    {
        _currentBoneWeights = new float[lookAtBones.Length];
        _targetBoneWeights = new float[lookAtBones.Length];
        
        var blackboard = agent.BlackboardReference;
        
        if (blackboard.GetVariable("CurrentTarget", out _targetHealthBlackBoard))
        {
            _targetHealth = _targetHealthBlackBoard.Value;
            _targetHealthBlackBoard.OnValueChanged += OnTargetChanged;
        }
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
        for (var i = 0; i < lookAtBones.Length && i < boneWeightMultipliers.Length; i++)
        {
            _targetBoneWeights[i] = desiredWeight * boneWeightMultipliers[i];
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
        
        for (var i = 0; i < lookAtBones.Length && i < _currentBoneWeights.Length; i++)
        {
            graphCore.SetLookAtBoneWeight(lookAtBones[i], _currentBoneWeights[i]);
        }
    }
}