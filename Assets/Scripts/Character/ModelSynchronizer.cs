using UnityEngine;

public class ModelSynchronizer : MonoBehaviour
{
    [field: SerializeField] private CombinedBone[] bones;

    private void Awake()
    {
        foreach (var bone in bones)
        {
            bone.SetStartRotation(bone.Transform.localRotation);
        }
    }

    private void FixedUpdate()
    {
        foreach (var bone in bones)
        {
            var targetRot = Quaternion.Inverse(bone.Transform.localRotation) * bone.StartRotation;
            targetRot = targetRot.normalized; 
            bone.Joint.targetRotation = targetRot;
        }
    }
}

[System.Serializable]
public struct CombinedBone
{
    [field: SerializeField] public string BoneName { get; private set; }
    [field: SerializeField] public Transform Transform { get; private set; }
    [field: SerializeField] public ConfigurableJoint Joint { get; private set; }
    public Quaternion StartRotation { get; private set; }

    public void SetStartRotation(Quaternion startRotation)
    {
        StartRotation = startRotation;
    }
}