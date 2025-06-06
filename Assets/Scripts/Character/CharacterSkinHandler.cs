using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterSkinHandler
{
    private readonly Transform _transform;
    private readonly CharacterSkinData _characterSkinData;
    private SkinnedMeshRenderer _blankRenderer;
    private List<SkinnedMeshRenderer> _temporaryBlankRenderers;

    public CharacterSkinHandler(Transform transform, CharacterSkinData characterSkinData)
    {
        _transform = transform;
        _characterSkinData = characterSkinData;
        
        InitializeRenderer();
        SetupBaseSkin();
        ApplyAdditionalSkins();
    }
    
    private void InitializeRenderer()
    {
        _blankRenderer = _transform.GetComponentsInChildren<SkinnedMeshRenderer>()
            .FirstOrDefault(renderer1 => renderer1.gameObject.name.Contains("blankSkinnedMesh"));
        _temporaryBlankRenderers = new List<SkinnedMeshRenderer>();
    }
    
    private void SetupBaseSkin()
    {
        _transform.localScale = new Vector3(_characterSkinData.SizeMode, _characterSkinData.SizeMode,_characterSkinData.SizeMode);
        ApplySkin(_characterSkinData.SkinData[0], _blankRenderer);
    }
    
    private void ApplyAdditionalSkins()
    {
        if (_characterSkinData.SkinData.Count <= 1) return;

        for (var i = 1; i < _characterSkinData.SkinData.Count; i++)
        {
            var newRendererObject = Object.Instantiate(_blankRenderer, _blankRenderer.transform.parent);
            _temporaryBlankRenderers.Add(newRendererObject);
            ApplySkin(_characterSkinData.SkinData[i], newRendererObject);
        }
    }
    
    private static void ApplySkin(SkinData skinData, SkinnedMeshRenderer targetRenderer)
    {
        Debug.Log(0);
        if (skinData == null || targetRenderer == null) return;
        Debug.Log(1);
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
