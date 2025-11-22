using UnityEngine;

public class ShootingSystem
{
    public bool IsProjectileLoaded { get; set; }

    private readonly Collider _parent;
    private readonly Transform _characterTransform;
    private readonly Inventory _inventory;
    
    private Projectile _projectile;
    
    public ShootingSystem(Collider parent, Transform characterTransform, Inventory inventory)
    {
        _parent = parent;
        _characterTransform = characterTransform;
        _inventory = inventory;
    }

    public bool ContainsProjectile()
    {
        //проверять наличие подходящих снарядов в инвентаре
        return true;
    }

    public void SetProjectileLoaded(bool value)
    {
        IsProjectileLoaded = value;
    }

    public void TakeProjectile(GameObject projectilePrefab)
    {
        var projectileObject = Object.Instantiate(projectilePrefab);
        _projectile = projectileObject.GetComponent<Projectile>();
        
        var spawnPosition = _characterTransform.position 
                            + _characterTransform.forward 
                            + _characterTransform.TransformVector(_projectile.StartPositionOffset);

        projectileObject.transform.SetPositionAndRotation(spawnPosition, _characterTransform.rotation);
        projectileObject.transform.SetParent(_characterTransform); // тут помещать в наследники кости, которая указана в дате
        // удалять дату из инвентаря
    }

    public void ReturnProjectile()
    {
        if (_projectile == null)
        {
            return;
        }
        Object.Destroy(_projectile.gameObject);
        _projectile = null;
        //вернуть дату в инвентарь
    }

    public void Shot(Vector3 direction, int accuracy)
    {
        if (_projectile == null)
        {
            return;
        }
        _projectile.Launch(_parent, direction, accuracy);
        SetProjectileLoaded(false);
        _projectile = null;
    }
}
