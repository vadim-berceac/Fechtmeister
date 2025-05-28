using UnityEngine;
using Zenject;

public class CharacterCore : MonoBehaviour
{
    [field: SerializeField] public LocomotionSettings LocomotionSettings { get; set; }
    
    private SceneCharacterContainer _sceneCharacterContainer;
    private SceneCamera _sceneCamera;
    public CharacterInputHandler CharacterInputHandler { get; private set; }

    private ICharacterInputSet _inputByPlayer;
    
    //State Machine
    public StatesContainer StatesContainer { get; private set; }
    public State CurrentState { get; private set; }

    [Inject]
    private void Construct(SceneCamera sceneCamera, SceneCharacterContainer sceneCharacterContainer, PlayerInput playerInput, StatesContainer statesContainer)
    {
        _sceneCamera = sceneCamera;
        _sceneCharacterContainer = sceneCharacterContainer;
        _inputByPlayer = playerInput;
        StatesContainer = statesContainer;
        CharacterInputHandler = new CharacterInputHandler();
        
        CurrentState = StatesContainer.IdleState;
    }

    public void Select(bool value)
    {
        if (value)
        {
            _sceneCamera.SetTarget(transform);
            CharacterInputHandler.SetupInputSet(_inputByPlayer);
            return;
        }
        _sceneCamera.SetTarget(null);
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
    [field: SerializeField] public AnimationCurve SpeedCurve { get; private set; }
}
