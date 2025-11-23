using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileData", menuName = "Scriptable Objects/ProjectileData")]
public class ProjectileData : ScriptableObject, ISimpleItemData
{
    [field: SerializeField] public WeaponParams WeaponParams { get; set; }
    [field: SerializeField] public LaunchSettings LaunchSettings { get; set; }
    [field: SerializeField] public string ItemName { get; set; }
    [field: SerializeField] public string ItemDescription { get; set; }
    [field: SerializeField] public Sprite ItemIcon { get; set; }
    [field: SerializeField] public GameObject EquippedModelPrefab { get; set; }
    [field: SerializeField] public GameObject GroundedModelPrefab { get; set; }
    [field: SerializeField] public BoneData ShootPosition { get; set; }
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
