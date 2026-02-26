using MicahW.PointGrass;
using UnityEngine;
using Zenject;

public class GrassInstaller : MonoInstaller
{
    [SerializeField] private PointGrassWind pointGrassWind;
    [SerializeField] private PointGrassDisplacementManager pointGrassDisplacementManager;
    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<PointGrassDisplacementManager>()
            .FromScriptableObject(pointGrassDisplacementManager)
            .AsSingle()
            .NonLazy();
        
        Container.BindInterfacesAndSelfTo<PointGrassWind>()
            .FromScriptableObject(pointGrassWind)
            .AsSingle()
            .NonLazy();
    }
}
