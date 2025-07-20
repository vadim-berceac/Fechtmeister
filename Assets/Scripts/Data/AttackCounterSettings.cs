
using UnityEngine;

[System.Serializable]
public struct AttackCounterSettings
{
    [field: SerializeField] public int AttacksCount { get; private set; }
    [field: SerializeField] public float AttacksResetDelay { get; private set; } 
}
