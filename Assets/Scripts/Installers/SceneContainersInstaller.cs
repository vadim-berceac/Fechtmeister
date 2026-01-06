using UnityEngine;
using Zenject;

public class SceneContainersInstaller : MonoInstaller
{
    [SerializeField] private GameObject sceneContainerPrefab;
    [SerializeField] private GameObject updateSystemPrefab;
    public override void InstallBindings()
    {
        Container.Bind<SceneCharacterContainer>().FromComponentInNewPrefab(sceneContainerPrefab).AsSingle().NonLazy();
        Container.Bind<GameWindowContainer>().FromComponentInHierarchy().AsSingle().NonLazy();
        Container.Bind<CentralizedUpdateSystem>().FromComponentInNewPrefab(updateSystemPrefab).AsSingle().NonLazy();
    }
}