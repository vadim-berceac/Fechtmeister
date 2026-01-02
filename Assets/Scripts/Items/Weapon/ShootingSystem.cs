using UnityEngine;

public class ShootingSystem
{
    public bool IsProjectileLoaded { get; set; }

    private readonly CharacterCore _characterCore;
    
    private ProjectileController _projectileController;
    
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
        _projectileController = projectileObject.AddComponent<ProjectileController>();
        _projectileController.SetParams(projectileData, weaponData);
        var boneData = projectileData.BoneData[0];
        _characterCore.GraphCore.CoreData.Animator.AttachToBone(projectileObject.transform, boneData.BonesType, boneData.Position, 
            boneData.Rotation.eulerAngles,  boneData.Scale,  boneData.Active);
    }

    public void ReturnProjectile()
    {
        if (_projectileController == null)
        {
            return;
        }
        Object.Destroy(_projectileController.gameObject);
        _projectileController = null;
    }

    public void Shot(int accuracy)
    {
        if (_projectileController == null) return;

        _projectileController.transform.SetParent(null);

        _projectileController.Launch(_characterCore.LocomotionSettings.CharacterCollider, accuracy);
        SetProjectileLoaded(false);
        _projectileController = null;
    }
}
