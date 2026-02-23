using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileData", menuName = "Scriptable Objects/ProjectileData")]
public class ProjectileData : ScriptableObject, IEquppiedItemData
{
    [field: SerializeField] public WeaponParams WeaponParams { get; set; }
    [field: SerializeField] public LaunchSettings LaunchSettings { get; set; }
    [field: SerializeField] public string ItemName { get; set; }
    [field: SerializeField] public string ItemDescription { get; set; }
    [field: SerializeField] public Sprite ItemIcon { get; set; }
    [field: SerializeField] public GameObject EquippedModelPrefab { get; set; }
    [field: SerializeField] public GameObject GroundedModelPrefab { get; set; }
    [field: SerializeField] public AudioClip SpawnSound { get; set; }
    [field: SerializeField] public AudioClip FlySound { get; set; }
    [field: SerializeField] public AudioClip ImpactSound { get; set; }
    [field: SerializeField] public GameObject TrailPrefab { get; set; }
    [field: SerializeField] public BoneData[] BoneData { get; set; }
    [field: SerializeField] public ItemDecorationData[] ItemDecorationData { get; set; }
    public ItemsPositions.Occupied ItemPosition { get; set; } // hide
    public IKBoneData IKBoneData { get; set; } // hide
    
}

[System.Serializable]
public struct LaunchSettings
{
    [field: SerializeField] public float StickOffset { get; set; }
    [field: SerializeField] public float Gravity { get; set; }
    [field: SerializeField] public float Drag { get; set; }
    [field: SerializeField, Range (0.01f, 20f)] public float MaxSpreadAngles { get; set; }
    [field: SerializeField] public float Lifetime { get; set; }
    [field: SerializeField] public LayerMask LayerMask { get; set; }
}
