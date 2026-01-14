using UnityEngine;

public class HitBodyPart : MonoBehaviour
{
    [field: SerializeField] public HitBodyPartSettings HitBodyPartSettings { get; set; }
    
    public void Damage(float damage, DamageTypes damageType, Transform source = null)
    {
        HitBodyPartSettings.Health.Damage(damage * HitBodyPartSettings.DamageMultiplier, damageType, source);
    }
}

[System.Serializable]
public struct HitBodyPartSettings
{
    [field: SerializeField] public HealthComponent Health { get; private set; }
    [field: SerializeField] public Collider Col { get; private set; }
    [field: SerializeField] public Transform Transform { get; private set; }
    [field: SerializeField] public float DamageMultiplier { get; private set; }
}
