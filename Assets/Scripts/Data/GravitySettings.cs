using UnityEngine;

[System.Serializable]
public struct GravitySettings
{
    [field: Header("Grounding settings")]
    [field: SerializeField] public Vector3 GroundOffset { get; private set; }
    [field: SerializeField] public float CheckSphereRadius { get; private set; }
    
    [field: Header("Falling settings")]
    [field: SerializeField] public bool ImmuneToFallDamage { get; private set; }
    [field: SerializeField] public float FallDamageThreshold { get; private set; }
}
