using UnityEngine;
using Zenject;

public class CharacterCore : MonoBehaviour
{
    [field: SerializeField] private CameraTargetSettings CameraTargetSettings { get; set; }
   
    private CameraTarget _cameraTarget;
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
            return;
        }
        _cameraTarget.ResetTarget();
    }

    private void Awake()
    {
        Initialize();
    }

    private void OnEnable()
    {
        _sceneCharacterContainer.Add(this);
    }

    private void Initialize()
    {
        _cameraTarget = new CameraTarget(CameraTargetSettings, _sceneCamera);
    }

    private void OnDisable()
    {
        _sceneCharacterContainer.Remove(this);
    }
}
