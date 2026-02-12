using Unity.Burst;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Zenject;

[BurstCompile]
public class PlayableGraphCore : ManagedUpdatableObject
{
    [field: SerializeField] public PlayableGraphCoreData CoreData { get; set; }
    public PlayableGraph Graph { get; private set; }
    public AnimationLayerMixerPlayable LayerMixer { get; private set; }
    public AnimationMixerPlayable FullBodyLayerMixer0 { get; private set; }
    public AnimationMixerPlayable UpperBodyLayerMixer1 { get; private set; }
    
    public PlayablesAnimatorController FullBodyAnimatorController { get; private set; }
    public PlayablesLayerController UpperBodyLayerController { get; private set; }

    [SerializeField] private LookAtBoneConfig[] lookAtBones;
    private LookAtSystem lookAtSystem;

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
      
        Playable finalPlayable = LayerMixer;
        if (lookAtBones != null && lookAtBones.Length > 0)
        {
            lookAtSystem = new LookAtSystem(lookAtBones, CoreData.Animator);
            finalPlayable = lookAtSystem.Initialize(Graph, LayerMixer);
        }
       
        var playableOutput = AnimationPlayableOutput.Create(Graph, "Animation", CoreData.Animator);
        playableOutput.SetSourcePlayable(finalPlayable);
        
        InitializeParts();
        
        Graph.Play();
    }

    private void InitializeParts()
    {
        FullBodyAnimatorController = new PlayablesAnimatorController(this);
        UpperBodyLayerController = new PlayablesLayerController(Graph, UpperBodyLayerMixer1);
    }
    
    public override void OnManagedUpdate()
    {
        FullBodyAnimatorController.OnUpdate(Time.deltaTime);
        UpperBodyLayerController.OnUpdate();
    }
    
    public override void OnManagedLateUpdate()
    {
        lookAtSystem?.Update();
    }
    
    protected override void OnDisable()
    {
        base.OnDisable();
       
        lookAtSystem?.Dispose();
        
        Graph.Destroy();
    }
    
    public void SetLookAtBoneWeight(HumanBodyBones humanBone, float weight)
    {
        lookAtSystem?.SetBoneWeight(humanBone, weight);
    }
    
    public float GetLookAtBoneWeight(HumanBodyBones humanBone)
    {
        return lookAtSystem?.GetBoneWeight(humanBone) ?? 0f;
    }
    
    public void SetLookAtBoneRotationOffset(HumanBodyBones humanBone, Vector3 eulerOffset)
    {
        lookAtSystem?.SetBoneRotationOffset(humanBone, eulerOffset);
    }
    
    public Vector3 GetLookAtBoneRotationOffset(HumanBodyBones humanBone)
    {
        return lookAtSystem?.GetBoneRotationOffset(humanBone) ?? Vector3.zero;
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