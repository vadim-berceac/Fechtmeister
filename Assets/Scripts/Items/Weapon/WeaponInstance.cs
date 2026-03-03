using UnityEngine;

public class WeaponInstance : IItemInstance
{
    public IEquppiedItemData EquppiedItemData { get; set; }
    public Transform Instance { get; set; }
    public Transform[] ItemDecorations { get; set; }
    public IItemControlComponent ItemControlComponent { get; set; }
    public PlayableGraphCore PlayableGraphCore { get; set; }

    private readonly Collider _owner;
    private readonly SceneCharacterContainer _sceneCharacterContainer;
    
    public WeaponInstance(ref IEquppiedItemData equppiedItemData, Collider owner,
        SceneCharacterContainer sceneCharacterContainer, PlayableGraphCore playableGraphCore)
    {
        EquppiedItemData = equppiedItemData;
        equppiedItemData = null;
        _owner = owner;
        _sceneCharacterContainer = sceneCharacterContainer;
        PlayableGraphCore = playableGraphCore;
        
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

        ItemControlComponent = new WeaponController(_owner, (WeaponData)EquppiedItemData, _sceneCharacterContainer);

        var boneData = EquppiedItemData.BoneData[1];
      
        PlayableGraphCore.AttachEquipment(Instance, boneData.BonesType, boneData.Active, boneData.Position, 
            boneData.Rotation.eulerAngles, boneData.Scale,  boneData.UseBone);
    }

    public void ResetAction()
    {
        ItemControlComponent.ResetAction();
    }
}
