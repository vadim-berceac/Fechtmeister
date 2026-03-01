using Unity.Burst;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Zenject;

[BurstCompile]
public class PlayableGraphCore : ManagedUpdatableObject
{
    [field: SerializeField] public int GeneralMixerCount { get; private set; }
    public PlayableGraph Graph { get; private set; }
    public Animator Animator { get; private set; }
    public AnimationLayerMixerPlayable LayerMixer { get; private set; }
    public AnimationMixerPlayable FullBodyLayerMixer0 { get; private set; }
    public AnimationMixerPlayable UpperBodyLayerMixer1 { get; private set; }
    
    public PlayablesAnimatorController FullBodyAnimatorController { get; private set; }
    public PlayablesLayerController UpperBodyLayerController { get; private set; }

    [SerializeField] private LookAtBoneConfig[] lookAtBones;
    private LookAtSystem _lookAtSystem;

    [Inject]
    private void Construct(StatesContainer statesContainer, Animator animator)
    {
        Graph = PlayableGraph.Create("General Graph");
        Animator = animator;
        LayerMixer = AnimationLayerMixerPlayable.Create(Graph, 2);
        FullBodyLayerMixer0 = AnimationMixerPlayable.Create(Graph, GeneralMixerCount);
        UpperBodyLayerMixer1 = AnimationMixerPlayable.Create(Graph, 2); 
        
        Graph.Connect(FullBodyLayerMixer0, 0, LayerMixer, 0);
        Graph.Connect(UpperBodyLayerMixer1, 0, LayerMixer, 1);
        LayerMixer.SetInputWeight(FullBodyLayerMixer0, 1f);
        LayerMixer.SetInputWeight(UpperBodyLayerMixer1, 0f);
       
        LayerMixer.SetLayerMaskFromAvatarMask(1, statesContainer.GetAvatarMasksSettings().UpperBodyMask);
        LayerMixer.SetInputWeight(UpperBodyLayerMixer1, 1f);
        LayerMixer.SetLayerAdditive(1, false);
      
        Playable finalPlayable = LayerMixer;
        if (lookAtBones != null && lookAtBones.Length > 0)
        {
            _lookAtSystem = new LookAtSystem(lookAtBones, Animator);
            finalPlayable = _lookAtSystem.Initialize(Graph, LayerMixer);
        }
       
        var playableOutput = AnimationPlayableOutput.Create(Graph, "Animation", Animator);
        playableOutput.SetSourcePlayable(finalPlayable);
        
        InitializeParts();
        
        Graph.Play();
    }

    private void InitializeParts()
    {
        FullBodyAnimatorController = new PlayablesAnimatorController(this);
        UpperBodyLayerController = new PlayablesLayerController(Graph, UpperBodyLayerMixer1, LayerMixer, 1);
    }
    
    public override void OnManagedUpdate()
    {
        FullBodyAnimatorController.OnUpdate(Time.deltaTime);
        UpperBodyLayerController.OnUpdate();
    }
    
    public override void OnManagedLateUpdate()
    {
        _lookAtSystem?.Update();
    }
    
    protected override void OnDisable()
    {
        base.OnDisable();
       
        _lookAtSystem?.Dispose();
        
        Graph.Destroy();
    }
    
    public void SetLookAtBoneWeight(HumanBodyBones humanBone, float weight)
    {
        _lookAtSystem?.SetBoneWeight(humanBone, weight);
    }
    
    public float GetLookAtBoneWeight(HumanBodyBones humanBone)
    {
        return _lookAtSystem?.GetBoneWeight(humanBone) ?? 0f;
    }
    
    public void SetLookAtBoneRotationOffset(HumanBodyBones humanBone, Vector3 eulerOffset)
    {
        _lookAtSystem?.SetBoneRotationOffset(humanBone, eulerOffset);
    }
    
    public Vector3 GetLookAtBoneRotationOffset(HumanBodyBones humanBone)
    {
        return _lookAtSystem?.GetBoneRotationOffset(humanBone) ?? Vector3.zero;
    }
}