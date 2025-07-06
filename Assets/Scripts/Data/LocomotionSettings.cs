using UnityEngine;

[System.Serializable]
public struct LocomotionSettings
{
    [field: SerializeField] public Animator Animator { get; private set; }
    [field: SerializeField] public CharacterController CharacterController { get; private set; }
}
