using System;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(CharacterPresetLoader))]
public class CharacterCore : ManagedUpdatableObject
{
    [field: SerializeField] public bool IsAI { get; private set; }
    [field: SerializeField] public Transform DamagedObject { get; private set; }
    [field: SerializeField] public PlayableGraphCore GraphCore { get; private set; }
    [field: SerializeField] public LocomotionSettings LocomotionSettings { get; set; }
    [field: SerializeField] public GravitySettings GravitySettings { get; set; }
    [field: SerializeField] public TargetingSettings TargetingSettings { get; set; }
    [field: SerializeField] public LedgeDetectionSettings LedgeDetectionSettings { get; set; }
    [field: SerializeField] public NavMeshCharacterInput NavMeshCharacterInput { get; private set; }
    
    public SceneCamera SceneCamera { get; private set; }
    public CharacterInputHandler CharacterInputHandler { get; private set; }
    public Transform CashedTransform { get; private set; }

    public ICharacterInputSet InputByPlayer { get; private set; }
    
    //State Machine
    public StatesSet StatesSet { get; private set; }
    public State CurrentState { get; private set; }
    public State CurrentSubState { get; private set; }
    public Counter AttackCounter { get; private set; }
    public CurrentSpeed CurrentSpeed { get; private set; }
    public StateTimer StateTimer { get; private set; }
    public Action OnStateChanged;
    
    public CharacterGravity Gravity { get; private set; }
    
    //creating
    public SceneCharacterContainer SceneCharacterContainer { get; private set; }
    public CharacterColliderSizer CharacterColliderSizer { get; private set; }
    public CharacterPresetLoader PresetLoader { get; private set; }
    public CharacterSkinHandler SkinHandler { get; private set; }
    public CharacterTargetingSystem TargetingSystem { get; private set; }
    public LedgeDetection LedgeDetection { get; private set; }
    public Inventory Inventory { get; private set; }
    public IDamageable Health { get; private set; }

    [Inject]
    private void Construct(SceneCamera sceneCamera, SceneCharacterContainer sceneCharacterContainer,
        PlayerInput playerInput, StatesContainer statesContainer)
    {
        SceneCamera = sceneCamera;
        SceneCharacterContainer = sceneCharacterContainer;
        CashedTransform = transform;
        CharacterInputHandler = new CharacterInputHandler(LocomotionSettings.InputSmoothingSpeed);
        
        if (!IsAI)
        {
            InputByPlayer = playerInput;
        }
        else
        {
            InputByPlayer = NavMeshCharacterInput;
            CharacterInputHandler.SetupInputSet(InputByPlayer);
        }
        
        Gravity = new CharacterGravity();
        
        PresetLoader = GetComponent<CharacterPresetLoader>();
        StatesSet = statesContainer.GetStateSet(PresetLoader.CharacterPersonalityData.StateMachineType);
        CharacterColliderSizer = new CharacterColliderSizer(PresetLoader.CharacterPersonalityData.CharacterSkinDataSettings.PrimarySkin,
            LocomotionSettings.CharacterCollider, LocomotionSettings.CharacterController);
        SkinHandler = new CharacterSkinHandler(CashedTransform, PresetLoader.CharacterPersonalityData);
        TargetingSystem = new CharacterTargetingSystem(TargetingSettings.ItemTargeting);
        LedgeDetection = new LedgeDetection(LedgeDetectionSettings);
        AttackCounter = new Counter();
        StateTimer = new StateTimer();
        CurrentSpeed = new CurrentSpeed(CashedTransform);
        
        Inventory = new Inventory(this, PresetLoader, 3);
        Health = GetComponent<IDamageable>();
        Health.Initialize(PresetLoader.CharacterPersonalityData.HealthDataSettings.MaxHealth, 
            PresetLoader.CharacterPersonalityData.HealthDataSettings.CurrentHealthPercentage, 
            PresetLoader.CharacterPersonalityData.HealthDataSettings.HitReactionTriggerValuePercentage, DamagedObject,
            PresetLoader.CharacterPersonalityData.ResistanceSettings);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        CurrentState = StatesSet.GetStartState();
        CurrentState.EnterState(this);
        CurrentSubState = StatesSet.GetStartSubState();
        CurrentSubState.EnterState(this);
        
        SceneCharacterContainer.Add(this, LocomotionSettings.CharacterCollider);
    }

    public void SetState(State state)
    {
        CurrentState?.ExitState(this);
        CurrentState = state;
        CurrentState?.EnterState(this);
    }

    public void SetSubState(State state)
    {
        CurrentSubState?.ExitState(this);
        CurrentSubState = state;
        CurrentSubState?.EnterState(this);
    }

    public override void OnManagedUpdate()
    {
        CharacterInputHandler.SmoothInput(Time.deltaTime);
        CurrentState?.UpdateState(this);
        CurrentSubState?.UpdateState(this);
        CurrentSpeed.OnUpdate();
    }

    public override void OnManagedFixedUpdate()
    {
        CurrentState?.FixedUpdateState(this);
        CurrentSubState?.FixedUpdateState(this);
    }
    
    protected override void OnDisable()
    {
        base.OnDisable();
        SceneCharacterContainer.Remove(this);
        Inventory.Destroy();
    }
}
