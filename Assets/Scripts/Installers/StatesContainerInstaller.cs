using MicahW.PointGrass;
using UnityEngine;
using Zenject;

public class StatesContainerInstaller : MonoInstaller
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private GameObject prefab1;
    public override void InstallBindings()
    {
        Container.Bind<StatesContainer>().FromComponentInNewPrefab(prefab).AsSingle().NonLazy();
        Container.Bind<PointGrassDisplacementManager>().FromComponentInNewPrefab(prefab1).AsSingle().NonLazy();
    }
}
