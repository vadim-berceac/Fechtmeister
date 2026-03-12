using System;

public static class CharacterSelector
{
    public static Action<CharacterInfo> OnCharacterSelected;
    public static void Select(CharacterInfo characterInfo, bool value)
    {
        if (value)
        {
            characterInfo.Core.SceneCamera.SetTarget(characterInfo.Core.CashedTransform, immediate: true);
            characterInfo.Core.CharacterInputHandler.SetupInputSet(characterInfo.Core.InputByPlayer);
            characterInfo.ControlledByPlayer(true);
            OnCharacterSelected?.Invoke(characterInfo);
            return;
        }
        characterInfo.Core.SceneCamera.SetTarget(null, immediate: true);
        characterInfo.Core.CharacterInputHandler.SetupInputSet(null);
        characterInfo.ControlledByPlayer(false);
        OnCharacterSelected?.Invoke(null);
    }
}
