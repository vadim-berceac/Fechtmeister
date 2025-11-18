using UnityEngine;

public abstract class MovementState : State
{
    [field: Space(3)]
    [field: Header("Fast Attack Settings")]
    [field: SerializeField] public bool FastAttackAllowed { get; set; } = true;
    
    public override void EnterState(CharacterCore character)
    {
        base.EnterState(character);
        SetAnimationIndex(character);
    }
    
    protected override void CheckAction(CharacterCore character)
    {
        base.CheckAction(character);
        Move(character);
        CastFastAttack(character);
    }
    public override void ExitState(CharacterCore character)
    {
        base.ExitState(character);
        character.CurrentSpeed.StopUpdateLastHorizontalSpeed();
    }
    
    private void SetAnimationIndex(CharacterCore character)
    {
        var animIndex = character.CharacterInputHandler.IsWeaponDraw
            ? ((WeaponData)character.Inventory.WeaponSystem.InstanceInHands.ItemData).AnimationType
            : 0;
        character.GraphCore.FullBodyAnimatorController.SetAnimationState(this, animIndex);
    }

    private static void Move(CharacterCore character)
    {
        character.GraphCore.FullBodyAnimatorController.Move(character.CharacterInputHandler.InputX, character.CharacterInputHandler.InputY);
    }

    private void CastFastAttack(CharacterCore character)
    {
        if (!FastAttackAllowed)
        {
            return;
        }
        if (!character.CharacterInputHandler.IsWeaponDraw)
        {
            return;
        }
        if (character.CharacterInputHandler.IsAttack && !character.Inventory.WeaponSystem.WeaponInstanceIsRanged 
                                                     && character.GraphCore.UpperBodyLayerController.IsComplete())
        {
            character.SetSubState(character.StatesContainer.GetState("FastAttackSubState"));
        } 
    }
}
