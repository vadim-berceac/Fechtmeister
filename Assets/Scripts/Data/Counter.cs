using UnityEngine;

public class Counter
{
    public int Count {get; private set;}
    private float _lastIncrementTime;
    private readonly float _resetDelay; 
    private readonly int _maxCount; 

    public Counter(float resetDelay, int maxCount)
    {
        _resetDelay = resetDelay;
        _maxCount = maxCount;
        
        Count = 0;
        _lastIncrementTime = -_resetDelay; 
    }

    public void Increment()
    {
        if (Time.time - _lastIncrementTime > _resetDelay)
        {
            Count = 0;
        }
        
        Count++;
        
        if (Count > _maxCount)
        {
            Count = 0;
        }
       
        _lastIncrementTime = Time.time;
    }

    public void Reset()
    {
        Count = 0;
        _lastIncrementTime = -_resetDelay;
    }
}
