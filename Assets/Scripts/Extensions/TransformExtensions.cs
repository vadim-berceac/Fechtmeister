using Unity.Burst;
using UnityEngine;

public static class TransformExtensions
{
    public static Transform FindChildRecursive(this Transform transform, string name)
    {
        foreach (Transform child in transform)
        {
            if (child.name.Contains(name))
            {
                return child;
            }

            var result = FindChildRecursive(child, name);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }
}
