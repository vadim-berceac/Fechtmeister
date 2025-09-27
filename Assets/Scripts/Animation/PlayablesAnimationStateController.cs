using System.Collections.Generic;
using System.Linq;
using ModestTree;
using UnityEngine;

public class PlayablesAnimationStateController
{
    private readonly HashSet<AnimationState> _animationStates;
    private readonly PlayableGraphCore _playableGraphCore;
    
    private AnimationState _currentAnimationState;
    
    public PlayablesAnimationStateController(PlayableGraphCore playableGraphCore, StatesContainer statesContainer)
    {
        _playableGraphCore = playableGraphCore;
        _animationStates = new HashSet<AnimationState>();

        var stateContainer = statesContainer.GetStates();
        
        foreach (var state in stateContainer)
        {
            _animationStates.Add(new AnimationState(playableGraphCore.Graph, state));
        }
    }

    public void SetAnimationState(string animationStateName, int blendParamValue, int clipParamValue)
    {
        if (_animationStates.IsEmpty() || _animationStates.Count < 1)
        {
            return;
        }
        var newState = _animationStates.FirstOrDefault(s => s.GetName() == animationStateName);
        
        _currentAnimationState = newState;

        SetAnimationBlend(blendParamValue, clipParamValue);
        
        Debug.LogWarning(newState?.GetName());
    }

    private void SetAnimationBlend(int blendParamValue, int clipParamValue)
    {
        var mixer = _currentAnimationState.GetMixerPlayable(blendParamValue, clipParamValue);
        
        _playableGraphCore.Graph.Connect(mixer, 0, _playableGraphCore.GeneralMixerPlayable, 1);
    }

    public AnimationState GetCurrentAnimationState()
    {
        return _currentAnimationState;
    }

    public bool IsCurrentStateComplete()
    {
        return _currentAnimationState.IsComplete();
    }

    public float GetCurrentStateNormalizedDuration()
    {
        return _currentAnimationState.GetNormalizedDuration();
    }
    
    public void OnUpdate()
    {
        SmoothBlend();
    }

    private void SmoothBlend()
    {
        
    }
}
