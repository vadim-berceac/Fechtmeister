using UnityEngine;
using Zenject;

public class UIInputInstaller : MonoInstaller
{
    [SerializeField] private UIInput uiInput;
    public override void InstallBindings()
    {
        Container.Bind<UIInput>()
            .FromScriptableObject(uiInput)
            .AsSingle()
            .NonLazy();
    }
}