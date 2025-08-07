using Zenject;

public class InventoryDrawerInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<InventoryDrawer>().FromComponentsInHierarchy().AsSingle().NonLazy();
    }
}