using UnityEngine;

public class WeaponInstance : IItemInstance
{
    public IEquppiedItemData EquppiedItemData { get; set; }
    public Transform Instance { get; set; }
    public Transform IKBoneTransform { get; set; }
    public Transform[] ItemDecorations { get; set; }
    public IItemControlComponent ItemControlComponent { get; set; }
    public Animator Animator { get; set; }

    private readonly Collider _owner;
    private readonly SceneCharacterContainer _sceneCharacterContainer;
    
    public WeaponInstance(ref IEquppiedItemData equppiedItemData, Collider owner,
        SceneCharacterContainer sceneCharacterContainer, Animator animator)
    {
        EquppiedItemData = equppiedItemData;
        equppiedItemData = null;
        _owner = owner;
        _sceneCharacterContainer = sceneCharacterContainer;
        Animator = animator;
        
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

        var boneData = EquppiedItemData.BoneData[1];
      
        Animator.AttachToBone(Instance, boneData.BonesType, boneData.Position, 
            boneData.Rotation.eulerAngles, boneData.Scale, boneData.Active);
    }

    public void ResetAction()
    {
        ItemControlComponent.ResetAction();
    }
}
