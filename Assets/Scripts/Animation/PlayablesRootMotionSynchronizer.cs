
using Unity.Burst;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

[BurstCompile]
public class PlayablesRootMotionSynchronizer
{
    private readonly PlayableGraphCore _playableGraphCore;
    private AnimationScriptPlayable _animationScriptPlayable;
    
    private float _currentFallSpeed;
    private Vector3 _deltaPosition;
    private Quaternion _deltaRotation;

    public PlayablesRootMotionSynchronizer(PlayableGraphCore graph)
    {
        _playableGraphCore = graph;
        
        var rootMotionJob = new RootMotionJob
        {
            CharacterController = _playableGraphCore.CoreData.CharacterController,
            CharacterTransform = _playableGraphCore.CoreData.CharacterCore.CashedTransform,
            ApplyRootMotion = _playableGraphCore.CoreData.Animator.applyRootMotion,
            CurrentFallSpeed = _currentFallSpeed
        };
        
        _animationScriptPlayable = AnimationScriptPlayable.Create(graph.Graph, rootMotionJob);
        
        var inputCount = graph.GeneralMixerPlayable.GetInputCount();
        graph.GeneralMixerPlayable.SetInputCount(inputCount + 1);

        graph.Graph.Connect(_animationScriptPlayable, 0, graph.GeneralMixerPlayable, inputCount);
    }
    
    public void OnFixedUpdate()
    {
        if (!_playableGraphCore.CoreData.CharacterCore.CurrentState.UseGravity)
        {
            _currentFallSpeed = 0;
        }
        else
        {
            _currentFallSpeed = _playableGraphCore.CoreData.CharacterCore.GetCurrentFallSpeed(
                useGravity: true,
                currentFallSpeed: _currentFallSpeed,
                isOnValidGround: _playableGraphCore.CoreData.CharacterController.isGrounded
            ) * _playableGraphCore.CoreData.CharacterCore.CurrentState.FallSpeedMultiplier;

            if (_animationScriptPlayable.IsValid())
            {
                var jobData = _animationScriptPlayable.GetJobData<RootMotionJob>();
                jobData.CurrentFallSpeed = _currentFallSpeed;
                _animationScriptPlayable.SetJobData(jobData);
            }
        }

        if (!_playableGraphCore.CoreData.Animator.applyRootMotion)
        {
            return;
        }
        _deltaPosition = _playableGraphCore.CoreData.Animator.deltaPosition;
        _deltaRotation = _playableGraphCore.CoreData.Animator.deltaRotation;
        
        if (!_animationScriptPlayable.IsValid())
        {
           return;
        }
        var jobData2 = _animationScriptPlayable.GetJobData<RootMotionJob>();
        jobData2.DeltaPosition = _deltaPosition;
        jobData2.DeltaRotation = _deltaRotation;
        _animationScriptPlayable.SetJobData(jobData2);
    }
}

public struct RootMotionJob : IAnimationJob
{
    public CharacterController CharacterController;
    public Transform CharacterTransform;
    public bool ApplyRootMotion;
    public float CurrentFallSpeed;
    public Vector3 DeltaPosition;
    public Quaternion DeltaRotation;
    
    private Vector3 _finalPosition;

    public void ProcessAnimation(AnimationStream stream)
    {
        if (!ApplyRootMotion)
            return;
        
        CharacterTransform.rotation *= DeltaRotation;

        _finalPosition = DeltaPosition + Vector3.up * CurrentFallSpeed * Time.deltaTime;
       
        CharacterController.Move(_finalPosition);

        CharacterController.transform.rotation = CharacterTransform.rotation;
    }

    public void ProcessRootMotion(AnimationStream stream)
    {
       
    }
}
