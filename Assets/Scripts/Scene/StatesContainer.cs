using UnityEngine;

public class StatesContainer : MonoBehaviour
{
   [field: SerializeField] public IdleState IdleState { get; private set; }
   [field: SerializeField] public WalkState WalkState { get; private set; }
   [field: SerializeField] public RunState RunState { get; private set; }
   [field: SerializeField] public JumpState JumpState { get; private set; }
   [field: SerializeField] public FallState FallState { get; private set; }
   [field: SerializeField] public CombatIdleState CombatIdleState { get; private set; }
   [field: SerializeField] public CombatWalkState CombatWalkState { get; private set; }
   [field: SerializeField] public CombatRunState CombatRunState { get; private set; }
   [field: SerializeField] public WeaponOnState WeaponOnState { get; private set; }
   [field: SerializeField] public WeaponOffState WeaponOffState { get; private set; }
   [field: SerializeField] public FastAttackState FastAttackState { get; private set; }
   [field: SerializeField] public TakeLootState TakeLootState { get; private set; }
}
