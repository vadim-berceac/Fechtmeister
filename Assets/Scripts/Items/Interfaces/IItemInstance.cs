using UnityEngine;

public interface IItemInstance
{
    public IItemData ItemData { get; set; }
    public CharacterBonesContainer CharacterBonesContainer { get; set; }
    public Transform Instance { get; set; }
    
    public void CreateInstance();
    public void AttachToBone(CharacterBones.Type boneType); // получаем BoneData из ItemData, получаем BoneTransform из CharacterBonesContainer, совмещаем
    public void DestroyInstance();
}
