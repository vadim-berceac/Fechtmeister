using UnityEngine;

public class ArmorInstance : IItemInstance
{
    public IItemData ItemData { get; set; }
    public CharacterBonesContainer CharacterBonesContainer { get; set; }
    public Transform Instance { get; set; }
    public Transform IKBoneTransform { get; set; }
    public Transform[] ItemDecorations { get; set; }

    public ArmorInstance(ref IItemData itemData, CharacterBonesContainer characterBonesContainer)
    {
        ItemData = itemData;
        CharacterBonesContainer = characterBonesContainer;

        if (itemData.BoneData[0] != null)
        {
            if (itemData.BoneData[0].MeshType == BoneData.MeshInstanceType.Mesh)
            {
                Debug.LogWarning("Создаем инстанс на кости скелета");
                return;
            }

            if (itemData.BoneData[0].MeshType == BoneData.MeshInstanceType.SkinnedMesh)
            {
                Debug.LogWarning("Создаем инстанс на клонируя кости");
            }
        }
    }
    
    public void CreateInstance()
    {
        
    }

    public void CreateDecorations()
    {
        
    }

    public void AttachToBone(Transform instance, BoneData boneData)
    {
        
    }

    public Transform TryToFindIKBoneTransform()
    {
        return null;
    }

    public void DestroyInstance()
    {
        
    }
}
