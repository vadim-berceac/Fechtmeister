using UnityEngine;
using Zenject;

public class SceneCameraInstaller : MonoInstaller
{
    [SerializeField] private GameObject prefab;
    public override void InstallBindings()
    {
        Container.Bind<SceneCamera>().FromComponentInNewPrefab(prefab).AsSingle().NonLazy();
    }
}