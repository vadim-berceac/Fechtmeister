using System.Linq;
using Unity.Burst;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

[BurstCompile]
public class CharacterPlayablesAnimatorController
{
    public bool IsTransitioning {get; private set;}
    private PlayableGraph _playableGraph;
    private readonly AnimationMixerPlayable _generalMixer;
    private AnimationMixerPlayable _currentBlendMixer;
    private AnimationMixerPlayable _previousBlendMixer;
    private AnimationBlendConfig _currentBlendConfig;
    private State _currentState;
    private float _transitionTime;
    private float _blendDuration;
    private int _targetClipIndex = -1; 
    private float _clipTransitionTime;
    private const float ClipTransitionDuration = 0.05f; 
    private bool _isClipTransitioning;
    private float[] _targetWeights; 
    private float[] _currentWeights; 
    private float _moveTransitionTime; 
    private bool _isMoveTransitioning;
    private int _currentSlot; 
    private bool _actionTimeReached ; 
    private float _previousNormalizedTime; 

    public CharacterPlayablesAnimatorController(Animator animator)
    {
        _playableGraph = PlayableGraph.Create("CharacterPlayablesAnimatorController");
        var playableOutput = AnimationPlayableOutput.Create(_playableGraph, "Output", animator);
        _generalMixer = AnimationMixerPlayable.Create(_playableGraph, 2);
        playableOutput.SetSourcePlayable(_generalMixer);
        
        _playableGraph.Play();
    }

    public void SetAnimationState(State state, int animationBlendParamValue)
    {
        _currentBlendConfig = state.Clips.FirstOrDefault(b => (int)b.ParamValue == animationBlendParamValue);
        if (_currentBlendConfig == null)
        {
            Debug.LogWarning($"Blend config not found for param value: {animationBlendParamValue}");
            return;
        }
       
        if (_currentState == state && _currentBlendMixer.IsValid() && !IsTransitioning)
        {
            var isSameBlend = true;
            for (var i = 0; i < _currentBlendConfig.Clips.Length; i++)
            {
                var playable = (AnimationClipPlayable)_currentBlendMixer.GetInput(i);
                if (playable.GetAnimationClip() == _currentBlendConfig.Clips[i].Clip)
                {
                    continue;
                }
                isSameBlend = false;
                break;
            }
            if (isSameBlend)
            {
                return; 
            }
        }

        _blendDuration = state.EnterTransitionDuration;
        _transitionTime = 0f;
       
        var newBlendMixer = AnimationMixerPlayable.Create(_playableGraph, _currentBlendConfig.Clips.Length);
        
        for (var i = 0; i < _currentBlendConfig.Clips.Length; i++)
        {
            var clip = _currentBlendConfig.Clips[i];
            var clipPlayable = AnimationClipPlayable.Create(_playableGraph, clip.Clip);
            clipPlayable.SetSpeed(clip.Speed);

            if (!clip.Clip.isLooping)
            {
                clipPlayable.SetDuration(clip.Clip.length);
            }

            _playableGraph.Connect(clipPlayable, 0, newBlendMixer, i);
            newBlendMixer.SetInputWeight(i, 0.0f); 
        }

        var initialClipIndex = 0;
        for (var i = 0; i < _currentBlendConfig.Clips.Length; i++)
        {
            if ((int)_currentBlendConfig.Clips[i].ParamValue == animationBlendParamValue)
            {
                initialClipIndex = i;
                break;
            }
        }
        newBlendMixer.SetInputWeight(initialClipIndex, 1.0f); 

        _targetWeights = new float[_currentBlendConfig.Clips.Length];
        _currentWeights = new float[_currentBlendConfig.Clips.Length];
        for (var i = 0; i < _currentBlendConfig.Clips.Length; i++)
        {
            _currentWeights[i] = newBlendMixer.GetInputWeight(i);
            _targetWeights[i] = newBlendMixer.GetInputWeight(i); 
        }

        if (_currentBlendMixer.IsValid())
        {
            _previousBlendMixer = _currentBlendMixer;
            var previousSlot = _currentSlot;
            var newSlot = 1 - _currentSlot; 

            if (_generalMixer.GetInput(newSlot).IsValid())
            {
                _playableGraph.Disconnect(_generalMixer, newSlot);
            }

            _playableGraph.Connect(newBlendMixer, 0, _generalMixer, newSlot);

            _generalMixer.SetInputWeight(previousSlot, 1.0f);
            _generalMixer.SetInputWeight(newSlot, 0.0f);

            IsTransitioning = true;
            _currentSlot = newSlot;
        }
        else
        {
            if (_generalMixer.GetInput(0).IsValid())
            {
                _playableGraph.Disconnect(_generalMixer, 0);
            }
            _playableGraph.Connect(newBlendMixer, 0, _generalMixer, 0);
            _generalMixer.SetInputWeight(0, 1.0f);
            _generalMixer.SetInputWeight(1, 0.0f);
            _currentSlot = 0;
        }

        _currentBlendMixer = newBlendMixer;
        _currentState = state;

        _isMoveTransitioning = false;
        _moveTransitionTime = 0f;

        _actionTimeReached = false;
        _previousNormalizedTime = 0f;

        _playableGraph.Evaluate();
    }

    public void SetAnimationStateClip(int animationBlendParamValue)
    {
        if (_currentBlendConfig == null || !_currentBlendMixer.IsValid())
        {
            return;
        }
        
        var targetClipIndex = -1;
        for (var i = 0; i < _currentBlendConfig.Clips.Length; i++)
        {
            if ((int)_currentBlendConfig.Clips[i].ParamValue == animationBlendParamValue)
            {
                targetClipIndex = i;
                break;
            }
        }

        if (targetClipIndex == -1)
        {
            return; 
        }
        
        if (_isClipTransitioning && _targetClipIndex == targetClipIndex)
        {
            return;
        }

        _isClipTransitioning = true;
        _clipTransitionTime = 0f;
        _targetClipIndex = targetClipIndex;

        for (var i = 0; i < _currentBlendMixer.GetInputCount(); i++)
        {
            _currentWeights[i] = _currentBlendMixer.GetInputWeight(i);
        }

        for (var i = 0; i < _currentBlendMixer.GetInputCount(); i++)
        {
            _targetWeights[i] = i == targetClipIndex ? 1.0f : 0.0f;
        }

        _actionTimeReached = false;
        _previousNormalizedTime = 0f;

        _playableGraph.Evaluate();
    }

    /// <summary>
    /// Доделать смешивание
    /// </summary>
    public void BlendCurrentAnimationStateClips(float byValue)
    {
        //TODO
        //смешать все анимационные клипы, содержащиеся в текущем _currentBlendConfig
        // по входящему параметру byValue
    }

    public void Move(float movementX, float movementY)
    {
        if (_currentBlendConfig == null || !_currentBlendMixer.IsValid())
        {
            Debug.LogWarning("Move: No valid blend config or mixer available.");
            return;
        }

        for (var i = 0; i < _currentBlendMixer.GetInputCount(); i++)
        {
            _currentWeights[i] = _currentBlendMixer.GetInputWeight(i);
        }

        var paramVector = new Vector2(movementX, movementY);
        var totalWeight = 0f;
        var maxWeight = 0f;
        
        for (var i = 0; i < _currentBlendConfig.Clips.Length; i++)
        {
            var clip = _currentBlendConfig.Clips[i];
            var distance = (paramVector - clip.ParamPosition).magnitude;
            if (distance == 0f)
            {
                _targetWeights[i] = float.MaxValue; 
            }
            else
            {
                _targetWeights[i] = 1f / Mathf.Pow(distance, 4f); 
            }
            totalWeight += _targetWeights[i];
            if (_targetWeights[i] > maxWeight)
            {
                maxWeight = _targetWeights[i];
            }
        }
        
        var threshold = 0.05f * maxWeight;
        totalWeight = 0f;
        for (var i = 0; i < _targetWeights.Length; i++)
        {
            if (_targetWeights[i] < threshold)
            {
                _targetWeights[i] = 0f;
            }
            totalWeight += _targetWeights[i];
        }

        if (totalWeight > 0f)
        {
            for (var i = 0; i < _targetWeights.Length; i++)
            {
                _targetWeights[i] = _targetWeights[i] / totalWeight;
            }
        }
        else
        {
            for (var i = 0; i < _targetWeights.Length; i++)
            {
                _targetWeights[i] = 1f / _targetWeights.Length;
            }
        }

        _isMoveTransitioning = true;
        _moveTransitionTime = 0f;

        _actionTimeReached = false;
        _previousNormalizedTime = 0f;

        _playableGraph.Evaluate();
    }

    public void OnUpdate(float deltaTime)
    {
        UpdateActionTimeFlag();
        
        if (_isClipTransitioning)
        {
            _clipTransitionTime += deltaTime;
            var t = Mathf.Clamp01(_clipTransitionTime / ClipTransitionDuration);
            var sumWeights = 0f;
            for (var i = 0; i < _currentBlendMixer.GetInputCount(); i++)
            {
                var weight = Mathf.Lerp(_currentWeights[i], _targetWeights[i], t);
                _currentBlendMixer.SetInputWeight(i, weight);
                sumWeights += weight;
            }
            if (Mathf.Abs(sumWeights - 1f) > 0.001f)
            {
                for (var i = 0; i < _currentBlendMixer.GetInputCount(); i++)
                {
                    var weight = _currentBlendMixer.GetInputWeight(i) / sumWeights;
                    _currentBlendMixer.SetInputWeight(i, weight);
                }
            }

            if (t >= 1.0f)
            {
                _isClipTransitioning = false;
                sumWeights = 0f;
                for (var i = 0; i < _currentBlendMixer.GetInputCount(); i++)
                {
                    _currentBlendMixer.SetInputWeight(i, _targetWeights[i]);
                    _currentWeights[i] = _targetWeights[i];
                    sumWeights += _targetWeights[i];
                }
                if (Mathf.Abs(sumWeights - 1f) > 0.001f)
                {
                    for (var i = 0; i < _currentBlendMixer.GetInputCount(); i++)
                    {
                        _currentBlendMixer.SetInputWeight(i, _targetWeights[i] / sumWeights);
                        _currentWeights[i] = _targetWeights[i] / sumWeights;
                    }
                }
            }

            _playableGraph.Evaluate();
        }
        if (_isMoveTransitioning)
        {
            _moveTransitionTime += deltaTime;
            var t = Mathf.Clamp01(_moveTransitionTime / ClipTransitionDuration);
            var sumWeights = 0f;
            for (var i = 0; i < _currentBlendMixer.GetInputCount(); i++)
            {
                var weight = Mathf.Lerp(_currentWeights[i], _targetWeights[i], t);
                _currentBlendMixer.SetInputWeight(i, weight);
                sumWeights += weight;
            }
            if (Mathf.Abs(sumWeights - 1f) > 0.001f)
            {
                for (var i = 0; i < _currentBlendMixer.GetInputCount(); i++)
                {
                    var weight = _currentBlendMixer.GetInputWeight(i) / sumWeights;
                    _currentBlendMixer.SetInputWeight(i, weight);
                }
            }

            if (t >= 1.0f)
            {
                _isMoveTransitioning = false;
                sumWeights = 0f;
                for (var i = 0; i < _currentBlendMixer.GetInputCount(); i++)
                {
                    _currentBlendMixer.SetInputWeight(i, _targetWeights[i]);
                    _currentWeights[i] = _targetWeights[i]; 
                    sumWeights += _targetWeights[i];
                }

                if (Mathf.Abs(sumWeights - 1f) > 0.001f)
                {
                    for (var i = 0; i < _currentBlendMixer.GetInputCount(); i++)
                    {
                        _currentBlendMixer.SetInputWeight(i, _targetWeights[i] / sumWeights);
                        _currentWeights[i] = _targetWeights[i] / sumWeights;
                    }
                }
            }

            _playableGraph.Evaluate();
        }
        if (!IsTransitioning)
            return;

        _transitionTime += deltaTime;

        var stateT = Mathf.Clamp01(_transitionTime / _blendDuration);
        var currentWeight = Mathf.Lerp(0f, 1f, stateT);
        
        var previousSlot = 1 - _currentSlot;
        _generalMixer.SetInputWeight(previousSlot, 1f - currentWeight); 
        _generalMixer.SetInputWeight(_currentSlot, currentWeight);   
        if (stateT >= 1f)
        {
            IsTransitioning = false;
            
            if (_previousBlendMixer.IsValid())
            {
                _playableGraph.Disconnect(_generalMixer, previousSlot);
                _previousBlendMixer.Destroy();
                _previousBlendMixer = default; 
            }
            _generalMixer.SetInputWeight(_currentSlot, 1.0f);
            _generalMixer.SetInputWeight(1 - _currentSlot, 0.0f);
            if (!_generalMixer.GetInput(_currentSlot).IsValid())
            {
                Debug.LogError($"Current slot {_currentSlot} is invalid after transition!");
            }
            else
            {
                var sumWeights = 0f;
                for (var i = 0; i < _currentBlendMixer.GetInputCount(); i++)
                {
                    sumWeights += _currentBlendMixer.GetInputWeight(i);
                }
                if (Mathf.Abs(sumWeights - 1f) > 0.001f)
                {
                    Debug.LogWarning($"Invalid weights in _currentBlendMixer: sum={sumWeights}. Normalizing...");
                    for (var i = 0; i < _currentBlendMixer.GetInputCount(); i++)
                    {
                        var weight = _currentBlendMixer.GetInputWeight(i) / sumWeights;
                        _currentBlendMixer.SetInputWeight(i, weight);
                        _currentWeights[i] = weight;
                        _targetWeights[i] = weight;
                    }
                }
            }
        }

        _playableGraph.Evaluate();
    }

    private void UpdateActionTimeFlag()
    {
        if (!_currentBlendMixer.IsValid() || _currentBlendConfig == null)
        {
            _actionTimeReached = false;
            _previousNormalizedTime = 0f;
            return;
        }
       
        var maxWeight = 0f;
        var activeClipIndex = -1;
        for (var i = 0; i < _currentBlendMixer.GetInputCount(); i++)
        {
            var weight = _currentBlendMixer.GetInputWeight(i);
            if (weight <= maxWeight)
            {
                continue;
            }
            maxWeight = weight;
            activeClipIndex = i;
        }

        if (activeClipIndex == -1 || maxWeight <= 0f)
        {
            _actionTimeReached = false;
            _previousNormalizedTime = 0f;
            return;
        }
        var clipPlayable = (AnimationClipPlayable)_currentBlendMixer.GetInput(activeClipIndex);
        if (!clipPlayable.IsValid())
        {
            _actionTimeReached = false;
            _previousNormalizedTime = 0f;
            return;
        }

        var blendClip = _currentBlendConfig.Clips[activeClipIndex];
        var clip = blendClip.Clip;
        var actionTime = blendClip.ActionTime; 
        
        var currentNormalized = GetCurrentClipNormalizedTime();
        
        if (!_actionTimeReached && currentNormalized >= actionTime)
        {
            _actionTimeReached = true;
        }

        if (IsCurrentClipFinished())
        {
            _actionTimeReached = false; 
        }
        else
        {
            if (clip.isLooping)
            {
                if (currentNormalized < _previousNormalizedTime)
                {
                    _actionTimeReached = false; 
                }
            }
            if (!_actionTimeReached && currentNormalized >= actionTime)
            {
                _actionTimeReached = true;
            }
        }

        _previousNormalizedTime = currentNormalized;
    }

    public bool HasReachedActionTime()
    {
        return _actionTimeReached;
    }

    public void ResetActionTimeFlag()
    {
        _actionTimeReached = false;
    }

    public bool IsCurrentClipFinished()
    {
        if (!_currentBlendMixer.IsValid() || _currentBlendConfig == null)
        {
            return false; 
        }

        var maxWeight = 0f;
        var activeClipIndex = -1;
        for (var i = 0; i < _currentBlendMixer.GetInputCount(); i++)
        {
            var weight = _currentBlendMixer.GetInputWeight(i);
            if (weight <= maxWeight)
            {
               continue;
            }
            maxWeight = weight;
            activeClipIndex = i;
        }

        if (activeClipIndex == -1 || maxWeight <= 0f)
        {
            return false; 
        }
       
        var clipPlayable = (AnimationClipPlayable)_currentBlendMixer.GetInput(activeClipIndex);
        if (!clipPlayable.IsValid())
        {
            return false;
        }

        var clip = _currentBlendConfig.Clips[activeClipIndex].Clip;
        
        if (clip.isLooping)
        {
            return false;
        }

        var currentTime = clipPlayable.GetTime();
        var duration = clipPlayable.GetDuration();
        return currentTime >= duration;
    }

    public float GetCurrentClipNormalizedTime()
    {
        if (!_currentBlendMixer.IsValid() || _currentBlendConfig == null)
        {
            return 0f; 
        }

        var maxWeight = 0f;
        var activeClipIndex = -1;
        for (var i = 0; i < _currentBlendMixer.GetInputCount(); i++)
        {
            var weight = _currentBlendMixer.GetInputWeight(i);
            if (weight <= maxWeight)
            {
               continue;
            }
            maxWeight = weight;
            activeClipIndex = i;
        }

        if (activeClipIndex == -1 || maxWeight <= 0f)
        {
            return 0f;
        }
      
        var clipPlayable = (AnimationClipPlayable)_currentBlendMixer.GetInput(activeClipIndex);
        if (!clipPlayable.IsValid())
        {
            return 0f; 
        }

        var clip = _currentBlendConfig.Clips[activeClipIndex].Clip;
        var currentTime = clipPlayable.GetTime();
        var duration = clipPlayable.GetDuration();

        if (duration <= 0.0)
        {
            return 0f; 
        }

        float normalizedTime;
        if (clip.isLooping)
        {
            normalizedTime = (float)((currentTime % duration) / duration);
        }
        else
        {
            normalizedTime = Mathf.Clamp01((float)(currentTime / duration));
        }

        return normalizedTime;
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