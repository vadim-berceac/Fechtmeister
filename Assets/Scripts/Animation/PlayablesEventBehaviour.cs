using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

public class PlayablesEventBehaviour : PlayableBehaviour
{
    private int _triggerFrame; 
    private System.Action _onTrigger;
    private bool _onTriggerSet;

    private double _computedTriggerTime;
    private bool _triggered;

    public void SetEventTrigger(int frameIndex, System.Action onTrigger)
    {
        if (frameIndex < 0)
        {
            Debug.LogError("Frame index must be >= 0.");
            return;
        }
        _onTrigger = onTrigger;
        _triggerFrame = frameIndex;
        _onTriggerSet = true;
        _triggered = false; 
    }

    public override void OnGraphStart(Playable playable)
    {
        if (!_onTriggerSet)
        {
            return;
        }
        // Получаем анимационный клип из input 1 для вычисления времени по фрейму
        var animInput = (AnimationClipPlayable)playable.GetInput(1);
        if (animInput.IsValid())
        {
            var animationClip = animInput.GetAnimationClip();
            if (animationClip != null && animationClip.frameRate > 0f)
            {
                _computedTriggerTime = (double)_triggerFrame / animationClip.frameRate;
            }
            else
            {
                Debug.LogWarning("AnimationClip не найден или frameRate равен 0. Триггер не будет работать.");
                _computedTriggerTime = 0d;
            }
        }

        base.OnGraphStart(playable);
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (!_onTriggerSet)
        {
            return;
        }
        // Читаем время из input 1 (анимация как источник времени)
        var animInput = (AnimationClipPlayable)playable.GetInput(1);
        if (!animInput.IsValid()) return;

        var animTime = animInput.GetTime();

        if (!_triggered && animTime >= _computedTriggerTime)
        {
            _onTrigger?.Invoke(); // Вызываем событие/метод
            _triggered = true;
        }

        base.ProcessFrame(playable, info, playerData);
    }

    public override void OnGraphStop(Playable playable)
    {
        _onTrigger = null;
        _triggerFrame = 0;
        _onTriggerSet = false;
    }
}