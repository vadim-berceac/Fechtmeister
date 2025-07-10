using UnityEngine;
using System.Linq;

public static class StateExtensions
{
    public static void CorrectLayersWeight(this State state, CharacterCore character, AdditionalLayer[] additionalLayers, float enterTransitionDuration)
    {
        for (var i = 0; i < character.LocomotionSettings.Animator.layerCount; i++)
        {
            var isLayerInAdditionalLayers = false;
            
            if (additionalLayers != null && additionalLayers.Length > 0)
            {
                foreach (var layer in additionalLayers)
                {
                    if (layer.Layer != i)
                    {
                        continue;
                    }
                    isLayerInAdditionalLayers = true;
                    if (layer.ExcludedWeaponIndices.Contains(
                            (int)(character.LocomotionSettings.Animator.GetFloat(AnimationParams.WeaponType))))
                    {
                        character.LocomotionSettings.Animator.SetLayerWeight(i, 0);
                        continue;
                    }
                    character.AnimationLayerWeightTransition.StartTransition(layer.Layer, layer.Weight, enterTransitionDuration);
                }
            }
            if (!isLayerInAdditionalLayers)
            {
                character.AnimationLayerWeightTransition.StartTransition(i, 0, enterTransitionDuration);
            }
        }
    }
}
