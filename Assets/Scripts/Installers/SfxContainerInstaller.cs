using UnityEngine;
using Zenject;

public class SfxContainerInstaller : MonoInstaller
{
    [SerializeField] private SfxContainer sfxContainer;
    public override void InstallBindings()
    {
        Container.Bind<SfxContainer>().FromScriptableObject(sfxContainer).AsSingle().NonLazy();
    }
}