using UnityEngine;

[CreateAssetMenu(fileName = "AnimationBlendConfig", menuName = "Animation/BlendConfig")]
public class AnimationBlendConfig : ScriptableObject
{
    [field: SerializeField] public float ParamValue { get; private set; }
    [field: SerializeField] public BlendClip[] Clips { get; private set; } 
}

[System.Serializable]
public struct BlendClip
{
    [field: SerializeField] public AnimationClip Clip { get; private set; }    
    [field: SerializeField] public float ParamValue { get; private set; }      
    [field: SerializeField] public Vector2 ParamPosition { get; private set; }  
    [field: SerializeField] public float Speed { get; private set; }         
    [field: SerializeField, Range(0, 1)] public float ActionTime { get; private set; }
}
