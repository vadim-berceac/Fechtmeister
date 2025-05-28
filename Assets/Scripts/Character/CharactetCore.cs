using UnityEngine;
using Zenject;

public class CharacterCore : MonoBehaviour
{
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
            _sceneCamera.SetTarget(transform);
            return;
        }
        _sceneCamera.SetTarget(null);
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
