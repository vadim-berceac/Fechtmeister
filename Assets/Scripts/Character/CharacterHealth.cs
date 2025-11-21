
using UnityEngine;

public class CharacterHealth : Damageable
{
    public CharacterHealth(float maxHealth, float currentHealth, float hitReactionThresholdPercentage, Transform damagedObject) : base(maxHealth,
        currentHealth, hitReactionThresholdPercentage, damagedObject)
    {
        
    }
}
