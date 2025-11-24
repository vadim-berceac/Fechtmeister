using System;

public readonly struct Transition<T> where T : CharacterCore
{
    private readonly Func<T, bool> _customCondition;
    private readonly string _targetStateName;
    public string TargetStateName => _targetStateName;

    public Transition(Func<T, bool> customCondition, string targetState)
    {
        _customCondition = customCondition ?? throw new ArgumentNullException(nameof(customCondition));
        _targetStateName = targetState;
    }

    private static bool CheckBaseCondition(T target)
    {
        return !target.GraphCore.FullBodyAnimatorController.IsTransitioning;
    }

    public bool Check(T target)
    {
        if (!CheckBaseCondition(target))
            return false;

        return _customCondition(target);
    }
}
