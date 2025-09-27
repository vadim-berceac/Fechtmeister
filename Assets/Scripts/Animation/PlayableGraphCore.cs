using Unity.Burst;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Zenject;

[BurstCompile]
public class PlayableGraphCore : MonoBehaviour
{
    [field: SerializeField] public PlayableGraphCoreData CoreData { get; set; }
    
    public PlayableGraph Graph { get; private set; }
    public AnimationLayerMixerPlayable LayerMixer { get; private set; }
    public AnimationMixerPlayable FullBodyLayerMixer0 { get; private set; }
    public AnimationMixerPlayable HalfBodyLayerMixer1 { get; private set; }
    
    public PlayablesAnimatorController FullBodyAnimatorController { get; private set; }
    //написать упрощенный вариант анимационного контроллера для HalfBody

    [Inject]
    private void Construct()
    {
        Graph = PlayableGraph.Create("General Graph");
        LayerMixer = AnimationLayerMixerPlayable.Create(Graph, 2);
        FullBodyLayerMixer0 = AnimationMixerPlayable.Create(Graph, CoreData.GeneralMixerCount);
        HalfBodyLayerMixer1 = AnimationMixerPlayable.Create(Graph, 1);
        
        Graph.Connect(FullBodyLayerMixer0, 0, LayerMixer, 0);
        Graph.Connect(HalfBodyLayerMixer1, 0, LayerMixer, 1);
        LayerMixer.SetInputWeight(FullBodyLayerMixer0, 1f);
        LayerMixer.SetInputWeight(HalfBodyLayerMixer1, 1f);
        
        var playableOutput = AnimationPlayableOutput.Create(Graph, "Animation", CoreData.Animator);
        playableOutput.SetSourcePlayable(LayerMixer);
        
        InitializeParts();
        
        Graph.Play();
        // вроде бы структура графа корректна
        // осталось только подключать клип с подходящей маской к HalfBodyLayerMixer1
    }

    private void InitializeParts()
    {
        FullBodyAnimatorController = new PlayablesAnimatorController(this);
    }
    
    private void Update()
    {
        FullBodyAnimatorController.OnUpdate(Time.deltaTime);
    }
    
    private void OnDestroy()
    {
        Graph.Destroy();
    }
}

[System.Serializable]
public struct PlayableGraphCoreData
{
    [field: SerializeField] public int GeneralMixerCount { get; private set; }
    [field: SerializeField] public Animator Animator { get; set; }
    [field: SerializeField] public CharacterController CharacterController { get; set; }
    [field: SerializeField] public CharacterCore CharacterCore { get; set; }
}