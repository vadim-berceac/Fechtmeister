using UnityEngine;
using Zenject;

public class SceneCharacterContainerInstaller : MonoInstaller
{
    [SerializeField] private GameObject characterContainerPrefab;
    public override void InstallBindings()
    {
        Container.Bind<SceneCharacterContainer>().FromComponentInNewPrefab(characterContainerPrefab).AsSingle().NonLazy();
    }
}