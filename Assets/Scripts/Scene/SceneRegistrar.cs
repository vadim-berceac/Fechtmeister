
using UnityEngine;

public class SceneRegistrar
{
    private readonly CharacterCore _characterCore;
    private readonly SceneCharacterContainer _sceneCharacterContainer;
    
    public SceneRegistrar(CharacterCore characterCore, SceneCharacterContainer sceneCharacterContainer)
    {
        _characterCore = characterCore;
        _sceneCharacterContainer = sceneCharacterContainer;
    }

    public void Add()
    {
        _sceneCharacterContainer.Characters.Add(_characterCore);
        Debug.LogWarning($"Added in SceneCharacterContainer '{_characterCore.name}'");
    }

    public void Remove()
    {
        _sceneCharacterContainer.Characters.Remove(_characterCore);
    }
}
