using System;
using UnityEngine;
using Zenject;

public class CharacterCore : MonoBehaviour
{
    [field: SerializeField] private CameraTargetSettings CameraTargetSettings { get; set; }
    public CameraTarget CameraTarget { get; private set; }
    public SceneRegistrar SceneRegistrar { get; private set; }
    
    private SceneCharacterContainer _sceneCharacterContainer;
    private SceneCamera _sceneCamera;

    [Inject]
    private void Construct(SceneCamera sceneCamera, SceneCharacterContainer sceneCharacterContainer)
    {
        _sceneCamera = sceneCamera;
        _sceneCharacterContainer = sceneCharacterContainer;
    }

    private void Awake()
    {
        Initialize();
    }

    private void OnEnable()
    {
        SceneRegistrar.Add();
    }

    private void Initialize()
    {
        CameraTarget = new CameraTarget(CameraTargetSettings, _sceneCamera);
        SceneRegistrar = new SceneRegistrar(this, _sceneCharacterContainer);
    }

    private void OnDisable()
    {
        SceneRegistrar.Remove();
    }
}
