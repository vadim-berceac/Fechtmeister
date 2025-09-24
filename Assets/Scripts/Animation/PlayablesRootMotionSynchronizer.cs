using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class PlayablesRootMotionSynchronizer
{
    private readonly PlayableGraphCore _playableGraphCore;
    private AnimationScriptPlayable _animationScriptPlayable;
    private float _currentFallSpeed;

    public PlayablesRootMotionSynchronizer(PlayableGraphCore graph)
    {
        _playableGraphCore = graph;

        var rootMotionJob = new RootMotionJob
        {
            ApplyRootMotion = _playableGraphCore.CoreData.Animator.applyRootMotion,
            CurrentFallSpeed = 0f
        };

        _animationScriptPlayable = AnimationScriptPlayable.Create(graph.Graph, rootMotionJob);
        graph.Graph.Connect(_animationScriptPlayable, 0, graph.GeneralMixerPlayable, 2);
        graph.GeneralMixerPlayable.SetInputWeight(2, 1.0f);
    }

    public void OnFixedUpdate()
    {
        var core = _playableGraphCore.CoreData.CharacterCore;
        var controller = _playableGraphCore.CoreData.CharacterController;
        var animator = _playableGraphCore.CoreData.Animator;
        var transform = _playableGraphCore.transform;

        _currentFallSpeed = core.CurrentState.UseGravity
            ? core.GetCurrentFallSpeed(true, _currentFallSpeed, controller.isGrounded) * core.CurrentState.FallSpeedMultiplier
            : 0f;

        if (_animationScriptPlayable.IsValid())
        {
            var jobData = _animationScriptPlayable.GetJobData<RootMotionJob>();
            jobData.CurrentFallSpeed = _currentFallSpeed;
            jobData.ApplyRootMotion = animator.applyRootMotion;
            jobData.DeltaPosition = animator.deltaPosition;
            jobData.DeltaRotation = animator.deltaRotation;
            _animationScriptPlayable.SetJobData(jobData);
        }

        // Применение root motion на главном потоке
        if (_animationScriptPlayable.IsValid())
        {
            var jobData = _animationScriptPlayable.GetJobData<RootMotionJob>();

            if (core.CurrentState.UseGravity || animator.applyRootMotion)
            {
                controller.Move(jobData.ComputedDeltaPosition);
                transform.rotation *= jobData.ComputedDeltaRotation;
            }
        }
    }
}


public struct RootMotionJob : IAnimationJob
{
    public bool ApplyRootMotion;
    public float CurrentFallSpeed;
    public Vector3 DeltaPosition;
    public Quaternion DeltaRotation;

    // Выходные данные, безопасные для чтения в MonoBehaviour
    public Vector3 ComputedDeltaPosition;
    public Quaternion ComputedDeltaRotation;

    public void ProcessAnimation(AnimationStream stream)
    {
        if (!ApplyRootMotion)
        {
            ComputedDeltaPosition = Vector3.zero;
            ComputedDeltaRotation = Quaternion.identity;
            return;
        }

        ComputedDeltaRotation = DeltaRotation;
        ComputedDeltaPosition = DeltaPosition + Vector3.up * CurrentFallSpeed * Time.deltaTime;
    }

    public void ProcessRootMotion(AnimationStream stream) { }
}

