using UnityEngine;

[CreateAssetMenu(fileName = "SfxSet", menuName = "Scriptable Objects/SfxSet")]
public class SfxSet : ScriptableObject
{
    [field: SerializeField] private SfxData[] sfxData;

    public AudioClip GetRandomClip()
    {
        return sfxData[Random.Range(0, sfxData.Length)].Clip;
    }
}

[System.Serializable]
public struct SfxData
{
    // на случай если еще понадобятся поля
    [field: SerializeField] public AudioClip Clip { get; private set; }
}
