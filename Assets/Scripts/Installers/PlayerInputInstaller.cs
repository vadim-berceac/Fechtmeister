using UnityEngine;
using Zenject;

public class PlayerInputInstaller : MonoInstaller
{
    [SerializeField] private GameObject prefab;
    public override void InstallBindings()
    {
        Container.Bind<PlayerInput>().FromComponentInNewPrefab(prefab).AsSingle().NonLazy();
    }
}