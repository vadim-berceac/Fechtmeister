using UnityEngine;

public class CharacterFootStepsPlayer : MonoBehaviour
{
    [field: SerializeField] private CharacterPresetLoader CharacterPresetLoader { get; set; }
    [field: SerializeField] private Transform LFootTransform { get; set; }
    [field: SerializeField] private Transform RFootTransform { get; set; }
    
    public void LeftStep()
    {
        if (CharacterPresetLoader.CharacterPersonalityData.CharacterSkinDataSettings.PrimarySkin.StepsBase == null)
        {
            return;
        }
        PlaySound(LFootTransform.position,
            CharacterPresetLoader.CharacterPersonalityData.CharacterSkinDataSettings.PrimarySkin.StepsBase.GetRandomClip());
    }

    public void RightStep()
    {
        if (CharacterPresetLoader.CharacterPersonalityData.CharacterSkinDataSettings.PrimarySkin.StepsBase == null)
        {
            return;
        }
        PlaySound(RFootTransform.position,
            CharacterPresetLoader.CharacterPersonalityData.CharacterSkinDataSettings.PrimarySkin.StepsBase.GetRandomClip());
    }

    private static void PlaySound(Vector3 position, AudioClip clip)
    {
        AudioSource.PlayClipAtPoint(clip, position);
    }
}