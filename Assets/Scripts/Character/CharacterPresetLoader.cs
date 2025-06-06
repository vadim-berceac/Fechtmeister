using UnityEngine;

public class CharacterPresetLoader : MonoBehaviour
{
    //сюжа юужем устанавливать пресет персонажа, скин, скиллы и тд
    [field: SerializeField] public CharacterPersonalityData CharacterPersonalityData { get; set; }
}
