using UnityEngine;
using Zenject;

[RequireComponent(typeof(CharacterPresetLoader))]
public class CharacterCore : MonoBehaviour
{
    [field: Header("Temp")]
    [field: SerializeField] public WeaponData TempWeaponData { get; set; } // переместить в инвентарь
    
    
    
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
    
    public CharacterGravity Gravity { get; private set; }
    
    //creating
    public CharacterPresetLoader PresetLoader { get; private set; }
    public CharacterSkinHandler SkinHandler { get; private set; }
    public CharacterBonesContainer BonesContainer { get; private set; }
    public WeaponSystem WeaponSystem { get; private set; }

    [Inject]
    private void Construct(SceneCamera sceneCamera, SceneCharacterContainer sceneCharacterContainer, PlayerInput playerInput, StatesContainer statesContainer)
    {
        SceneCamera = sceneCamera;
        _sceneCharacterContainer = sceneCharacterContainer;
        _inputByPlayer = playerInput;
        StatesContainer = statesContainer;
        CashedTransform = transform;
        CharacterInputHandler = new CharacterInputHandler();
        Gravity = new CharacterGravity();
        
        PresetLoader = GetComponent<CharacterPresetLoader>();
        SkinHandler = new CharacterSkinHandler(CashedTransform, PresetLoader.CharacterPersonalityData.CharacterSkinData);
        BonesContainer = new CharacterBonesContainer(CashedTransform);
        
        CurrentState = StatesContainer.IdleState;
        CurrentState.EnterState(this);
        
        WeaponSystem = new WeaponSystem(3, BonesContainer);
        WeaponSystem.Equip(TempWeaponData);
        WeaponSystem.SelectInstance(0);
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
    [field: Header("Grounding settings")]
    [field: SerializeField] public Vector3 GroundOffset { get; private set; }
    [field: SerializeField] public float CheckSphereRadius { get; private set; }
    
    [field: Header("Falling settings")]
    [field: SerializeField] public bool ImmuneToFallDamage { get; private set; }
    [field: SerializeField] public float FallDamageThreshold { get; private set; }
}
