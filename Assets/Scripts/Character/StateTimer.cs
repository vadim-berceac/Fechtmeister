
public class StateTimer
{
    private float _currentTimeInState;
    private bool _actionIsPossible;

    public float GetCurrentTimeInState()
    {
        return _currentTimeInState;
    }

    public bool ActionIsPossible()
    {
        return _actionIsPossible;
    }

    public void SetActionIsPossible(bool isPossible)
    {
        _actionIsPossible = isPossible;
    }

    public void ResetTime()
    {
        _currentTimeInState = 0;
    }

    public void OnUpdate(float deltaTime)
    {
        _currentTimeInState += deltaTime;
    }
}
