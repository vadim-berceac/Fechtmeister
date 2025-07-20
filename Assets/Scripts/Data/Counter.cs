using UnityEngine;

public class Counter
{
    private int _count;
    private float _lastIncrementTime;
    private float _resetDelay; 
    private int _maxCount;

    public Counter()
    {
        _count = 0;
        _lastIncrementTime = -_resetDelay; 
    }
    
    public void SetValue(float resetDelay, int maxCount)
    {
        _resetDelay = resetDelay;
        _maxCount = maxCount;
        
        _count = 0;
        _lastIncrementTime = -_resetDelay; 
    }

    public int GetValue()
    {
        Increment();
        return _count;
    }

    private void Increment()
    {
        if (Time.time - _lastIncrementTime > _resetDelay)
        {
            _count = 0;
            _lastIncrementTime = Time.time;
            return;
        }
        
        _count++;
        
        if (_count > _maxCount)
        {
            _count = 0;
        }
       
        _lastIncrementTime = Time.time;
    }
}
