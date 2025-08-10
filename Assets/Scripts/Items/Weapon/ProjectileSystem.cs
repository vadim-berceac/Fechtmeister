using UnityEngine;

public class ProjectileSystem
{
    public bool IsProjectileLoaded { get; set; }
    
    public ProjectileSystem()
    {
        
    }

    public void SetProjectileLoaded(bool value)
    {
        IsProjectileLoaded = value;
    }
}
