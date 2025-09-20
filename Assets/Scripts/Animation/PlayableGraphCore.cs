using Unity.Burst;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

[BurstCompile]
public class PlayableGraphCore : MonoBehaviour
{
    [field: SerializeField] public PlayableGraphCoreData CoreData { get; set; }
    
    public PlayableGraph Graph { get; private set; }
    public AnimationMixerPlayable GeneralMixerPlayable { get; private set; }
    
    public PlayablesAnimatorController PlayablesAnimatorController { get; private set; }

    private void Awake()
    {
        Graph = PlayableGraph.Create("General Graph");
        GeneralMixerPlayable = AnimationMixerPlayable.Create(Graph, CoreData.GeneralMixerInputsCount);
        var playableOutput = AnimationPlayableOutput.Create(Graph, "Animation", CoreData.Animator);
        playableOutput.SetSourcePlayable(GeneralMixerPlayable);
        
        Graph.Play();
    }

    private void Start()
    {
        PlayablesAnimatorController = new PlayablesAnimatorController(this);
    }

    private void Update()
    {
        PlayablesAnimatorController.OnUpdate(Time.deltaTime);
    }

    private void OnDestroy()
    {
        Graph.Destroy();
    }
}

[System.Serializable]
public struct PlayableGraphCoreData
{
    [field: SerializeField] public Animator Animator { get; set; }
    [field: SerializeField] public int GeneralMixerInputsCount { get; set; }
}
