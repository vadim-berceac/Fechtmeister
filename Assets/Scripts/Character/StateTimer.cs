public class StateTimer
{
    private float _currentTimeInState;

    public float GetCurrentTimeInState()
    {
        return _currentTimeInState;
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
