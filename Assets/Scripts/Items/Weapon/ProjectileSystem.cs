using UnityEngine;

public class ProjectileSystem
{
    public bool IsProjectileLoaded { get; set; }
    //current projectile
    
    public ProjectileSystem()
    {
        
    }

    public bool ContainsProjectile()
    {
        return true;
    }

    public void SetProjectileLoaded(bool value)
    {
        IsProjectileLoaded = value;
    }

    public void Shot()
    {
        Debug.Log("Shot by current projectile");
        SetProjectileLoaded(false);
    }
}
