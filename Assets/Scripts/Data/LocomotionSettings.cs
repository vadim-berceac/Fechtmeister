using UnityEngine;

[System.Serializable]
public struct LocomotionSettings
{
    [field: SerializeField] public CharacterController CharacterController { get; private set; }
    [field: SerializeField] public CapsuleCollider CharacterCollider { get; private set; }
    [field: SerializeField] public float InputSmoothingSpeed { get; private set; }
}
