using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterSkinData", menuName = "Scriptable Objects/CharacterSkinData")]
public class CharacterSkinData : ScriptableObject
{
    [field: SerializeField] public string SkinName { get; private set; }

    [field: SerializeField] [field: Range(0.5f, 1.5f)] public float SizeMode { get; private set; } = 1f;
    
    [field: SerializeField] public List<SkinData> SkinData { get; private set; }
}

[Serializable]
public class SkinData
{
    [field: SerializeField] public SkinnedMeshRenderer SkinnedMeshRenderer{ get; private set; }
    [field: SerializeField] public Material SkinMaterial { get; private set; }
}
