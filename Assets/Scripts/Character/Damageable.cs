using System;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    public Transform DamagedObject { get;  set; }
    public float MaxHealth { get;  set; }
    public float CurrentHealth { get;  set; }
    public float HitReactionThresholdPercentage { get;  set; }
    public bool IsHitReactionEnabled { get;  set; }
    public bool IsDestroyed { get;  set; }
    public Dictionary<DamageTypes, int> DamageResistances { get;  set; }

    public Action<float> OnCurrentHealthChanged { get;  set; }
    public Action OnDamageAttempt { get;  set; }

    public void Initialize(float maxHealth, float currentHealthPercentage, float hitReactionThresholdPercentage,
        Transform damagedObject, ResistanceSettings resistanceSettings);
    public bool CheckForResistance(float damageValue, DamageTypes damageType);
    public void Damage(float damage, DamageTypes damageType);
    public void Heal(float heal);
    public void SetHitReactionThreshold(float hitReactionThreshold);
    public void SetMaxHealth(float newMaxHealth);
    public void EnableHitReaction(bool enable);
    public void SetDestroyed(bool destroyed);
}
