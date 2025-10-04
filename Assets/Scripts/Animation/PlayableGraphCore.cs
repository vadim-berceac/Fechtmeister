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
    public AnimationMixerPlayable UpperBodyLayerMixer1 { get; private set; }
    
    public PlayablesAnimatorController FullBodyAnimatorController { get; private set; }
    public PlayablesLayerController UpperBodyLayerController { get; private set; }

    [Inject]
    private void Construct(StatesContainer statesContainer)
    {
        Graph = PlayableGraph.Create("General Graph");
        LayerMixer = AnimationLayerMixerPlayable.Create(Graph, 2);
        FullBodyLayerMixer0 = AnimationMixerPlayable.Create(Graph, CoreData.GeneralMixerCount);
        UpperBodyLayerMixer1 = AnimationMixerPlayable.Create(Graph, 1);
        
        Graph.Connect(FullBodyLayerMixer0, 0, LayerMixer, 0);
        Graph.Connect(UpperBodyLayerMixer1, 0, LayerMixer, 1);
        LayerMixer.SetInputWeight(FullBodyLayerMixer0, 1f);
        LayerMixer.SetInputWeight(UpperBodyLayerMixer1, 0f);
       
        LayerMixer.SetLayerMaskFromAvatarMask(1, statesContainer.GetAvatarMasksSettings().UpperBodyMask);
        LayerMixer.SetInputWeight(UpperBodyLayerMixer1, 1f);
        LayerMixer.SetLayerAdditive(1, true);
        
        var playableOutput = AnimationPlayableOutput.Create(Graph, "Animation", CoreData.Animator);
        playableOutput.SetSourcePlayable(LayerMixer);
        
        InitializeParts();
        
        Graph.Play();
    }

    private void InitializeParts()
    {
        FullBodyAnimatorController = new PlayablesAnimatorController(this);
        UpperBodyLayerController = new PlayablesLayerController(Graph, UpperBodyLayerMixer1);
    }
    
    private void Update()
    {
        FullBodyAnimatorController.OnUpdate(Time.deltaTime);
        UpperBodyLayerController.OnUpdate();
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