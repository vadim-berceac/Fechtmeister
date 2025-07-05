using UnityEngine;

public interface IItemInstance
{
    public IItemData ItemData { get; set; }
    public CharacterBonesContainer CharacterBonesContainer { get; set; }
    public Transform Instance { get; set; }
    public Transform IKBoneTransform { get; set; }
    public Transform[] ItemDecorations { get; set; }
    
    public void CreateInstance();
    public void CreateDecorations();
    public void AttachToBone(Transform instance, BoneData boneData); 
    public Transform TryToFindIKBoneTransform();
    public void DestroyInstance();
}
