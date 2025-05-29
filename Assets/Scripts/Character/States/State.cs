using UnityEngine;

public abstract class State : ScriptableObject
{
    [field: SerializeField] protected float enterTransitionDuration;
    [field: SerializeField] protected int animationLayer;
    public abstract void EnterState(CharacterCore character);
    public abstract void UpdateState(CharacterCore character);
    public abstract void CheckSwitch(CharacterCore character);
    public abstract void ExitState(CharacterCore character);
}
