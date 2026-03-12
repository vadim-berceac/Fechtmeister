using UnityEngine;
using Zenject;

public class CharacterFootStepsPlayer : MonoBehaviour
{
    private SfxSet _sfxSet;
    private Transform _lFootTransform;
    private Transform _rFootTransform;
    
    private float _lastFootstepTime;
    private Vector3 _lastFootstepPosition;
    
    private const float FootstepCooldown = 0.25f;
    private const float MinFootstepDistance = 0.4f;

    [Inject]
    private void Construct(CharacterPresetLoader characterPresetLoader, Animator animator)
    {
        _sfxSet = characterPresetLoader.CharacterPersonalityData.CharacterSkinDataSettings.PrimarySkin.StepsBase;
        _lFootTransform = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
        _rFootTransform = animator.GetBoneTransform(HumanBodyBones.RightFoot);
    }
    
    public void LeftStep()
    {
        if (CanPlayFootstep(_lFootTransform.position))
        {
            PlaySound(_lFootTransform.position, _sfxSet.GetRandomClip());
        }
    }

    public void RightStep()
    {
        if (CanPlayFootstep(_rFootTransform.position))
        {
            PlaySound(_rFootTransform.position, _sfxSet.GetRandomClip());
        }
    }

    private bool CanPlayFootstep(Vector3 footPosition)
    {
        if (_sfxSet == null)
            return false;

        if (Time.time - _lastFootstepTime < FootstepCooldown)
            return false;

        if (Vector3.Distance(footPosition, _lastFootstepPosition) < MinFootstepDistance)
            return false;

        _lastFootstepTime = Time.time;
        _lastFootstepPosition = footPosition;
        return true;
    }

    private static void PlaySound(Vector3 position, AudioClip clip)
    {
        AudioSource.PlayClipAtPoint(clip, position);
    }
}