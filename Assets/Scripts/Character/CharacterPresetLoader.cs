using FIMSpace.FProceduralAnimation;
using UnityEngine;
using Zenject;

public class CharacterPresetLoader : MonoBehaviour
{
    [field: SerializeField] public CharacterPersonalityData CharacterPersonalityData { get; set; }
    
    [Inject]
    private void Construct(Animator animator, DiContainer container, ModelTag modelTag, LegsAnimator legsAnimator)
    {
        SetupSkin(modelTag);
        SetupAvatar();
        SetupDecorations();
        animator.Rebind();
        SetupHitBoxes(animator, container);
        SetupLegAnimator(legsAnimator, animator);
    }

    private void SetupHitBoxes(Animator animator, DiContainer container)
    {
        var hitBoxes = CharacterPersonalityData.CharacterSkinDataSettings.PrimarySkin.HitBoxes;

        foreach (var hitBox in hitBoxes)
        {
            var tr = animator.GetBoneTransform(hitBox.Bone);
            var obj = tr.gameObject;
            var col = obj.AddComponent<BoxCollider>();
            var bodyPart = obj.AddComponent<HitBodyPart>();
            
            obj.layer = hitBox.LayerIndex;
            
            col.size = hitBox.Size;
            col.center = hitBox.Center;
            col.excludeLayers = hitBox.Exclude;
            col.includeLayers = hitBox.Include;
            
            container.Inject(bodyPart);
            bodyPart.SetDamageMultiplier(hitBox.DamageMultiplier);
        }
    }

    private void SetupSkin(ModelTag modelTag)
    {
        var skins = CharacterPersonalityData.CharacterSkinDataSettings.PrimarySkin.SkinData;

        foreach (var skinData in skins)
        {
            var skin = Instantiate(skinData.SkinPrefab, modelTag.transform);
        }
    }

    private void SetupAvatar()
    {
        
    }

    private void SetupDecorations()
    {
        
    }

    private void SetupLegAnimator(LegsAnimator legsAnimator, Animator animator)
    {
        legsAnimator.Hips = animator.GetBoneTransform(HumanBodyBones.Hips);
        legsAnimator.Legs[0].InverseHint = true;
        legsAnimator.Legs[1].InverseHint = true;
        
        legsAnimator.Legs[0].BoneStart = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
        legsAnimator.Legs[1].BoneStart = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
        
        legsAnimator.Legs[0].BoneMid = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
        legsAnimator.Legs[1].BoneMid = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
        
        legsAnimator.Legs[0].BoneFeet = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
        legsAnimator.Legs[1].BoneFeet = animator.GetBoneTransform(HumanBodyBones.RightFoot);
        
        legsAnimator.Legs[0].BoneEnd = animator.GetBoneTransform(HumanBodyBones.LeftToes);
        legsAnimator.Legs[1].BoneEnd = animator.GetBoneTransform(HumanBodyBones.RightToes);
    }
}
