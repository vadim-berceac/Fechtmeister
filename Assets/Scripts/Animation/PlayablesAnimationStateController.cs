using System.Collections.Generic;
using System.Linq;
using ModestTree;
using UnityEngine;

public class PlayablesAnimationStateController
{
    private readonly HashSet<AnimationState> _animationStates;
    private readonly PlayableGraphCore _playableGraphCore;
    private readonly StatesContainer _statesContainer;
    
    private AnimationState _currentAnimationState;
    
    public PlayablesAnimationStateController(PlayableGraphCore playableGraphCore, StatesContainer statesContainer)
    {
        _playableGraphCore = playableGraphCore;
        _statesContainer = statesContainer;
        _animationStates = new HashSet<AnimationState>();

        var stateContainer = statesContainer.GetStates();
        
        foreach (var state in stateContainer)
        {
            _animationStates.Add(new AnimationState(state));
        }
        
        Initialize();
    }

    private void Initialize()
    {
        
    }

    public void SetAnimationState(string animationStateName)
    {
        if (_animationStates.IsEmpty() || _animationStates.Count < 1)
        {
            return;
        }
        var newState = _animationStates.FirstOrDefault(s => s.GetName() == animationStateName);
        
        Debug.Assert(newState != null, nameof(newState) + " != null");
        // назначить новый стейт, провести плавный переход
    }

    public AnimationState GetCurrentAnimationState()
    {
        return _currentAnimationState;
    }
    
    public void OnUpdate()
    {
        
    }
}
