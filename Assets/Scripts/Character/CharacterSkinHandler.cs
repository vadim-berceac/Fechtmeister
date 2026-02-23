using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterSkinHandler
{
    private const string BlankSkinnedMeshName = "blankSkinnedMesh";
    private readonly Transform _transform;
    private readonly Animator _animator;
    private readonly CharacterSkinData _characterSkinData;
    private SkinnedMeshRenderer _blankRenderer;
    private List<SkinnedMeshRenderer> _renderersInstances;

    public CharacterSkinHandler(Transform transform, CharacterPersonalityData characterPersonalityData, Animator animator)
    {
        _transform = transform;
        _animator = animator;
        _characterSkinData = characterPersonalityData.CharacterSkinDataSettings.PrimarySkin;
        _transform.gameObject.name = characterPersonalityData.NamingSettings.CharacterName;
        
        InitializeRenderer();
    }
    
    private void InitializeRenderer()
    {
        _blankRenderer = _transform.GetComponentsInChildren<SkinnedMeshRenderer>()
            .FirstOrDefault(renderer1 => renderer1.gameObject.name.Contains(BlankSkinnedMeshName));
        _renderersInstances = new List<SkinnedMeshRenderer>();
        
        SetupBaseSkin();
        ApplyAdditionalSkins();
    }
    
    private void SetupBaseSkin()
    {
        _transform.localScale = new Vector3(_characterSkinData.SizeMode, _characterSkinData.SizeMode,_characterSkinData.SizeMode);
        _blankRenderer.ApplySkin(_characterSkinData.SkinData[0], _animator);
        _renderersInstances.Add(_blankRenderer);
    }
    
    private void ApplyAdditionalSkins()
    {
        if (_characterSkinData.SkinData.Count <= 1) return;

        for (var i = 1; i < _characterSkinData.SkinData.Count; i++)
        {
            var newRendererObject = Object.Instantiate(_blankRenderer, _blankRenderer.transform.parent);
            _renderersInstances.Add(newRendererObject);
            newRendererObject.ApplySkin(_characterSkinData.SkinData[i], _animator);
        }
    }

    public void ClearRenderersInstances()
    {
        if (_renderersInstances == null || _renderersInstances.Count < 1) return;
        foreach (var renderer in _renderersInstances)
        {
            if (renderer.name.Contains(BlankSkinnedMeshName))
            {
                renderer.ApplySkin(null, _animator);
                continue;
            }
            Object.Destroy(renderer.gameObject);
        }
        _renderersInstances.Clear();
    }
}
