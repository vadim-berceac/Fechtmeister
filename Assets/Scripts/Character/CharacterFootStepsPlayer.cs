using UnityEngine;
using Zenject;

public class CharacterFootStepsPlayer : MonoBehaviour, ISfxUser
{
    [field: SerializeField] public string SfxSetName { get; set; }
    [field: SerializeField] private Transform LFootTransform { get; set; }
    [field: SerializeField] private Transform RFootTransform { get; set; }
    
    public SfxSet SfxSet { get; set; }

    [Inject]
    private void Construct(SfxContainer sfxContainer)
    {
        SfxSet = sfxContainer.GetSfxSet(SfxSetName);
    }

    public void PlayRandomSfx(Vector3 position)
    {
        SfxSet.PlayRandomAtPoint(position);
    }

    public void LStepPlay()
    {
        PlayRandomSfx(LFootTransform.position);
    }

    public void RStepPlay()
    {
        PlayRandomSfx(RFootTransform.position);
    }
}
