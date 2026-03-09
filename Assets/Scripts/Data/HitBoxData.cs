using System;
using UnityEngine;

[Serializable]
public struct HitBoxData
{
    [field: SerializeField] public HumanBodyBones Bone { get; set; } 
    [field: SerializeField] public int LayerIndex { get; set; }
    [field: SerializeField] public float DamageMultiplier { get; set; }
    [field: SerializeField] public GameObject VisualPrefab { get; set; }
    
    [field: Header("Collider Settings")]
    [field: SerializeField] public LayerMask Include { get; set; }
    [field: SerializeField] public LayerMask Exclude { get; set; }
    [field: SerializeField] public Vector3 Center { get; set; }
    [field: SerializeField] public Vector3 Size { get; set; }
}
