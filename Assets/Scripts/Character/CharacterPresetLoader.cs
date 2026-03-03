using Unity.VisualScripting;
using UnityEngine;
using Zenject;

public class CharacterPresetLoader : MonoBehaviour
{
    [field: SerializeField] public CharacterPersonalityData CharacterPersonalityData { get; set; }
    
    [Inject]
    private void Construct(Animator animator, DiContainer container)
    {
        SetupHitBoxes(animator, container);
    }

    public void SetupHitBoxes(Animator animator, DiContainer container)
    {
        var hitBoxes = CharacterPersonalityData.CharacterSkinDataSettings.PrimarySkin.HitBoxes;

        foreach (var hitBox in hitBoxes)
        {
            var tr = animator.GetBoneTransform(hitBox.Bone);
            var col = tr.AddComponent<BoxCollider>();
            var bodyPart = tr.AddComponent<HitBodyPart>();
            
            tr.gameObject.layer = hitBox.LayerIndex;
            
            col.size = hitBox.Size;
            col.center = hitBox.Center;
            col.excludeLayers = hitBox.Exclude;
            col.includeLayers = hitBox.Include;
            
            container.Inject(bodyPart);
            bodyPart.SetDamageMultiplier(hitBox.DamageMultiplier);
        }
    }
}
