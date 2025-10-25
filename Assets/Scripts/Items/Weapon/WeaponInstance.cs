using UnityEngine;

public class WeaponInstance : IItemInstance
{
    public IItemData ItemData { get; set; }
    public CharacterBonesContainer CharacterBonesContainer { get; set; }
    public Transform Instance { get; set; }
    public Transform IKBoneTransform { get; set; }
    public Transform[] ItemDecorations { get; set; }
    public IItemControlComponent ItemControlComponent { get; set; }
    public StateTimer StateTimer { get; set; }

    private readonly Collider _owner;
    
    public WeaponInstance(ref IItemData itemData, CharacterBonesContainer characterBonesContainer, Collider owner, 
       StateTimer stateTimer)
    {
        ItemData = itemData;
        itemData = null;
        CharacterBonesContainer = characterBonesContainer;
        StateTimer = stateTimer;
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
        
        var weaponDamageComponent = Instance.gameObject.AddComponent<WeaponDamageComponent>();
        weaponDamageComponent.SetOwner(_owner);
        weaponDamageComponent.SetData(ItemData);
        weaponDamageComponent.SetStateTimer(StateTimer);

        ItemControlComponent = weaponDamageComponent;
        
        this.AttachToBone(Instance, ItemData.BoneData[1]);
    }
}
