using System;
using Object = UnityEngine.Object;

public class ProjectileSystem : IItemInstancesContainer
{
    public IItemInstance[] Instances { get; set; }
    public int InstancesCount { get; set; }
    public Action<IEquppiedItemData> OnItemUnEquipped { get; set; }
    public StateTimer StateTimer { get; set; }
    
    private readonly CharacterCore _characterCore;
    private ProjectileController _projectileController;
    
    public bool IsProjectileLoaded { get; set; }
    
    public ProjectileSystem(int instancesCount, CharacterCore characterCore)
    {
        InstancesCount = instancesCount;
        _characterCore = characterCore;
        Instances = new IItemInstance[instancesCount];
    }

    public bool HasProjectiles()
    {
        return Instances[0] != null && Instances[0].EquppiedItemData != null;
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
