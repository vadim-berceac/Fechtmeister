using UnityEngine;

public class WeaponInstance : IItemInstance
{
    public IEquppiedItemData EquppiedItemData { get; set; }
    public CharacterBonesContainer CharacterBonesContainer { get; set; }
    public Transform Instance { get; set; }
    public Transform IKBoneTransform { get; set; }
    public Transform[] ItemDecorations { get; set; }
    public IItemControlComponent ItemControlComponent { get; set; }

    private readonly Collider _owner;
    private readonly SceneCharacterContainer _sceneCharacterContainer;
    
    public WeaponInstance(ref IEquppiedItemData equppiedItemData, CharacterBonesContainer characterBonesContainer, Collider owner,
        SceneCharacterContainer sceneCharacterContainer)
    {
        EquppiedItemData = equppiedItemData;
        equppiedItemData = null;
        CharacterBonesContainer = characterBonesContainer;
        _owner = owner;
        _sceneCharacterContainer = sceneCharacterContainer;
        
        CreateInstance();
        this.CreateDecorations();
    }
    
    public void CreateInstance()
    {
        if (EquppiedItemData.EquippedModelPrefab == null)
        {
            return;
        }
        if (EquppiedItemData.BoneData == null)
        {
            return;
        }
        
        Instance = Object.Instantiate(EquppiedItemData.EquippedModelPrefab).transform;
        
        IKBoneTransform = this.TryToFindIKBoneTransform();

        ItemControlComponent = new WeaponController(_owner, (WeaponData)EquppiedItemData, _sceneCharacterContainer);
        
        this.AttachToBone(Instance, EquppiedItemData.BoneData[1]);
    }

    public void ResetAction()
    {
        ItemControlComponent.ResetAction();
    }
}
