using System;
using UnityEngine;

public abstract class Damageable
{
    public float MaxHealth { get; private set; }
    public float CurrentHealth { get; private set; }
    public float HitReactionThresholdPercentage { get; private set; }
    public bool IsHitReactionEnabled { get; private set; }
    public bool IsDestroyed { get; private set; }
    
    public Action<float> OnCurrentHealthChanged { get; private set; }

    public Damageable(float maxHealth, float currentHealthPercentage, float hitReactionThresholdPercentage)
    {
        MaxHealth = maxHealth;
        CurrentHealth = MaxHealth / 100 * currentHealthPercentage;
        HitReactionThresholdPercentage = hitReactionThresholdPercentage;
    }
    
    public virtual void Damage(float damage)
    {
        CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
        
        OnCurrentHealthChanged?.Invoke(CurrentHealth);
        
        if (damage > MaxHealth / 100 * HitReactionThresholdPercentage)
        {
            EnableHitReaction(true);
        }

        if (CurrentHealth <= 0)
        {
            SetDestroyed(true);
        }
    }

    public virtual void Heal(float heal)
    {
        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + heal);
        
        OnCurrentHealthChanged?.Invoke(CurrentHealth);
    }

    public void SetHitReactionThreshold(float hitReactionThreshold)
    {
        HitReactionThresholdPercentage = hitReactionThreshold;
    }

    public void SetMaxHealth(float newMaxHealth)
    {
        MaxHealth = newMaxHealth;
    }

    public void EnableHitReaction(bool enable)
    {
        IsHitReactionEnabled = enable;
    }

    public void SetDestroyed(bool destroyed)
    {
        IsDestroyed = destroyed;
    }
}
