using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class CharacterPlayablesAnimatorController
{
    private PlayableGraph _playableGraph;
    private readonly AnimationPlayableOutput _animationPlayableOutput;
    private readonly AnimationMixerPlayable _generalMixer;
    private readonly List<AnimationMixerPlayable> _statesMixers;
    private readonly List<AnimationMixerPlayable> _clipsMixers;
    private readonly Animator _animator;
    private readonly State[] _states;

    public CharacterPlayablesAnimatorController(Animator animator, State[] states)
    {
        _animator = animator;
        _states = states;
        
        _playableGraph = PlayableGraph.Create("CharacterPlayable");
        _animationPlayableOutput = AnimationPlayableOutput.Create(_playableGraph, "Animation", _animator);
        _generalMixer = AnimationMixerPlayable.Create(_playableGraph, _states.Length);
        _animationPlayableOutput.SetSourcePlayable(_generalMixer);
        
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
                    // Устанавливаем вес для каждого клипа в clipMixer (по умолчанию 1 для первого клипа)
                    clipMixer.SetInputWeight(k, k == 0 ? 1f : 0f);
                }

                _playableGraph.Connect(clipMixer, 0, stateMixer, j);
                _clipsMixers.Add(clipMixer);
                // Устанавливаем вес для clipMixer в stateMixer (по умолчанию 1 для первой группы клипов)
                stateMixer.SetInputWeight(j, j == 0 ? 1f : 0f);
            }

            _playableGraph.Connect(stateMixer, 0, _generalMixer, i);
            _statesMixers.Add(stateMixer);
            // Устанавливаем вес для stateMixer в generalMixer (по умолчанию 1 для первого состояния)
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

        bool stateFound = false;
        int clipMixerIndex = 0; // Счетчик для доступа к _clipsMixers
        for (var i = 0; i < _states.Length; i++)
        {
            if (_states[i].name == stateName)
            {
                _generalMixer.SetInputWeight(i, 1f);
                stateFound = true;
                // Активируем первую группу клипов и первый клип в выбранном состоянии
                var stateMixer = _statesMixers[i];
                for (int j = 0; j < stateMixer.GetInputCount(); j++)
                {
                    stateMixer.SetInputWeight(j, j == 0 ? 1f : 0f); // Первая группа клипов активна
                    if (clipMixerIndex < _clipsMixers.Count)
                    {
                        var clipMixer = _clipsMixers[clipMixerIndex];
                        for (int k = 0; k < clipMixer.GetInputCount(); k++)
                        {
                            clipMixer.SetInputWeight(k, k == 0 ? 1f : 0f); // Первый клип активен
                        }
                        clipMixerIndex++;
                    }
                }
                Debug.Log($"Activated state: {_states[i].name}, Input: {_generalMixer.GetInput(i)}");
            }
            else
            {
                _generalMixer.SetInputWeight(i, 0f);
                // Отключаем все группы клипов в неактивных состояниях
                var stateMixer = _statesMixers[i];
                for (int j = 0; j < stateMixer.GetInputCount(); j++)
                {
                    stateMixer.SetInputWeight(j, 0f);
                    if (clipMixerIndex < _clipsMixers.Count)
                    {
                        var clipMixer = _clipsMixers[clipMixerIndex];
                        for (int k = 0; k < clipMixer.GetInputCount(); k++)
                        {
                            clipMixer.SetInputWeight(k, 0f);
                        }
                        clipMixerIndex++;
                    }
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

    public void OnDestroy()
    {
        if (!_playableGraph.IsValid())
        {
            return;
        }
        _playableGraph.Destroy();
    }
}
