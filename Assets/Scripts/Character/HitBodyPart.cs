using UnityEngine;
using Zenject;

public class HitBodyPart : MonoBehaviour
{
    [field: SerializeField] public float DamageMultiplier { get; private set; } = 1.0f;
    public HealthComponent Health { get; private set; }
    public Transform Transform { get; private set; }
    public bool IsActive { get; private set; }

    private GameObject _visual;

    [Inject]
    private void Construct(HealthComponent healthComponent)
    {
        Health = healthComponent;
        Transform = transform;
    }

    public void Activate(bool active)
    {
        IsActive = active;
        
        if(_visual == null) return;

        if (active)
        {
            _visual.SetActive(true);
            return;
        }
        _visual.SetActive(false);
    }

    public void SetVisual(GameObject visual)
    {
        _visual = visual;
    }

    public void SetDamageMultiplier(float damageMultiplier)
    {
        DamageMultiplier = damageMultiplier;
    }
    
    public void Damage(float damage, DamageTypes damageType, Transform source = null)
    {
        if(!IsActive) return;
        
        var finalDamage = damage * DamageMultiplier;
    
        if (finalDamage <= 0)
        {
            return;
        }
    
        Health.Damage(finalDamage, damageType, source);
    }
}
