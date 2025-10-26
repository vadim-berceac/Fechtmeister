using UnityEngine;

public class WeaponController : ItemControlComponent <WeaponData>
{
    private readonly SceneCharacterContainer _sceneCharacterContainer;
    private readonly int _layerMask;
    private readonly Collider[] _hitBuffer = new Collider[32];
    public WeaponController(Collider owner, WeaponData weaponData, SceneCharacterContainer sceneCharacterContainer)
        : base(owner, weaponData)
    {
        _sceneCharacterContainer = sceneCharacterContainer;
        _layerMask = LayerMask.GetMask("Character");
    }
    
    public override void Use()
    {
        if (ActionCompleted)
        {
            return;
        }
        
        DetectHitsWithOverlap();
        PlaySound(TypedItemData.WeaponParams.WhooshSound, Owner.transform.position);
        ActionCompleted = true;
    }

    public override void ResetAction()
    {
        ActionCompleted = false;
    }

    private void DetectHitsWithOverlap()
    {
        var weaponParams = TypedItemData.WeaponParams;

        var boxCenter = Owner.transform.position + Owner.transform.forward * weaponParams.HitBoxForwardOffset;
        var halfExtents = weaponParams.HitBoxSize * 0.5f;
        var orientation = Owner.transform.rotation;

        var hitCount = Physics.OverlapBoxNonAlloc(boxCenter, halfExtents, _hitBuffer, orientation, _layerMask);

        for (var i = 0; i < hitCount; i++)
        {
            var hit = _hitBuffer[i];
            if (hit == Owner) continue;

            var target = _sceneCharacterContainer.GetCharacter(hit);
            if (target == null) continue;

            PlaySound(TypedItemData.WeaponParams.HitSound, target.transform.position);
            
            target.Health.Damage(weaponParams.Damage);
            Debug.Log($"Overlap Hit: {target.name} (damage: {weaponParams.Damage})");
        }
    }

    private void PlaySound(AudioClip clip, Vector3 position)
    {
        if (clip == null)
        {
            return;
        }
        AudioSource.PlayClipAtPoint(clip, position);
    }
}
