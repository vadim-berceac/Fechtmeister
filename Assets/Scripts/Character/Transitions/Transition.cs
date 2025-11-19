using System;

public readonly struct Transition<T>
{
    private readonly Func<T, bool> _condition;
    private readonly string _targetStateName;
    public string TargetStateName => _targetStateName;

    public Transition(Func<T, bool> condition, string targetState)
    {
        _condition = condition;
        _targetStateName = targetState;
    }
    
    public bool Check(T target)
    {
        return _condition(target);
    }
}
