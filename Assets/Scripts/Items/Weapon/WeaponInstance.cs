using UnityEngine;

public class WeaponInstance : IItemInstance
{
    public IItemData ItemData { get; set; }
    public CharacterBonesContainer CharacterBonesContainer { get; set; }
    public Transform Instance { get; set; }

    public WeaponInstance(IItemData itemData, CharacterBonesContainer characterBonesContainer)
    {
        ItemData = itemData;
        CharacterBonesContainer = characterBonesContainer;
        
        CreateInstance();
        
        AttachToBone(CharacterBones.Type.RightHand);
    }
    
    public void CreateInstance()
    {
        if (ItemData.EquippedModelPrefab == null)
        {
            return;
        }
        Instance = Object.Instantiate(ItemData.EquippedModelPrefab).transform;
    }

    public void AttachToBone(CharacterBones.Type boneType)
    {
        var boneData = ItemData.GetBoneData(boneType);
        if (boneData == null)
        {
            Debug.LogWarning(ItemData.ItemName + " doesn't have a bone for " + boneType);
            return;
        }

        var boneTransform = CharacterBonesContainer.GetBoneTransform(boneType);
        
        Instance.parent = boneTransform.Transform;
        
        Instance.SetLocalPositionAndRotation(boneData.Position, boneData.Rotation);
        
        Instance.localScale = boneData.Scale;
    }

    public void DestroyInstance()
    {
        ItemData = null;
        CharacterBonesContainer = null;
        if (Instance == null)
        {
            return;
        }
        Instance.parent = null;
        Object.Destroy(Instance.gameObject);
    }
}
