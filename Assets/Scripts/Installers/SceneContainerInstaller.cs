using UnityEngine;
using Zenject;

public class SceneContainerInstaller : MonoInstaller
{
    [SerializeField] private ScriptableObject _scriptableObject;
    public override void InstallBindings()
    {
        Container.Bind<SceneContainer>().FromScriptableObject(_scriptableObject).AsSingle().NonLazy();
    }
}