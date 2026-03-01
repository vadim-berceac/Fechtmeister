using System;
using UnityEngine;
using Object = UnityEngine.Object;

public class ProjectileSystem : InventoryBag, IItemInstancesContainer
{
    public IItemInstance[] Instances { get; set; }
    public int InstancesCount { get; set; }
    public Action<IEquppiedItemData> OnItemEquipped { get; set; }
    public Action<IEquppiedItemData> OnItemUnEquipped { get; set; }
    public StateTimer StateTimer { get; set; }

    private readonly CharacterCore _characterCore;
    private ProjectileController _projectileController;

    public bool IsProjectileLoaded { get; set; }

    public ProjectileSystem(int instancesCount, CharacterCore characterCore) : base(1)
    {
        InstancesCount = instancesCount;
        _characterCore = characterCore;
        Instances = new IItemInstance[instancesCount];
    }

    public bool HasProjectiles()
    {
        if (Instances[0]?.EquppiedItemData == null) return false;
        var cell = GetCell(Instances[0].EquppiedItemData);
        return cell != null && cell.Quantity > 0;
    }

    public void SetProjectileLoaded(bool value)
    {
        IsProjectileLoaded = value;
    }

    public void TakeProjectile(WeaponData weaponData)
    {
        var projectileData = (ProjectileData)Instances[0].EquppiedItemData;

        var projectileObject = Object.Instantiate(projectileData.EquippedModelPrefab);
        _projectileController = projectileObject.AddComponent<ProjectileController>();
        _projectileController.SetParams(projectileData, weaponData);

        var boneData = projectileData.BoneData[0];
        _characterCore.GraphCore.Animator.AttachTransformSource(
            projectileObject.transform,
            boneData.BonesType,
            boneData.Position,
            boneData.Rotation.eulerAngles,
            boneData.Scale,
            boneData.Active,
            boneData.UseBone
        );

        switch (weaponData.WeaponParams.WastingCharges.WastingCharges)
        {
            case WastingCharges.None:
                break;
            case WastingCharges.Projectiles:
                RemoveItem(projectileData, weaponData.WeaponParams.WastingCharges.ChargesPerUse);
                break;
            case WastingCharges.Mana:
                break;
            case WastingCharges.Health:
                break;
        }
    }

    public void ReturnProjectile(WeaponData weaponData)
    {
        if (_projectileController == null) return;

        var projectileData = (ProjectileData)Instances[0].EquppiedItemData;

        switch (weaponData.WeaponParams.WastingCharges.WastingCharges)
        {
            case WastingCharges.None:
                break;
            case WastingCharges.Projectiles:
                AddItem(projectileData, weaponData.WeaponParams.WastingCharges.ChargesPerUse);
                break;
            case WastingCharges.Mana:
                break;
            case WastingCharges.Health:
                break;
        }

        Object.Destroy(_projectileController.gameObject);
        _projectileController = null;
    }

    public void Shot()
    {
        if (_projectileController == null) return;

        _projectileController.transform.SetParent(null);
        _projectileController.Launch(
            _characterCore.CapsuleCollider,
            _characterCore.AimTargetTransform
        );

        SetProjectileLoaded(false);
        _projectileController = null;

        if (!HasProjectiles())
            this.DestroyInstance(Instances[0].EquppiedItemData);
    }
}