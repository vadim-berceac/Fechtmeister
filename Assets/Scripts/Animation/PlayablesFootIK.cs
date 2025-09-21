/// <summary>
/// не работает, переписать
/// </summary>

using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class PlayablesFootIK : IAnimationJobPlayable
{
    private readonly PlayableGraphCore _core;
    
    private readonly Transform _leftFootTransform;
    private readonly Transform _rightFootTransform;
    private AnimationScriptPlayable _ikPlayable;

    public PlayablesFootIK(PlayableGraphCore core)
    {
        _core = core;
        _leftFootTransform = core.CoreData.Animator.GetBoneTransform(HumanBodyBones.LeftFoot);
        _rightFootTransform = core.CoreData.Animator.GetBoneTransform(HumanBodyBones.RightFoot);
        InitializeIKPlayable();
    }

    private void InitializeIKPlayable()
    {
        var animator = _core.CoreData.Animator;

        var leftFoot = animator.BindStreamTransform(_leftFootTransform);
        var rightFoot = animator.BindStreamTransform(_rightFootTransform);
        var leftParent = animator.BindStreamTransform(animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg));
        var rightParent = animator.BindStreamTransform(animator.GetBoneTransform(HumanBodyBones.RightLowerLeg));

        var job = new FootIKJob
        {
            leftFoot = leftFoot,
            rightFoot = rightFoot,
            leftFootParent = leftParent,
            rightFootParent = rightParent,
            weight = _core.FootIKData.JobWeight
        };

        _ikPlayable = AnimationScriptPlayable.Create(_core.Graph, job);
        _ikPlayable.SetJobData(job);

        var mixer = _core.GeneralMixerPlayable;
        var inputIndex = mixer.GetInputCount();
        mixer.SetInputCount(inputIndex + 1);
        _core.Graph.Connect(_ikPlayable, 0, mixer, inputIndex);
        mixer.SetInputWeight(inputIndex, _core.FootIKData.PlayableInputWeight);
    }

    public void OnUpdate()
    {
        if (!_ikPlayable.IsValid()) return;

        var left = ComputeFootTarget(_leftFootTransform, _core.FootIKData.GroundLayerMask);
        var right = ComputeFootTarget(_rightFootTransform, _core.FootIKData.GroundLayerMask);

        var job = _ikPlayable.GetJobData<FootIKJob>();
        job.leftFootTargetRotation = left.rotation;
        job.rightFootTargetRotation = right.rotation;
        job.leftFootHit = left.hit;
        job.rightFootHit = right.hit;
        _ikPlayable.SetJobData(job);
    }

    private (bool hit, Quaternion rotation) ComputeFootTarget(Transform foot, LayerMask groundMask)
    {
        if (foot == null || foot.parent == null)
            return (false, Quaternion.identity);

        var origin = foot.position + Vector3.up * 0.5f;
        var ray = new Ray(origin, Vector3.down);
        if (Physics.Raycast(ray, out var hit, _core.FootIKData.RaycastDistance, groundMask))
        {
            var targetRot = Quaternion.FromToRotation(Vector3.up, hit.normal) * foot.rotation;
            return (true, targetRot);
        }

        return (false, Quaternion.identity);
    }
    
    public PlayableHandle GetHandle() => _ikPlayable.GetHandle();
    public T GetJobData<T>() where T : struct, IAnimationJob => _ikPlayable.GetJobData<T>();
    public void SetJobData<T>(T data) where T : struct, IAnimationJob => _ikPlayable.SetJobData(data);

}

public struct FootIKJob : IAnimationJob
{
    public TransformStreamHandle leftFoot;
    public TransformStreamHandle rightFoot;
    public TransformStreamHandle leftFootParent;
    public TransformStreamHandle rightFootParent;
    public float weight;

    public Quaternion leftFootTargetRotation;
    public Quaternion rightFootTargetRotation;
    public bool leftFootHit;
    public bool rightFootHit;

    public void ProcessRootMotion(AnimationStream stream) { }

    public void ProcessAnimation(AnimationStream stream)
    {
        if (!leftFoot.IsValid(stream) || !rightFoot.IsValid(stream)) return;

        ApplyRotation(stream, leftFoot, leftFootParent, leftFootTargetRotation, leftFootHit);
        ApplyRotation(stream, rightFoot, rightFootParent, rightFootTargetRotation, rightFootHit);
    }

    private void ApplyRotation(AnimationStream stream, TransformStreamHandle foot, TransformStreamHandle parent,
        Quaternion targetWorldRotation, bool hit)
    {
        if (!hit || !parent.IsValid(stream)) return;

        var parentRot = parent.GetRotation(stream);
        var targetLocalRot = Quaternion.Inverse(parentRot) * targetWorldRotation;

        var currentRot = foot.GetRotation(stream);
        var newRot = Quaternion.Slerp(currentRot, targetLocalRot, weight);

        foot.SetRotation(stream, newRot);
    }
}