using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using System.Collections.Generic;
using System.Linq;

public class CharacterPlayablesAnimatorController
{
    private PlayableGraph _playableGraph;
    private readonly PlayableOutput _playableOutput;
    private readonly AnimationMixerPlayable _generalMixer;
    private readonly State[] _states;
    private AnimationMixerPlayable _currentBlendMixer;
    private AnimationMixerPlayable _previousBlendMixer;
    private float _transitionTime;
    private float _blendDuration;
    private bool _isTransitioning;
    private State _currentState; // Track current state to prevent redundant transitions

    public CharacterPlayablesAnimatorController(Animator animator, State[] states)
    {
        _playableGraph = PlayableGraph.Create("CharacterPlayablesAnimatorController");
        _playableOutput = AnimationPlayableOutput.Create(_playableGraph, "Output", animator);
        _states = states;
        _generalMixer = AnimationMixerPlayable.Create(_playableGraph, 2);
        _playableOutput.SetSourcePlayable(_generalMixer);
        
        _playableGraph.Play();
    }

    public void SetAnimationState(State state, int animationBlendParamValue)
    {
        var blend = state.Clips.FirstOrDefault(b => (int)b.ParamValue == animationBlendParamValue);

        if (blend == null)
        {
            return;
        }

        // Check if we're already in this state with the same blend parameters
        if (_currentState == state && _currentBlendMixer.IsValid() && _isTransitioning == false)
        {
            bool isSameBlend = true;
            for (int i = 0; i < blend.Clips.Length; i++)
            {
                var playable = (AnimationClipPlayable)_currentBlendMixer.GetInput(i);
                if (playable.GetAnimationClip() != blend.Clips[i].Clip)
                {
                    isSameBlend = false;
                    break;
                }
            }
            if (isSameBlend)
            {
                return; // No need to change anything, we're already in this state
            }
        }

        _blendDuration = state.EnterTransitionDuration;
        _transitionTime = 0f;

        // Create new blend mixer for the current state
        var newBlendMixer = AnimationMixerPlayable.Create(_playableGraph, blend.Clips.Length);

        // Set up clips in the new blend mixer
        for (int i = 0; i < blend.Clips.Length; i++)
        {
            var clip = blend.Clips[i];
            var clipPlayable = AnimationClipPlayable.Create(_playableGraph, clip.Clip);
            clipPlayable.SetSpeed(clip.Speed);

            if (!clip.Clip.isLooping)
            {
                clipPlayable.SetDuration(clip.Clip.length);
            }

            _playableGraph.Connect(clipPlayable, 0, newBlendMixer, i);
            newBlendMixer.SetInputWeight(i, 1.0f / blend.Clips.Length); // Equal weight for each clip
        }

        // If there's a previous mixer, start transition
        if (_currentBlendMixer.IsValid())
        {
            _previousBlendMixer = _currentBlendMixer;
            _isTransitioning = true;

            // Disconnect previous connections to avoid the error
            if (_generalMixer.GetInput(0).IsValid())
            {
                _playableGraph.Disconnect(_generalMixer, 0);
            }
            if (_generalMixer.GetInput(1).IsValid())
            {
                _playableGraph.Disconnect(_generalMixer, 1);
            }

            // Connect both mixers to the general mixer
            _playableGraph.Connect(_previousBlendMixer, 0, _generalMixer, 0);
            _playableGraph.Connect(newBlendMixer, 0, _generalMixer, 1);
            _generalMixer.SetInputWeight(0, 1.0f); // Previous at full weight
            _generalMixer.SetInputWeight(1, 0.0f); // New at zero weight
        }
        else
        {
            // First animation, connect directly
            if (_generalMixer.GetInput(0).IsValid())
            {
                _playableGraph.Disconnect(_generalMixer, 0);
            }
            _playableGraph.Connect(newBlendMixer, 0, _generalMixer, 0);
            _generalMixer.SetInputWeight(0, 1.0f);
        }

        _currentBlendMixer = newBlendMixer;
        _currentState = state;

        // Update the graph
        _playableGraph.Evaluate();
    }

    public void OnUpdate(float deltaTime)
    {
        if (!_isTransitioning)
            return;

        _transitionTime += deltaTime;

        float t = Mathf.Clamp01(_transitionTime / _blendDuration);
        float currentWeight = Mathf.Lerp(0f, 1f, t);
        
        _generalMixer.SetInputWeight(0, 1f - currentWeight); // Fade out previous
        _generalMixer.SetInputWeight(1, currentWeight);     // Fade in current

        if (t >= 1f)
        {
            _isTransitioning = false;
            
            // Clean up previous mixer
            if (_previousBlendMixer.IsValid())
            {
                _playableGraph.Disconnect(_generalMixer, 0);
                _previousBlendMixer.Destroy();
            }

            // Shift current mixer to first slot
            _generalMixer.SetInputWeight(0, 1.0f);
            _generalMixer.SetInputWeight(1, 0.0f);
        }

        _playableGraph.Evaluate();
    }

    public void Move(float movementX, float movementY)
    {
        
    }

    public void OnDestroy()
    {
        if (!_playableGraph.IsValid())
        {
            return;
        }
        _playableGraph.Destroy();
    }
}