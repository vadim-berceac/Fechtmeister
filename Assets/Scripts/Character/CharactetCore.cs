using System;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(CharacterPresetLoader))]
public class CharacterCore : MonoBehaviour
{
    [field: SerializeField] public Transform DamagedObject { get; private set; }
    [field: SerializeField] public PlayableGraphCore GraphCore { get; private set; }
    [field: SerializeField] public LocomotionSettings LocomotionSettings { get; set; }
    [field: SerializeField] public GravitySettings GravitySettings { get; set; }
    [field: SerializeField] public TargetingSettings TargetingSettings { get; set; }
    [field: SerializeField] public LedgeDetectionSettings LedgeDetectionSettings { get; set; }
    
    public SceneCamera SceneCamera { get; private set; }
    public CharacterInputHandler CharacterInputHandler { get; private set; }
    public Transform CashedTransform { get; private set; }

    public ICharacterInputSet InputByPlayer { get; private set; }
    
    //State Machine
    public StatesContainer StatesContainer { get; private set; }
    public State CurrentState { get; private set; }
    public State CurrentSubState { get; private set; }
    public Counter AttackCounter { get; private set; }
    public CurrentSpeed CurrentSpeed { get; private set; }
    public ShootingSystem ShootingSystem { get; private set; }
    public StateTimer StateTimer { get; private set; }
    public Action OnStateChanged;
    
    public CharacterGravity Gravity { get; private set; }
    
    //creating
    public SceneCharacterContainer SceneCharacterContainer { get; private set; }
    public CharacterColliderSizer CharacterColliderSizer { get; private set; }
    public CharacterPresetLoader PresetLoader { get; private set; }
    public CharacterSkinHandler SkinHandler { get; private set; }
    public CharacterBonesContainer BonesContainer { get; private set; }
    public CharacterTargetingSystem TargetingSystem { get; private set; }
    public LedgeDetection LedgeDetection { get; private set; }
    public Inventory Inventory { get; private set; }
    public IDamageable Health { get; private set; }

    [Inject]
    private void Construct(SceneCamera sceneCamera, SceneCharacterContainer sceneCharacterContainer, PlayerInput playerInput, StatesContainer statesContainer)
    {
        SceneCamera = sceneCamera;
        SceneCharacterContainer = sceneCharacterContainer;
        InputByPlayer = playerInput;
        StatesContainer = statesContainer;
        CashedTransform = transform;
        CharacterInputHandler = new CharacterInputHandler(LocomotionSettings.InputSmoothingSpeed);
        Gravity = new CharacterGravity();
        
        PresetLoader = GetComponent<CharacterPresetLoader>();
        CharacterColliderSizer = new CharacterColliderSizer(PresetLoader.CharacterPersonalityData.CharacterSkinDataSettings.PrimarySkin,
            LocomotionSettings.CharacterCollider, LocomotionSettings.CharacterController);
        SkinHandler = new CharacterSkinHandler(CashedTransform, PresetLoader.CharacterPersonalityData);
        BonesContainer = new CharacterBonesContainer(CashedTransform);
        TargetingSystem = new CharacterTargetingSystem(TargetingSettings.ItemTargeting, TargetingSettings.CharacterTargeting);
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
        ShootingSystem = new ShootingSystem(LocomotionSettings.CharacterCollider, CashedTransform, Inventory);
    }

    private void Start()
    {
        CurrentState = StatesContainer.GetState("IdleState");
        CurrentState.EnterState(this);
        CurrentSubState = StatesContainer.GetState("DefaultSubState");
        CurrentSubState.EnterState(this);
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

    private void Update()
    {
        CharacterInputHandler.SmoothInput(Time.deltaTime);
        CurrentState.UpdateState(this);
        CurrentSubState?.UpdateState(this);
        CurrentSpeed.OnUpdate();
    }

    private void FixedUpdate()
    {
        CurrentState.FixedUpdateState(this);
        CurrentSubState?.FixedUpdateState(this);
    }

    private void OnEnable()
    {
        SceneCharacterContainer.Add(this, LocomotionSettings.CharacterCollider);
    }

    private void OnDisable()
    {
        SceneCharacterContainer.Remove(this);
        Inventory.Destroy();
    }
}
