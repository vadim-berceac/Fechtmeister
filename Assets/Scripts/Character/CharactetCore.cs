using System;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(CharacterPresetLoader))]
public class CharacterCore : ManagedUpdatableObject
{
    [field: SerializeField] public bool IsAI { get; private set; }
    [field: SerializeField] public float InputSmoothingSpeed { get; private set; } = 10f;
    [field: SerializeField] public Transform DamagedObject { get; private set; }
    [field: SerializeField] public GravitySettings GravitySettings { get; set; }
    [field: SerializeField] public LedgeDetectionSettings LedgeDetectionSettings { get; set; }
    public BehaviorNewInput BehaviorNewInput { get; private set; }
    public Transform AimTargetTransform { get; private set; }
    public PlayableGraphCore GraphCore { get; private set; }
    public CharacterController CharacterController { get; private set; }
    public CapsuleCollider CapsuleCollider { get; private set; }
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
    public Action<State> OnStateChanged;
    
    public CharacterGravity Gravity { get; private set; }
    
    //creating
    public SceneCharacterContainer SceneCharacterContainer { get; private set; }
    public CharacterColliderSizer CharacterColliderSizer { get; private set; }
    public CharacterPresetLoader PresetLoader { get; private set; }
    //public CharacterSkinHandler SkinHandler { get; private set; }
    public CharacterTargetingSystem TargetingSystem { get; private set; }
    public LedgeDetection LedgeDetection { get; private set; }
    public Inventory Inventory { get; private set; }
    public IDamageable Health { get; private set; }

    [Inject]
    private void Construct(SceneCamera sceneCamera, CharacterController characterController, ItemTargeting itemTargeting,
        SceneCharacterContainer sceneCharacterContainer, CapsuleCollider capsuleCollider, CharacterPresetLoader presetLoader,
        PlayerInput playerInput, StatesContainer statesContainer, Animator animator, PlayableGraphCore playableGraphCore,
        BehaviorNewInput behaviorNewInput, AimTargeting aimTargeting)
    {
        SceneCamera = sceneCamera;
        CharacterController = characterController;
        SceneCharacterContainer = sceneCharacterContainer;
        CapsuleCollider = capsuleCollider;
        CashedTransform = transform;
        GraphCore = playableGraphCore;
        BehaviorNewInput = behaviorNewInput;
        AimTargetTransform = aimTargeting.transform;
        CharacterInputHandler = new CharacterInputHandler(InputSmoothingSpeed);
        
        if (!IsAI)
        {
            InputByPlayer = playerInput;
        }
        else
        {
            InputByPlayer = BehaviorNewInput;
            BehaviorNewInput.Enable();
            CharacterInputHandler.SetupInputSet(InputByPlayer);
        }
        
        Gravity = new CharacterGravity();
        
        PresetLoader = presetLoader;
        StatesSet = statesContainer.GetStateSet(PresetLoader.CharacterPersonalityData.StateMachineType);
        CharacterColliderSizer = new CharacterColliderSizer(PresetLoader.CharacterPersonalityData.CharacterSkinDataSettings.PrimarySkin,
            CapsuleCollider, CharacterController);
        //SkinHandler = new CharacterSkinHandler(CashedTransform, PresetLoader.CharacterPersonalityData, animator);
        TargetingSystem = new CharacterTargetingSystem(itemTargeting);
        LedgeDetection = new LedgeDetection(LedgeDetectionSettings);
        AttackCounter = new Counter();
        StateTimer = new StateTimer();
        CurrentSpeed = new CurrentSpeed(CashedTransform);
        
        Inventory = new Inventory(this, animator, PresetLoader, 3);
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

    public override void OnManagedLateUpdate()
    {
        CurrentState?.LateUpdateState(this);
        CurrentSubState?.LateUpdateState(this);
    }
    
    protected override void OnDisable()
    {
        base.OnDisable();
        Inventory.Destroy();
    }
}
