using UnityEngine;

public abstract class MovementState : State
{
    [field: Space(3)]
    [field: Header("Fast Attack Settings")]
    [field: SerializeField] public bool FastAttackAllowed { get; set; } = true;
    
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        character.SetAnimationByWeaponIndex(this);
    }
    
    protected override void CheckAction(CharacterCore character)
    {
        base.CheckAction(character);
        character.Move(character.CharacterInputHandler.InputX, character.CharacterInputHandler.InputY);
        character.CastAttack(this);
    }
    public override void ExitState(CharacterCore character)
    {
        base.ExitState(character);
        character.CurrentSpeed.StopUpdateLastHorizontalSpeed();
    }
}
