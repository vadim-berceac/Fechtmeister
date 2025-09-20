using Unity.Burst;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Zenject;

[BurstCompile]
public class PlayableGraphCore : MonoBehaviour
{
    [field: SerializeField] public PlayableGraphCoreData CoreData { get; set; }
    [field: SerializeField] public FootIKData FootIKData { get; set; }
    
    public PlayableGraph Graph { get; private set; }
    public AnimationMixerPlayable GeneralMixerPlayable { get; private set; }
    
    public PlayablesAnimatorController PlayablesAnimatorController { get; private set; }
    public PlayablesRootMotionSynchronizer PlayablesRootMotionSynchronizer { get; private set; }
    //public PlayablesFootIK PlayablesFootIK { get; private set; }

    [Inject]
    private void Construct()
    {
        Debug.Log("Construct");
        Graph = PlayableGraph.Create("General Graph");
        GeneralMixerPlayable = AnimationMixerPlayable.Create(Graph, 0);
        var playableOutput = AnimationPlayableOutput.Create(Graph, "Animation", CoreData.Animator);
        playableOutput.SetSourcePlayable(GeneralMixerPlayable);
        
        InitializeParts();
        
        Graph.Play();
    }

    private void InitializeParts()
    {
        PlayablesAnimatorController = new PlayablesAnimatorController(this);
        PlayablesRootMotionSynchronizer = new PlayablesRootMotionSynchronizer(this);
        //PlayablesFootIK = new PlayablesFootIK(this);
    }
    
    private void Update()
    {
        PlayablesAnimatorController.OnUpdate(Time.deltaTime);
        //PlayablesFootIK.OnUpdate(Time.deltaTime);
    }

    private void FixedUpdate()
    {
        PlayablesRootMotionSynchronizer.OnFixedUpdate();
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
    [field: SerializeField] public CharacterController CharacterController { get; set; }
    [field: SerializeField] public CharacterCore CharacterCore { get; set; }
}

[System.Serializable]
public struct FootIKData
{
    [field: SerializeField] public Transform LeftFoot { get; set; }
    [field: SerializeField] public Transform RightFoot { get; set; }
    [field: SerializeField] public LayerMask GroundLayerMask { get; set; }
}
