using UnityEngine;
using Zenject;

public class CharacterCore : MonoBehaviour
{
    [field: SerializeField] private CameraTargetSettings CameraTargetSettings { get; set; }

    private CameraTarget _cameraTarget;
    private SceneRegistrar _sceneRegistrar;
    
    private SceneCharacterContainer _sceneCharacterContainer;
    private SceneCamera _sceneCamera;

    [Inject]
    private void Construct(SceneCamera sceneCamera, SceneCharacterContainer sceneCharacterContainer)
    {
        _sceneCamera = sceneCamera;
        _sceneCharacterContainer = sceneCharacterContainer;
    }

    public void Select(bool value)
    {
        if (value)
        {
            _cameraTarget.SetTarget();
            //playerinput переключить на этого персонажа
            return;
        }
        _cameraTarget.ResetTarget();
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
        _cameraTarget = new CameraTarget(CameraTargetSettings, _sceneCamera);
        _sceneRegistrar = new SceneRegistrar(this, _sceneCharacterContainer);
    }

    private void OnDisable()
    {
        _sceneRegistrar.Remove();
    }
}
