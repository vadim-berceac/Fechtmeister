using Unity.Cinemachine;
using UnityEngine;
using Zenject;

public class SceneContainersInstaller : MonoInstaller
{
    [SerializeField] private GameObject cameraPrefab;
    [SerializeField] private GameObject inputPrefab;
    [SerializeField] private GameObject sceneInterfacePrefab;
    //
    [SerializeField] private StatesContainer statesContainer;
    [SerializeField] private SceneCharacterContainer sceneCharacterContainer;
    [SerializeField] private GameWindowContainer gameWindowContainer;
    [SerializeField] private CentralizedUpdateSystem updateSystem;
    [SerializeField] private VisionSystem visionSystem;
    
    public override void InstallBindings()
    {
        Container.Bind<PlayerInput>().FromComponentInNewPrefab(inputPrefab).AsSingle().NonLazy();
        Container.Bind<SceneIntreface>().FromComponentInNewPrefab(sceneInterfacePrefab).AsSingle().NonLazy();
        
        //
        Container.Bind<StatesContainer>()
            .FromScriptableObject(statesContainer)
            .AsSingle()
            .NonLazy();
        
        Container.Bind<SceneCharacterContainer>()
            .FromScriptableObject(sceneCharacterContainer)
            .AsSingle()
            .NonLazy();
        
        Container.Bind<GameWindowContainer>()
            .FromScriptableObject(gameWindowContainer)
            .AsSingle()
            .NonLazy();
        
        Container.BindInterfacesAndSelfTo<CentralizedUpdateSystem>()
            .FromScriptableObject(updateSystem)
            .AsSingle()
            .NonLazy();
        
        Container.BindInterfacesAndSelfTo<VisionSystem>()
            .FromScriptableObject(visionSystem)
            .AsSingle()
            .NonLazy();
        
        //
        Container.BindInterfacesAndSelfTo<InventoryDrawer>().FromComponentsInHierarchy().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<Sight>().FromComponentsInHierarchy().AsSingle().NonLazy();
        Container.Bind<SceneCamera>().FromComponentInNewPrefab(cameraPrefab).AsSingle().NonLazy();
        Container.Bind<CinemachineImpulseSource>().FromComponentInHierarchy().AsSingle().NonLazy();
    }
}