using UnityEngine;
using Zenject;

public class CharacterFootStepsPlayer : MonoBehaviour
{
    private SfxSet _sfxSet;
    private Transform _lFootTransform;
    private Transform _rFootTransform;

    [Inject]
    private void Construct(CharacterPresetLoader characterPresetLoader, Animator animator)
    {
        _sfxSet = characterPresetLoader.CharacterPersonalityData.CharacterSkinDataSettings.PrimarySkin.StepsBase;
        _lFootTransform = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
        _rFootTransform = animator.GetBoneTransform(HumanBodyBones.RightFoot);
    }
    
    public void LeftStep()
    {
        if (_sfxSet == null)
        {
            return;
        }
        PlaySound(_lFootTransform.position, _sfxSet.GetRandomClip());
    }

    public void RightStep()
    {
        if (_sfxSet == null)
        {
            return;
        }
        PlaySound(_rFootTransform.position, _sfxSet.GetRandomClip());
    }

    private static void PlaySound(Vector3 position, AudioClip clip)
    {
        AudioSource.PlayClipAtPoint(clip, position);
    }
}