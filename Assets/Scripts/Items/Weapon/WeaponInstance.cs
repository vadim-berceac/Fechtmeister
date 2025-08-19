using UnityEngine;

public class WeaponInstance : IItemInstance
{
    public IItemData ItemData { get; set; }
    public CharacterBonesContainer CharacterBonesContainer { get; set; }
    public Transform Instance { get; set; }
    public Transform IKBoneTransform { get; set; }
    public WeaponDamageComponent DamageComponent { get; set; }
    public Transform[] ItemDecorations { get; set; }

    public WeaponInstance(ref IItemData itemData, CharacterBonesContainer characterBonesContainer)
    {
        ItemData = itemData;
        itemData = null;
        CharacterBonesContainer = characterBonesContainer;
        
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
        
        this.AttachToBone(Instance, ItemData.BoneData[1]);
    }
}
