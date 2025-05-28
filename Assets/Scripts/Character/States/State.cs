using UnityEngine;

public abstract class State : ScriptableObject
{
    public abstract void EnterState(CharacterCore character);
    public abstract void UpdateState(CharacterCore character);
    public abstract void CheckSwitch(CharacterCore character);
    public abstract void ExitState(CharacterCore character);
}
