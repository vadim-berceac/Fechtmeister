using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthComponent : MonoBehaviour, IDamageable
{
    [field: SerializeField] private float HitReactionTime { get; set; } = 0.5f;
    
    public Transform DamagedObject { get; set; }
    public float MaxHealth { get; set; }
    public float CurrentHealth { get; set; }
    public float HitReactionThresholdPercentage { get; set; }
    
    public float CurrentHealthNormalized
    {
        get => CurrentHealth / MaxHealth;
        set => CurrentHealth = Mathf.Clamp01(value) * MaxHealth;
    }
    
    private bool _isHitReactionEnabled;
    private Coroutine _hitReactionCoroutine;
    
    public bool IsHitReactionEnabled
    {
        get
        {
            if (_isHitReactionEnabled)
            {
                _isHitReactionEnabled = false;
               
                if (_hitReactionCoroutine != null)
                {
                    StopCoroutine(_hitReactionCoroutine);
                    _hitReactionCoroutine = null;
                }
                
                return true;
            }
            return false;
        }
        set => _isHitReactionEnabled = value;
    }
    
    public bool IsDestroyed { get; set; }
    public Dictionary<DamageTypes, int> DamageResistances { get; set; } = new();

    public Action<float> OnCurrentHealthChanged { get; set; }
    public Action<Transform> OnDamageAttempt { get; set; }
    public Action<bool> OnDestroyed { get;  set; }

    public void Initialize(float maxHealth, float currentHealthPercentage, float hitReactionThresholdPercentage,
        Transform damagedObject, ResistanceSettings resistanceSettings)
    {
        MaxHealth = maxHealth;
        CurrentHealth = MaxHealth / 100 * currentHealthPercentage;
        HitReactionThresholdPercentage = hitReactionThresholdPercentage;
        DamagedObject = damagedObject;
        
        SetResistance(resistanceSettings);
    }

    public void SetResistance(ResistanceSettings resistanceSettings)
    {
        DamageResistances.Clear();
        
        if (resistanceSettings.Resistances.Length <= 0)
        {
            return;
        }
        
        foreach (var r in resistanceSettings.Resistances)
        {
            DamageResistances.Add(r.DamageType, r.ResistancePercentage);
        }
    }

    public bool CheckForResistance(float damageValue, DamageTypes damageType)
    {
        if (DamageResistances == null || DamageResistances.Count == 0)
        {
            return false;
        }
        
        return DamageResistances.TryGetValue(damageType, out var damageResistance) && damageResistance >= damageValue;
    }

    public void Damage(float damage, DamageTypes damageType, Transform source = null)
    {
        if (source != null)
        {
            OnDamageAttempt?.Invoke(source);
        }
        
        if (CheckForResistance(damage, damageType))
        {
            return;
        }
        
        CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
        
        if ((damage / MaxHealth) * 100f > HitReactionThresholdPercentage)
        {
            EnableHitReaction(true);
        }

        if (CurrentHealth <= 0)
        {
            SetDestroyed(true);
        }
        
        OnCurrentHealthChanged?.Invoke(CurrentHealth);
        
        Debug.Log($"DamageValue {damage} to {CurrentHealth} / {MaxHealth}, by {damageType}");
    }

    public void Heal(float heal)
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

    private void EnableHitReaction(bool enable)
    {
        if (enable)
        {
            // Останавливаем предыдущую корутину, если была
            if (_hitReactionCoroutine != null)
            {
                StopCoroutine(_hitReactionCoroutine);
            }
            
            _isHitReactionEnabled = true;
            _hitReactionCoroutine = StartCoroutine(ResetHitReactionAfterDelay());
            Debug.Log("[Health] Hit reaction enabled");
        }
        else
        {
            _isHitReactionEnabled = false;
            
            if (_hitReactionCoroutine != null)
            {
                StopCoroutine(_hitReactionCoroutine);
                _hitReactionCoroutine = null;
            }
        }
    }

    private IEnumerator ResetHitReactionAfterDelay()
    {
        yield return new WaitForSeconds(HitReactionTime);
        
        _isHitReactionEnabled = false;
        _hitReactionCoroutine = null;
        
        Debug.Log("[Health] Hit reaction auto-reset after timeout");
    }

    public void SetDestroyed(bool destroyed)
    {
        IsDestroyed = destroyed;
        OnDestroyed?.Invoke(IsDestroyed);
    }

    private void OnDisable()
    {
        if (_hitReactionCoroutine != null)
        {
            StopCoroutine(_hitReactionCoroutine);
            _hitReactionCoroutine = null;
        }
    }
}