using Unity.Behavior;
using UnityEngine;
using Zenject;

public class CharacterInstaller : MonoInstaller
{
    [SerializeField] private GameObject targeting;
    [SerializeField] private GameObject aimTargeting;
    [SerializeField] private GameObject modelTag;

    public override void InstallBindings()
    {
        Container
            .Bind<CharacterController>()
            .FromInstance(GetComponentInParent<CharacterController>())
            .AsSingle()
            .NonLazy();
        
        Container
            .Bind<Animator>()
            .FromInstance(GetComponentInParent<Animator>())
            .AsSingle()
            .NonLazy();
        
        Container
            .Bind<PlayableGraphCore>()
            .FromInstance(GetComponentInParent<PlayableGraphCore>())
            .AsSingle()
            .NonLazy();
        
        Container
            .Bind<CharacterCore>()
            .FromInstance(GetComponentInParent<CharacterCore>())
            .AsSingle()
            .NonLazy();
        
        Container
            .Bind<CharacterInfoComponent>()
            .FromInstance(GetComponentInParent<CharacterInfoComponent>())
            .AsSingle()
            .NonLazy();
        
        Container
            .Bind<BehaviorGraphAgent>()
            .FromInstance(GetComponentInParent<BehaviorGraphAgent>())
            .AsSingle()
            .NonLazy();
        
        Container
            .Bind<BehaviorNewInput>()
            .FromInstance(GetComponentInParent<BehaviorNewInput>())
            .AsSingle()
            .NonLazy();
        
        Container
            .Bind<CapsuleCollider>()
            .FromInstance(GetComponentInParent<CapsuleCollider>())
            .AsSingle()
            .NonLazy();
        
        Container
            .Bind<ItemTargeting>()
            .FromInstance(targeting.GetComponent<ItemTargeting>())
            .AsSingle()
            .NonLazy();
        
        Container
            .Bind<AimTargeting>()
            .FromInstance(aimTargeting.GetComponent<AimTargeting>())
            .AsSingle()
            .NonLazy();
        
        Container
            .Bind<ModelTag>()
            .FromInstance(modelTag.GetComponent<ModelTag>())
            .AsSingle()
            .NonLazy();
        
        Container
            .Bind<CharacterPresetLoader>()
            .FromInstance(GetComponentInParent<CharacterPresetLoader>())
            .AsSingle()
            .NonLazy();
        
        Container
            .Bind<HealthComponent>()
            .FromInstance(GetComponentInParent<HealthComponent>())
            .AsSingle()
            .NonLazy();
    }
}