using System;

public static class CharacterSelector
{
    public static Action<CharacterCore> OnCharacterSelected;
    public static void Select(CharacterCore characterCore, bool value)
    {
        if (value)
        {
            characterCore.SceneCamera.SetTarget(characterCore.CashedTransform);
            characterCore.CharacterInputHandler.SetupInputSet(characterCore.InputByPlayer);
            OnCharacterSelected?.Invoke(characterCore);
            return;
        }
        characterCore.SceneCamera.SetTarget(null);
        characterCore.CharacterInputHandler.SetupInputSet(null);
        OnCharacterSelected?.Invoke(null);
    }
}
