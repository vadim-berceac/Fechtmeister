using Unity.Burst;
using UnityEngine;

public class AnimationLayerWeightTransition
{
    private readonly Animator _animator;
    private float _currentWeight;
    private float _targetWeight;
    private float _transitionDuration;
    private float _transitionTime;
    private int _layerIndex;
    private bool _isTransitioning;
    
    public AnimationLayerWeightTransition(Animator animator)
    {
        _animator = animator;
    }

    [BurstCompile]
    public void StartTransition(int layerIndex, float targetWeight, float duration)
    {
        if (!_animator || layerIndex < 0 || layerIndex >= _animator.layerCount)
        {
            Debug.LogWarning("Invalid Animator or layer index.");
            return;
        }

        _layerIndex = layerIndex;
        _currentWeight = _animator.GetLayerWeight(layerIndex);
        _targetWeight = Mathf.Clamp01(targetWeight); 
        _transitionDuration = Mathf.Max(duration, 0.01f); 
        _transitionTime = 0f;
        _isTransitioning = true;
    }

    [BurstCompile]
    public void UpdateTransition()
    {
        if (!_isTransitioning)
        {
            return;
        }
        
        _transitionTime += Time.deltaTime;
        
        var t = Mathf.Clamp01(_transitionTime / _transitionDuration);

        var newWeight = Mathf.Lerp(_currentWeight, _targetWeight, t);
        _animator.SetLayerWeight(_layerIndex, newWeight);
       
        if (t < 1f)
        {
            return;
        }
        _isTransitioning = false;
        _animator.SetLayerWeight(_layerIndex, _targetWeight); 
    }
}
