using UnityEngine;
using Zenject;

public class HitBodyPart : MonoBehaviour
{
    [field: SerializeField] public float DamageMultiplier { get; private set; } = 1.0f;
    public HealthComponent Health { get; private set; }
    public Transform Transform { get; private set; }

    [Inject]
    private void Construct(HealthComponent healthComponent)
    {
        Health = healthComponent;
        Transform = transform;
    }

    public void SetDamageMultiplier(float damageMultiplier)
    {
        DamageMultiplier = damageMultiplier;
    }
    
    public void Damage(float damage, DamageTypes damageType, Transform source = null)
    {
        var finalDamage = damage * DamageMultiplier;
    
        if (finalDamage <= 0)
        {
            return;
        }
    
        Health.Damage(finalDamage, damageType, source);
    }
}
