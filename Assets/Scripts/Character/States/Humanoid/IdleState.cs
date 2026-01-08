using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;

[BurstCompile]
[CreateAssetMenu(fileName = "IdleState", menuName = "States/IdleState")]
public class IdleState : MovementState
{
    private void OnEnable()
    {
        Transitions = new List<Transition<CharacterCore>>()
        {
            new (c => Mathf.Abs(c.CharacterInputHandler.InputX) > 0 ||
                      Mathf.Abs(c.CharacterInputHandler.InputY) > 0, "WalkState"),
            new(c => c.Inventory.WeaponSystem.CanDrawWeapon(), "WeaponOnState"),
            new(c => c.CharacterInputHandler.IsJump, "JumpState"),
            new(c => !c.Gravity.Grounded, "FallState"),
            new(c => c.CharacterInputHandler.IsInteract && c.TargetingSystem.HasTarget(TargetingMode.Item), "TakeLootState"),
            new(c => c.CharacterInputHandler.IsInventoryOpen, "InventoryState"),
            new(c => c.Health.IsHitReactionEnabled, "GetHitState"),
            new(c => c.Health.CurrentHealthNormalized < 0.5f, "InjuredIdleState"),
        };
    }
}
