using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InjuredIdleState", menuName = "States/InjuredIdleState")]
public class InjuredIdleState : MovementState
{
    private void OnEnable()
    {
        Transitions = new List<Transition<CharacterCore>>()
        {
            new (c => c.Health.IsDestroyed && c.IsBoss, "BossStunState"),
            new (c => c.Health.IsDestroyed && !c.IsBoss, "DeathState"),
            new(c => c.Health.IsHitReactionEnabled, "GetHitState"),
            new (c => Mathf.Abs(c.CharacterInputHandler.InputX) > 0 ||
                      Mathf.Abs(c.CharacterInputHandler.InputY) > 0, "WalkState"),
            new(character => (character.Inventory.IsWeaponOn), "CombatIdleState"),
            new(c => c.CharacterInputHandler.IsJump, "JumpState"),
            new(c => !c.Gravity.Grounded, "FallState"),
            new(c => c.CharacterInputHandler.IsInteract && c.TargetingSystem.HasTarget(TargetingMode.Item), "TakeLootState"),
            new(c => c.CharacterInputHandler.IsInventoryOpen, "InventoryState"),
            new(c => c.Health.CurrentHealthNormalized >= 0.5f, "IdleState"),
        };
    }
    
    protected override void CheckAction(CharacterCore character)
    {
        base.CheckAction(character);
        if (character.Inventory.WeaponSystem.CanDrawWeapon() 
            && character.GraphCore.UpperBodyLayerController.IsComplete())
        {
            character.SetSubState(character.StatesSet.GetState("WeaponOnSubState"));
        } 
    }
}
