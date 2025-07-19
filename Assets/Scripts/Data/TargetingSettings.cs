using UnityEngine;

[System.Serializable]
public struct TargetingSettings
{
    [field: SerializeField] public Targeting ItemTargeting { get; set; }
    [field: SerializeField] public Targeting CharacterTargeting { get; set; }
}
