using UnityEngine;
using Zenject;

[RequireComponent(typeof(CharacterPresetLoader))]
public class CharacterCore : MonoBehaviour
{
    [field: Header("Temp")]
    [field: SerializeField] public int CurrentWeaponIndex { get; set; }
    [field: SerializeField] public LocomotionSettings LocomotionSettings { get; set; }
    [field: SerializeField] public GravitySettings GravitySettings { get; set; }
    
    private SceneCharacterContainer _sceneCharacterContainer;
    public SceneCamera SceneCamera { get; private set; }
    public CharacterInputHandler CharacterInputHandler { get; private set; }
    public Transform CashedTransform { get; private set; }

    private ICharacterInputSet _inputByPlayer;
    
    //State Machine
    public StatesContainer StatesContainer { get; private set; }
    public State CurrentState { get; private set; }
    
    //Gravity
    public float CurrentFallSpeed { get; private set; }
    public bool Grounded { get; private set; }
    
    //creating
    public CharacterPresetLoader PresetLoader { get; private set; }
    public CharacterSkinHandler SkinHandler { get; private set; }
    public CharacterBonesContainer BonesContainer { get; private set; }

    [Inject]
    private void Construct(SceneCamera sceneCamera, SceneCharacterContainer sceneCharacterContainer, PlayerInput playerInput, StatesContainer statesContainer)
    {
        SceneCamera = sceneCamera;
        _sceneCharacterContainer = sceneCharacterContainer;
        _inputByPlayer = playerInput;
        StatesContainer = statesContainer;
        CashedTransform = transform;
        CharacterInputHandler = new CharacterInputHandler();
        
        PresetLoader = GetComponent<CharacterPresetLoader>();
        SkinHandler = new CharacterSkinHandler(CashedTransform, PresetLoader.CharacterPersonalityData.CharacterSkinData);
        BonesContainer = new CharacterBonesContainer(CashedTransform);
        
        CurrentState = StatesContainer.IdleState;
        CurrentState.EnterState(this);
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

    public void SetFallSpeed(float speed)
    {
        CurrentFallSpeed = speed;
    }

    public void SetGrounded(bool value)
    {
        Grounded = value;
    }

    public void SetWeaponIndex(int index)
    {
        CurrentWeaponIndex = index;
    }

    private void Update()
    {
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
    }
}

[System.Serializable]
public struct LocomotionSettings
{
    [field: SerializeField] public Animator Animator { get; private set; }
    [field: SerializeField] public CharacterController CharacterController { get; private set; }
}

[System.Serializable]
public struct GravitySettings
{
    [field: SerializeField] public Vector3 GroundOffset { get; private set; }
    [field: SerializeField] public float CheckSphereRadius { get; private set; }
}
