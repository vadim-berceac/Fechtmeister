using UnityEngine;

public abstract class State : ScriptableObject
{
    [field: SerializeField] protected float enterTransitionDuration;
    [field: SerializeField] protected int animationLayer;
    [field: SerializeField] protected bool updateRotationByCamera;
    public abstract void EnterState(CharacterCore character);

    public virtual void UpdateState(CharacterCore character)
    {
        UpdateRotationByCamera(character);
    }
    public abstract void CheckSwitch(CharacterCore character);
    public abstract void ExitState(CharacterCore character);

    private void UpdateRotationByCamera(CharacterCore character)
    {
        if (!updateRotationByCamera)
        {
            return;
        }
        if (character.SceneCamera.Target == character.transform)
        {
            character.transform.rotation = Quaternion.Euler(0, character.SceneCamera.SceneCameraData.MainCamera.eulerAngles.y, 0);
        }
    }
}
