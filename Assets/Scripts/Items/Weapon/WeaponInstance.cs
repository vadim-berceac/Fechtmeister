using UnityEngine;

public class WeaponInstance : IItemInstance
{
    public IItemData ItemData { get; set; }
    public CharacterBonesContainer CharacterBonesContainer { get; set; }
    public Transform Instance { get; set; }
    public Transform IKBoneTransform { get; set; }
    public Transform[] ItemDecorations { get; set; }
    public IItemControlComponent ItemControlComponent { get; set; }

    private readonly Collider _owner;
    private readonly SceneCharacterContainer _sceneCharacterContainer;
    
    public WeaponInstance(ref IItemData itemData, CharacterBonesContainer characterBonesContainer, Collider owner,
        SceneCharacterContainer sceneCharacterContainer)
    {
        ItemData = itemData;
        itemData = null;
        CharacterBonesContainer = characterBonesContainer;
        _owner = owner;
        _sceneCharacterContainer = sceneCharacterContainer;
        
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

        ItemControlComponent = new WeaponController(_owner, (WeaponData)ItemData, _sceneCharacterContainer);
        
        this.AttachToBone(Instance, ItemData.BoneData[1]);
    }

    public void ResetAction()
    {
        ItemControlComponent.ResetAction();
    }
}
