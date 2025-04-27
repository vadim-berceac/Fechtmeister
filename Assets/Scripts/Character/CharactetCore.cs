using UnityEngine;
using Zenject;

public class CharacterCore : MonoBehaviour
{
    [field: SerializeField] private LocoMotionSettings LocoMotionSettings { get; set; }
    [field: SerializeField] private CameraTargetSettings CameraTargetSettings { get; set; }

    private LocoMotion _locoMotion;
    private CameraTarget _cameraTarget;
    private SceneRegistrar _sceneRegistrar;
    
    private SceneCharacterContainer _sceneCharacterContainer;
    private SceneCamera _sceneCamera;

    private ICharacterInputSet _inputByPlayer;

    [Inject]
    private void Construct(SceneCamera sceneCamera, SceneCharacterContainer sceneCharacterContainer, PlayerInput playerInput)
    {
        _sceneCamera = sceneCamera;
        _sceneCharacterContainer = sceneCharacterContainer;
        _inputByPlayer = playerInput;
    }

    public void Select(bool value)
    {
        if (value)
        {
            _cameraTarget.SetTarget();
            _locoMotion.SetupInputSet(_inputByPlayer);
            //playerinput переключить на этого персонажа
            return;
        }
        _cameraTarget.ResetTarget();
        _locoMotion.SetupInputSet(null);
        //playerinput отключить от этого персонажа
    }

    private void Awake()
    {
        Initialize();
    }

    private void OnEnable()
    {
        _sceneRegistrar.Add();
    }

    private void Initialize()
    {
        _locoMotion = new LocoMotion(LocoMotionSettings);
        _cameraTarget = new CameraTarget(CameraTargetSettings, _sceneCamera);
        _sceneRegistrar = new SceneRegistrar(this, _sceneCharacterContainer);
    }

    private void OnDisable()
    {
        _sceneRegistrar.Remove();
    }
}
