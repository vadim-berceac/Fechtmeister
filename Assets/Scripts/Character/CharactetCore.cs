using UnityEngine;
using Zenject;

[RequireComponent(typeof(CharacterPresetLoader))]
public class CharacterCore : MonoBehaviour
{
    [field: SerializeField] public LocomotionSettings LocomotionSettings { get; set; }
    [field: SerializeField] public GravitySettings GravitySettings { get; set; }
    
    public SceneCamera SceneCamera { get; private set; }
    public CharacterInputHandler CharacterInputHandler { get; private set; }
    public Transform CashedTransform { get; private set; }

    private ICharacterInputSet _inputByPlayer;
    
    //State Machine
    public AnimationLayerWeightTransition AnimationLayerWeightTransition { get; private set; }
    public StatesContainer StatesContainer { get; private set; }
    public State CurrentState { get; private set; }
    
    public CharacterGravity Gravity { get; private set; }
    
    //creating
    private SceneCharacterContainer _sceneCharacterContainer;
    public CharacterPresetLoader PresetLoader { get; private set; }
    public CharacterSkinHandler SkinHandler { get; private set; }
    public CharacterBonesContainer BonesContainer { get; private set; }
    public Inventory Inventory { get; private set; }

    [Inject]
    private void Construct(SceneCamera sceneCamera, SceneCharacterContainer sceneCharacterContainer, PlayerInput playerInput, StatesContainer statesContainer)
    {
        SceneCamera = sceneCamera;
        _sceneCharacterContainer = sceneCharacterContainer;
        _inputByPlayer = playerInput;
        StatesContainer = statesContainer;
        CashedTransform = transform;
        CharacterInputHandler = new CharacterInputHandler(LocomotionSettings.InputSmoothingSpeed);
        Gravity = new CharacterGravity();
        
        PresetLoader = GetComponent<CharacterPresetLoader>();
        SkinHandler = new CharacterSkinHandler(CashedTransform, PresetLoader.CharacterPersonalityData.CharacterSkinData);
        BonesContainer = new CharacterBonesContainer(CashedTransform);
        AnimationLayerWeightTransition = new AnimationLayerWeightTransition(LocomotionSettings.Animator);
        
        CurrentState = StatesContainer.IdleState;
        CurrentState.EnterState(this);
        
        Inventory = new Inventory(BonesContainer, PresetLoader, 3);
    }

    public void Select(bool value)
    {
        if (value)
        {
            SceneCamera.SetTarget(transform);
            CharacterInputHandler.SetupInputSet(_inputByPlayer);
            return;
        }
        SceneCamera.SetTarget(null);
        CharacterInputHandler.SetupInputSet(null);
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
