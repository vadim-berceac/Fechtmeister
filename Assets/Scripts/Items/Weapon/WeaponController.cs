using UnityEngine;

public class WeaponController : ItemControlComponent<WeaponData>
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
        if (ActionCompleted) return;

        DetectThreatsNearby();
        DetectAndDamageTargets();
        PlaySound(TypedItemData.WeaponParams.WhooshSounds.GetRandomClip(), Owner.transform.position);
        
        ActionCompleted = true;
    }

    public override void ResetAction()
    {
        ActionCompleted = false;
    }

    private void DetectThreatsNearby()
    {
        var threatMultiplier = 2;
        
        ProcessOverlapDetection(
            radiusMultiplier: threatMultiplier,
            onTargetDetected: target => target.Health.OnDamageAttempt?.Invoke(Owner.transform)
        );
    }

    private void DetectAndDamageTargets()
    {
        var weaponParams = TypedItemData.WeaponParams;
        
        ProcessOverlapDetection(
            radiusMultiplier: 1f,
            onTargetDetected: target =>
            {
                PlaySound(weaponParams.HitSounds.GetRandomClip(), target.transform.position);
                target.Health.Damage(weaponParams.Damage, weaponParams.DamageType, Owner.transform);
                Debug.Log($"Overlap Hit: {target.name} (damage: {weaponParams.Damage})");
            }
        );
    }

    private void ProcessOverlapDetection(float radiusMultiplier, System.Action<CharacterCore> onTargetDetected)
    {
        var weaponParams = TypedItemData.WeaponParams;
        
        var boxCenter = Owner.transform.position + Owner.transform.forward * weaponParams.HitBoxForwardOffset;
        var halfExtents = weaponParams.HitBoxSize * radiusMultiplier * 0.5f;
        var orientation = Owner.transform.rotation;

        var hitCount = Physics.OverlapBoxNonAlloc(boxCenter, halfExtents, _hitBuffer, orientation, _layerMask);

        for (var i = 0; i < hitCount; i++)
        {
            var hit = _hitBuffer[i];
            if (hit == Owner) continue;

            var target = _sceneCharacterContainer.GetCharacter(hit);
            if (target == null) continue;

            onTargetDetected?.Invoke(target);
        }
    }

    private void PlaySound(AudioClip clip, Vector3 position)
    {
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, position);
    }
}