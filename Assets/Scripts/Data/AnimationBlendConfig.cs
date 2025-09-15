using UnityEngine;

[CreateAssetMenu(fileName = "AnimationBlendConfig", menuName = "Animation/BlendConfig")]
public class AnimationBlendConfig : ScriptableObject
{
    [System.Serializable]
    public struct BlendClip
    {
        [field: SerializeField] public AnimationClip Clip { get; private set; }     // Анимационный клип
        [field: SerializeField] public float ParamValue { get; private set; }      // Значение параметра для 1D (или порог)
        [field: SerializeField] public Vector2 ParamPosition { get; private set; }  // Позиция в 2D-пространстве (для 2D-бленда)
        [field: SerializeField] public float Speed { get; private set; }           // Скорость воспроизведения клипа
        [field: SerializeField] [Range(0, 1)] public float ActionTime { get; private set; }
    }

    [field: SerializeField] public string ParameterName { get; private set; } 
    [field: SerializeField] public float ParamValue { get; private set; }
    [field: SerializeField] public BlendClip[] BlendClips { get; private set; }    // Массив анимаций
}
