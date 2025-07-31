using UnityEngine;
using Zenject;

public class SceneContainersInstaller : MonoInstaller
{
    [SerializeField] private GameObject sceneContainerPrefab;
    public override void InstallBindings()
    {
        Container.Bind<SceneCharacterContainer>().FromComponentInNewPrefab(sceneContainerPrefab).AsSingle().NonLazy();
        Container.Bind<GameWindowContainer>().FromComponentInHierarchy().AsSingle().NonLazy();
    }
}