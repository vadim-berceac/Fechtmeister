using UnityEngine;

public class ArmorInstance : IItemInstance
{
    public IEquppiedItemData EquppiedItemData { get; set; }
    public Transform Instance { get; set; }
    public Transform IKBoneTransform { get; set; }
    public Transform[] ItemDecorations { get; set; }
    public IItemControlComponent ItemControlComponent { get; set; }
    public PlayableGraphCore PlayableGraphCore { get; set; }

    private Collider _owner;

    public ArmorInstance(ref IEquppiedItemData equppiedItemData, Collider owner, PlayableGraphCore playableGraphCore)
    {
        EquppiedItemData = equppiedItemData;
        equppiedItemData = null;
        _owner = owner;
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

        if (EquppiedItemData.BoneData == null || EquppiedItemData.BoneData.Length < 1)
        {
            return;
        }

        Instance = Object.Instantiate(EquppiedItemData.EquippedModelPrefab).transform;
        
        var boneData = EquppiedItemData.BoneData[0];
      
        PlayableGraphCore.AttachEquipment(Instance, boneData.BonesType, boneData.Active, boneData.Position, 
            boneData.Rotation.eulerAngles, boneData.Scale, boneData.UseBone);
    }
}
