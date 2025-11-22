using UnityEngine;

public class ShootingSystem
{
    public bool IsProjectileLoaded { get; set; }

    private readonly Collider _parent;
    private readonly Transform _characterTransform;
    private readonly Inventory _inventory;
    
    public ShootingSystem(Collider parent, Transform characterTransform, Inventory inventory)
    {
        _parent = parent;
        _characterTransform = characterTransform;
        _inventory = inventory;
    }

    public bool ContainsProjectile()
    {
        return true;
    }

    public void SetProjectileLoaded(bool value)
    {
        if (value)
        {
            Debug.Log($"Пытаюсь взять снаряд из {_inventory}");
        }
        IsProjectileLoaded = value;
    }

    public void Shot (GameObject projectilePrefab, Vector3 direction, int accuracy)
    {
        var projectileObject = Object.Instantiate(projectilePrefab);
        var projectile = projectileObject.GetComponent<Projectile>();

        projectile.Launch(_parent, direction, accuracy);

        var spawnPosition = _characterTransform.position 
                            + _characterTransform.forward 
                            + _characterTransform.TransformVector(projectile.StartPositionOffset);

        projectileObject.transform.SetPositionAndRotation(spawnPosition, _characterTransform.rotation);
        
        SetProjectileLoaded(false);
    }
}
