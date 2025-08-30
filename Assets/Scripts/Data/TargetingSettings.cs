using UnityEngine;

[System.Serializable]
public struct TargetingSettings
{
    [field: SerializeField] public ItemTargeting ItemTargeting { get; set; }
    [field: SerializeField] public CharacterTargeting CharacterTargeting { get; set; }
}
