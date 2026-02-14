using UnityEngine;
using Zenject;

public class SceneContainersInstaller : MonoInstaller
{
    [SerializeField] private GameObject sceneContainerPrefab;
    [SerializeField] private GameObject updateSystemPrefab;
    [SerializeField] private GameObject sceneInterfacePrefab;
    public override void InstallBindings()
    {
        Container.Bind<SceneCharacterContainer>().FromComponentInNewPrefab(sceneContainerPrefab).AsSingle().NonLazy();
        Container.Bind<GameWindowContainer>().FromComponentInHierarchy().AsSingle().NonLazy();
        Container.Bind<CentralizedUpdateSystem>().FromComponentInNewPrefab(updateSystemPrefab).AsSingle().NonLazy();
        Container.Bind<VisionSystem>().FromComponentInHierarchy().AsSingle().NonLazy();
        Container.Bind<SceneIntreface>().FromComponentInNewPrefab(sceneInterfacePrefab).AsSingle().NonLazy();
    }
}