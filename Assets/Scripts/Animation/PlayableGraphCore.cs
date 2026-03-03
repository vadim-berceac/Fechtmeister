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
    [SerializeField] private SkeletonProfile skeletonProfile;
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
            ApplySkeletonProfileLookAt();
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

    private void ApplySkeletonProfileLookAt()
    {
        if (skeletonProfile == null || _lookAtSystem == null) return;

        foreach (var offset in skeletonProfile.lookAtOffsets)
            _lookAtSystem.SetBoneRotationOffset(offset.bone, offset.eulerOffset);
    }

    // ==================== EQUIPMENT ====================

    /// <summary>
    /// Прикрепить объект к кости. Если в SkeletonProfile есть коррекция для этой кости — применяется поверх переданных значений.
    /// </summary>
    public bool AttachEquipment(Transform source, HumanBodyBones bone, bool enabled,
        Vector3 position = default, Vector3 rotation = default, float scale = 1f)
    {
        if (skeletonProfile != null && skeletonProfile.TryGetBoneCorrection(bone, out var correction))
        {
            position += correction.position;
            rotation += correction.rotation;
            scale *= correction.scale;
        }

        return Animator.AttachTransformSource(source, bone, position, rotation, scale, enabled);
    }

    // ==================== LOOK AT ====================

    public override void OnManagedUpdate()
    {
        FullBodyAnimatorController.OnUpdate(Time.deltaTime);
        UpperBodyLayerController.OnUpdate();
    }

    public override void OnManagedLateUpdate()
    {
        _lookAtSystem?.Update();
    }

    public void SetLookAtBoneWeight(HumanBodyBones humanBone, float weight)
        => _lookAtSystem?.SetBoneWeight(humanBone, weight);

    public float GetLookAtBoneWeight(HumanBodyBones humanBone)
        => _lookAtSystem?.GetBoneWeight(humanBone) ?? 0f;

    public void SetLookAtBoneRotationOffset(HumanBodyBones humanBone, Vector3 eulerOffset)
        => _lookAtSystem?.SetBoneRotationOffset(humanBone, eulerOffset);

    public Vector3 GetLookAtBoneRotationOffset(HumanBodyBones humanBone)
        => _lookAtSystem?.GetBoneRotationOffset(humanBone) ?? Vector3.zero;

    // ==================== LIFECYCLE ====================

    protected override void OnDisable()
    {
        base.OnDisable();

        _lookAtSystem?.Dispose();

        Graph.Destroy();
    }
}