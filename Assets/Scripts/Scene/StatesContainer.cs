using UnityEngine;

public class StatesContainer : MonoBehaviour
{
   [field: SerializeField] public IdleState IdleState { get; private set; }
   [field: SerializeField] public WalkState WalkState { get; private set; }
   [field: SerializeField] public RunState RunState { get; private set; }
}
