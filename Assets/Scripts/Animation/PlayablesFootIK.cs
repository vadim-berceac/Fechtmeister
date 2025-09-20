/// <summary>
/// не работает, переписать
/// </summary>

using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class PlayablesFootIK : IAnimationJobPlayable
{
    private readonly PlayableGraphCore _core;
    private readonly FootIKData _footIKData;
    private AnimationScriptPlayable _ikPlayable;

    private float _raycastDistance = 2f;
    private float _footOffset = 0.2f;
    private float _weight = 1.0f;

    public PlayablesFootIK(PlayableGraphCore core)
    {
        _core = core;
        _footIKData = core.FootIKData;
        InitializeIKPlayable();
        
        
        for (int i = 0; i < (int)HumanBodyBones.LastBone; i++)
        {
            var bone = (HumanBodyBones)i;
            var t = _core.CoreData.Animator.GetBoneTransform(bone);
            if (t != null)
                Debug.Log($"Bone {bone} → {t.name}");
        }

    }

    private void InitializeIKPlayable()
    {
        var animator = _core.CoreData.Animator;
        if (animator == null || _footIKData.LeftFoot == null || _footIKData.RightFoot == null)
        {
            Debug.LogError("FootIK initialization failed: missing animator or foot transforms.");
            return;
        }

        var leftFoot = animator.BindStreamTransform(_footIKData.LeftFoot);
        var rightFoot = animator.BindStreamTransform(_footIKData.RightFoot);
        var leftParent = animator.BindStreamTransform(_footIKData.LeftFoot.parent);
        var rightParent = animator.BindStreamTransform(_footIKData.RightFoot.parent);

        var job = new FootIKJob
        {
            leftFoot = leftFoot,
            rightFoot = rightFoot,
            leftFootParent = leftParent,
            rightFootParent = rightParent,
            raycastDistance = _raycastDistance,
            footOffset = _footOffset,
            weight = _weight
        };

        _ikPlayable = AnimationScriptPlayable.Create(_core.Graph, job);
        _ikPlayable.SetJobData(job);

        var mixer = _core.GeneralMixerPlayable;
        var inputIndex = mixer.GetInputCount();
        mixer.SetInputCount(inputIndex + 1);
        _core.Graph.Connect(_ikPlayable, 0, mixer, inputIndex);
        mixer.SetInputWeight(inputIndex, 1.0f);
    }

    public void OnUpdate(float deltaTime)
    {
        if (!_ikPlayable.IsValid()) return;

        var left = ComputeFootTarget(_footIKData.LeftFoot, _core.FootIKData.GroundLayerMask);
        var right = ComputeFootTarget(_footIKData.RightFoot, _core.FootIKData.GroundLayerMask);

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
        if (Physics.Raycast(ray, out var hit, _raycastDistance, groundMask))
        {
            var targetRot = Quaternion.FromToRotation(Vector3.up, hit.normal) * foot.rotation;
            return (true, targetRot);
        }

        return (false, Quaternion.identity);
    }

    public void SetIKWeight(float weight)
    {
        _weight = Mathf.Clamp01(weight);
        var job = _ikPlayable.GetJobData<FootIKJob>();
        job.weight = _weight;
        _ikPlayable.SetJobData(job);
    }

    public PlayableHandle GetHandle() => _ikPlayable.GetHandle();
    public T GetJobData<T>() where T : struct, IAnimationJob => _ikPlayable.GetJobData<T>();
    public void SetJobData<T>(T data) where T : struct, IAnimationJob => _ikPlayable.SetJobData(data);

    public void Dispose()
    {
        if (_ikPlayable.IsValid())
            _ikPlayable.Destroy();
    }
}

public struct FootIKJob : IAnimationJob
{
    public TransformStreamHandle leftFoot;
    public TransformStreamHandle rightFoot;
    public TransformStreamHandle leftFootParent;
    public TransformStreamHandle rightFootParent;

    public float raycastDistance;
    public float footOffset;
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