using UnityEngine;

public class ShootingSystem
{
    public bool IsProjectileLoaded { get; set; }

    private readonly CharacterCore _characterCore;
    
    private Projectile _projectile;
    
    public ShootingSystem(CharacterCore characterCore)
    {
       _characterCore = characterCore;
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

    public void TakeProjectile(ProjectileData projectileData, WeaponData weaponData)
    {
        var projectileObject = Object.Instantiate(projectileData.EquippedModelPrefab);
        _projectile = projectileObject.AddComponent<Projectile>();
        _projectile.SetParams(projectileData, weaponData);
        
        _characterCore.GraphCore.CoreData.Animator.AttachToBone(projectileObject.transform,
            projectileData.ShootPosition.BonesType, projectileData.ShootPosition.Position, 
            projectileData.ShootPosition.Rotation.eulerAngles,  projectileData.ShootPosition.Scale,  projectileData.ShootPosition.Active);
    }

    public void ReturnProjectile()
    {
        if (_projectile == null)
        {
            return;
        }
        Object.Destroy(_projectile.gameObject);
        _projectile = null;
    }

    public void Shot(int accuracy)
    {
        if (_projectile == null) return;

        _projectile.transform.SetParent(null);

        _projectile.Launch(_characterCore.LocomotionSettings.CharacterCollider, accuracy);
        SetProjectileLoaded(false);
        _projectile = null;
    }
}
