using FIMSpace.FProceduralAnimation;
using UnityEngine;
using Zenject;

public class CharacterPresetLoader : MonoBehaviour
{
    [field: SerializeField] public CharacterPersonalityData CharacterPersonalityData { get; set; }
    
    [Inject]
    private void Construct(Animator animator, DiContainer container, ModelTag modelTag, LegsAnimator legsAnimator,
        PlayableGraphCore playableGraphCore, HealthComponent healthComponent)
    {
        SetupSkin(modelTag);
        SetupAvatar(animator);
        SetupDecorations(animator);
        animator.Rebind();
        SetupHitBoxes(animator, container, healthComponent);
        if (legsAnimator != null)
        {
            SetupLegAnimator(legsAnimator, animator);
        }
    }

    private void SetupHitBoxes(Animator animator, DiContainer container, HealthComponent healthComponent)
    {
        var hitBoxesData = CharacterPersonalityData.CharacterSkinDataSettings.PrimarySkin.HitBoxes;
        var parts = new HitBodyPart[hitBoxesData.Length];

        for (var i = 0; i < hitBoxesData.Length; i++)
        {
            var tr = animator.GetBoneTransform(hitBoxesData[i].Bone);
            var obj = tr.gameObject;
            var col = obj.AddComponent<BoxCollider>();
            var bodyPart = obj.AddComponent<HitBodyPart>();

            if (hitBoxesData[i].VisualPrefab != null)
            {
                var visualObj = Instantiate(hitBoxesData[i].VisualPrefab, obj.transform);
                visualObj.transform.localPosition = hitBoxesData[i].Center;
                bodyPart.SetVisual(visualObj);
            }
        
            obj.layer = hitBoxesData[i].LayerIndex;
        
            col.size = hitBoxesData[i].Size;
            col.center = hitBoxesData[i].Center;
            col.excludeLayers = hitBoxesData[i].Exclude;
            col.includeLayers = hitBoxesData[i].Include;
        
            container.Inject(bodyPart);
            bodyPart.SetDamageMultiplier(hitBoxesData[i].DamageMultiplier);
        
            parts[i] = bodyPart;
        }
        healthComponent.SetBodyParts(parts);
        healthComponent.EnableHitParts(true);
    }

    private void SetupSkin(ModelTag modelTag)
    {
        while (modelTag.transform.childCount > 0)
        {
            Destroy(modelTag.transform.GetChild(0).gameObject);
        }
        
        var skins = CharacterPersonalityData.CharacterSkinDataSettings.PrimarySkin.SkinData;

        foreach (var skinData in skins)
        {
            var skin = Instantiate(skinData.SkinPrefab, modelTag.transform);
        }
    }

    private void SetupAvatar(Animator animator)
    {
        animator.avatar = CharacterPersonalityData.CharacterSkinDataSettings.PrimarySkin.SkinData[0].Avatar;
    }

    private void SetupDecorations(Animator animator)
    {
        var decorations = CharacterPersonalityData.CharacterSkinDataSettings.PrimarySkin.SkinData[0].Decorations;

        foreach (var decoration in decorations)
        {
            var tr = animator.GetBoneTransform(decoration.BoneData.BonesType);
            var obj = Instantiate(decoration.ItemPrefab, tr);
            obj.transform.SetLocalPositionAndRotation(decoration.BoneData.Position, decoration.BoneData.Rotation);
            obj.transform.localScale = Vector3.one * decoration.BoneData.Scale;
        }
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
