using UnityEngine;
using Zenject;

public class UIInputInstaller : MonoInstaller
{
    [SerializeField] private GameObject uiInputPrefab;
    public override void InstallBindings()
    {
        Container.BindInterfacesTo<IUIInputSet>().FromComponentInNewPrefab(uiInputPrefab).AsSingle().NonLazy();
    }
}