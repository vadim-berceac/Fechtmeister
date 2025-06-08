using UnityEngine;

public static class SkinnedMeshRendererExtension
{
    private static void ChangeCharacterSkin(this SkinnedMeshRenderer oldSkin, SkinnedMeshRenderer newSkin)
    {
        if (newSkin.sharedMesh == null)
        {
            oldSkin.sharedMesh = null;
            return;
        }
        
        var newMesh = Object.Instantiate(newSkin.sharedMesh);
        var bones2 = newSkin.bones;
        var newBones = new Transform[bones2.Length];

        for (var i = 0; i < bones2.Length; i++)
        {
            var boneName = bones2[i].name;
            var bone = oldSkin.transform.root.FindChildRecursive(boneName);
            if (bone == null)
            {
                continue;
            }
            newBones[i] = bone;
        }

        oldSkin.sharedMesh = newMesh;
        oldSkin.bones = newBones;
    }
    
    public static void ApplySkin(this SkinnedMeshRenderer targetRenderer, SkinData skinData)
    {
        if (skinData == null || targetRenderer == null) return;
       
        if (skinData.SkinnedMeshRenderer != null)
        {
            targetRenderer.ChangeCharacterSkin(skinData.SkinnedMeshRenderer);
        }

        if (skinData.SkinMaterial != null)
        {
            targetRenderer.sharedMaterial = skinData.SkinMaterial;
        }
    }
}
