using UnityEngine;

public class WeaponInstance : IItemInstance
{
    public IItemData ItemData { get; set; }
    public CharacterBonesContainer CharacterBonesContainer { get; set; }
    public Transform Instance { get; set; }
    public Transform IKBoneTransform { get; set; }
    public WeaponDamageComponent DamageComponent { get; set; }
    public Transform[] ItemDecorations { get; set; }

    private readonly Collider _owner;
    
    public WeaponInstance(ref IItemData itemData, CharacterBonesContainer characterBonesContainer, Collider owner)
    {
        ItemData = itemData;
        itemData = null;
        CharacterBonesContainer = characterBonesContainer;
        _owner = owner;
        
        CreateInstance();
        this.CreateDecorations();
    }
    
    public void CreateInstance()
    {
        if (ItemData.EquippedModelPrefab == null)
        {
            return;
        }
        if (ItemData.BoneData == null)
        {
            return;
        }
        
        Instance = Object.Instantiate(ItemData.EquippedModelPrefab).transform;
        
        IKBoneTransform = this.TryToFindIKBoneTransform();

        DamageComponent = Instance.gameObject.AddComponent<WeaponDamageComponent>();
        DamageComponent.SetOwnerCollider(_owner);
        
        this.AttachToBone(Instance, ItemData.BoneData[1]);
    }
}
