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
    public AnimationMixerPlayable GeneralMixerPlayable { get; private set; }
    
    public PlayablesAnimatorController PlayablesAnimatorController { get; private set; }

    [Inject]
    private void Construct()
    {
        Debug.Log("Construct");
        Graph = PlayableGraph.Create("General Graph");
        GeneralMixerPlayable = AnimationMixerPlayable.Create(Graph, CoreData.GeneralMixerCount);
        var playableOutput = AnimationPlayableOutput.Create(Graph, "Animation", CoreData.Animator);
        playableOutput.SetSourcePlayable(GeneralMixerPlayable);
        
        InitializeParts();
        
        Graph.Play();
    }

    private void InitializeParts()
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
    [field: SerializeField] public int GeneralMixerCount { get; private set; }
    [field: SerializeField] public Animator Animator { get; set; }
    [field: SerializeField] public CharacterController CharacterController { get; set; }
    [field: SerializeField] public CharacterCore CharacterCore { get; set; }
}