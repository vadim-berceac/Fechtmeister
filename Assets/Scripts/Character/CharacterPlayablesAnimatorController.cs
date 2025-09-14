using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

/// <summary>
/// Тестовый режим
/// </summary>
public class CharacterPlayablesAnimatorController
{
    private PlayableGraph _playableGraph;
    private readonly AnimationMixerPlayable _generalMixer;
    private readonly List<AnimationMixerPlayable> _statesMixers;
    private readonly List<AnimationMixerPlayable> _clipsMixers;
    private readonly State[] _states;

    public CharacterPlayablesAnimatorController(Animator animator, State[] states)
    {
        _states = states;
        
        _playableGraph = PlayableGraph.Create("CharacterPlayable");
        var animationPlayableOutput = AnimationPlayableOutput.Create(_playableGraph, "Animation", animator);
        _generalMixer = AnimationMixerPlayable.Create(_playableGraph, _states.Length);
        animationPlayableOutput.SetSourcePlayable(_generalMixer);
        
        _statesMixers = new List<AnimationMixerPlayable>();
        _clipsMixers = new List<AnimationMixerPlayable>();
        
        for (var i = 0; i < _states.Length; i++)
        {
            var stateMixer = AnimationMixerPlayable.Create(_playableGraph, _states[i].Clips.Length);
            
            for (var j = 0; j < _states[i].Clips.Length; j++)
            {
                var clipMixer = AnimationMixerPlayable.Create(_playableGraph, _states[i].Clips[j].Clips.Length);
                
                for (var k = 0; k < _states[i].Clips[j].Clips.Length; k++)
                {
                    var clip = AnimationClipPlayable.Create(_playableGraph, _states[i].Clips[j].Clips[k].Clip);
                    if (!_states[i].Clips[j].Clips[k].Clip.isLooping)
                    {
                        clip.SetDuration(_states[i].Clips[j].Clips[k].Clip.length);
                    }
                    clip.SetSpeed(_states[i].Clips[j].Clips[k].Speed);
                    _playableGraph.Connect(clip, 0, clipMixer, k);
                    clipMixer.SetInputWeight(k, k == 0 ? 1f : 0f);
                }

                _playableGraph.Connect(clipMixer, 0, stateMixer, j);
                _clipsMixers.Add(clipMixer);
                stateMixer.SetInputWeight(j, j == 0 ? 1f : 0f);
            }

            _playableGraph.Connect(stateMixer, 0, _generalMixer, i);
            _statesMixers.Add(stateMixer);
            _generalMixer.SetInputWeight(i, i == 0 ? 1f : 0f);
        }

        _playableGraph.Play();
    }

    public void SelectAnimationState(string stateName)
    {
        if (!_playableGraph.IsValid())
        {
            Debug.LogWarning("PlayableGraph is not valid.");
            return;
        }

        var stateFound = false;
        var clipMixerIndex = 0; 
        for (var i = 0; i < _states.Length; i++)
        {
            if (_states[i].name == stateName)
            {
                _generalMixer.SetInputWeight(i, 1f);
                stateFound = true;
                var stateMixer = _statesMixers[i];
                for (var j = 0; j < stateMixer.GetInputCount(); j++)
                {
                    stateMixer.SetInputWeight(j, j == 0 ? 1f : 0f); 
                    if (clipMixerIndex >= _clipsMixers.Count)
                    {
                        continue;
                    }
                    var clipMixer = _clipsMixers[clipMixerIndex];
                    for (var k = 0; k < clipMixer.GetInputCount(); k++)
                    {
                        clipMixer.SetInputWeight(k, k == 0 ? 1f : 0f); 
                    }
                    clipMixerIndex++;
                }
            }
            else
            {
                _generalMixer.SetInputWeight(i, 0f);
                var stateMixer = _statesMixers[i];
                for (var j = 0; j < stateMixer.GetInputCount(); j++)
                {
                    stateMixer.SetInputWeight(j, 0f);
                    if (clipMixerIndex >= _clipsMixers.Count)
                    {
                       continue;
                    }
                    var clipMixer = _clipsMixers[clipMixerIndex];
                    for (var k = 0; k < clipMixer.GetInputCount(); k++)
                    {
                        clipMixer.SetInputWeight(k, 0f);
                    }
                    clipMixerIndex++;
                }
            }
        }

        if (!stateFound)
        {
            Debug.LogWarning($"State '{stateName}' not found.");
        }
    }

    public void SelectAnimationClip(int paramValue)
    {
        //ToDO
    }

    public void Move(float movementX, float movementY)
    {
        //ToDO
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
