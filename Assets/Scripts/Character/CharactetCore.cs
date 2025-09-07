using System;
using Unity.VisualScripting;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(CharacterPresetLoader))]
public class CharacterCore : MonoBehaviour
{
    [field: SerializeField] public LocomotionSettings LocomotionSettings { get; set; }
    [field: SerializeField] public GravitySettings GravitySettings { get; set; }
    [field: SerializeField] public TargetingSettings TargetingSettings { get; set; }
    
    public SceneCamera SceneCamera { get; private set; }
    public CharacterInputHandler CharacterInputHandler { get; private set; }
    public Transform CashedTransform { get; private set; }

    public ICharacterInputSet InputByPlayer { get; private set; }
    
    //State Machine
    public AnimationLayerWeightTransition AnimationLayerWeightTransition { get; private set; }
    public StatesContainer StatesContainer { get; private set; }
    public State CurrentState { get; private set; }
    public Counter AttackCounter { get; private set; }
    public Action OnStateChanged;
    
    public CharacterGravity Gravity { get; private set; }
    
    //creating
    private SceneCharacterContainer _sceneCharacterContainer;
    public CharacterPresetLoader PresetLoader { get; private set; }
    public CharacterSkinHandler SkinHandler { get; private set; }
    public CharacterBonesContainer BonesContainer { get; private set; }
    public CharacterTargetingSystem TargetingSystem { get; private set; }
    public Inventory Inventory { get; private set; }
    public CharacterHealth Health { get; private set; }

    [Inject]
    private void Construct(SceneCamera sceneCamera, SceneCharacterContainer sceneCharacterContainer, PlayerInput playerInput, StatesContainer statesContainer)
    {
        SceneCamera = sceneCamera;
        _sceneCharacterContainer = sceneCharacterContainer;
        InputByPlayer = playerInput;
        StatesContainer = statesContainer;
        CashedTransform = transform;
        CharacterInputHandler = new CharacterInputHandler(LocomotionSettings.InputSmoothingSpeed);
        Gravity = new CharacterGravity();
        
        PresetLoader = GetComponent<CharacterPresetLoader>();
        SkinHandler = new CharacterSkinHandler(CashedTransform, PresetLoader.CharacterPersonalityData);
        BonesContainer = new CharacterBonesContainer(CashedTransform);
        AnimationLayerWeightTransition = new AnimationLayerWeightTransition(LocomotionSettings.Animator);
        TargetingSystem = new CharacterTargetingSystem(TargetingSettings.ItemTargeting, TargetingSettings.CharacterTargeting);
        
        CurrentState = StatesContainer.IdleState;
        CurrentState.EnterState(this);
        AttackCounter = new Counter();
        
        Inventory = new Inventory(this, PresetLoader, 3);
        Health = new CharacterHealth(PresetLoader.CharacterPersonalityData.HealthDataSettings.MaxHealth, 
            PresetLoader.CharacterPersonalityData.HealthDataSettings.CurrentHealthPercentage, PresetLoader.CharacterPersonalityData.HealthDataSettings.HitReactionTriggerValuePercentage);
    }

    public void SetState(State state)
    {
        CurrentState.ExitState(this);
        CurrentState = state;
        CurrentState.EnterState(this);
    }

    private void Update()
    {
        CharacterInputHandler.SmoothInput(Time.deltaTime);
        AnimationLayerWeightTransition.UpdateTransition();
        CurrentState.UpdateState(this);
    }

    private void FixedUpdate()
    {
        CurrentState.FixedUpdateState(this);
    }

    private void OnEnable()
    {
        _sceneCharacterContainer.Add(this);
    }

    private void OnDisable()
    {
        _sceneCharacterContainer.Remove(this);
        Inventory.Destroy();
    }
}
