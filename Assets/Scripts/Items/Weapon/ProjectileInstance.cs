using UnityEngine;

public class ProjectileInstance : IItemInstance
{
    public IEquppiedItemData EquppiedItemData { get; set; }
    public Transform Instance { get; set; }
    public Transform IKBoneTransform { get; set; }
    public Transform[] ItemDecorations { get; set; }
    public IItemControlComponent ItemControlComponent { get; set; }
    public PlayableGraphCore PlayableGraphCore { get; set; }

    private Collider _owner;

    public ProjectileInstance(ref IEquppiedItemData equppiedItemData, Collider owner, PlayableGraphCore playableGraphCore)
    {
        EquppiedItemData = equppiedItemData;
        equppiedItemData = null;
        _owner = owner;
        PlayableGraphCore = playableGraphCore;

        CreateInstance();
    }
    
    public void CreateInstance()
    {
        if (EquppiedItemData.EquippedModelPrefab == null)
        {
            return;
        }

        if (EquppiedItemData.BoneData == null || EquppiedItemData.BoneData.Length < 1)
        {
            return;
        }

        Instance = Object.Instantiate(EquppiedItemData.ItemDecorationData[0].ItemPrefab).transform;
        
        var boneData = EquppiedItemData.ItemDecorationData[0].BoneData;
      
        PlayableGraphCore.AttachEquipment(Instance, boneData.BonesType, boneData.Active,boneData.Position, 
            boneData.Rotation.eulerAngles, boneData.Scale,  boneData.UseBone);
    }
}
