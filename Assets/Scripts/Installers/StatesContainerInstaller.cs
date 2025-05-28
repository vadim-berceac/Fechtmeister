using UnityEngine;
using Zenject;

public class StatesContainerInstaller : MonoInstaller
{
    [SerializeField] private GameObject prefab;
    public override void InstallBindings()
    {
        Container.Bind<StatesContainer>().FromComponentInNewPrefab(prefab).AsSingle().NonLazy();
    }
}
