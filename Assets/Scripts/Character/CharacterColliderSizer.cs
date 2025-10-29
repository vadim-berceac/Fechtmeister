using UnityEngine;

public class CharacterColliderSizer
{
    private readonly CharacterSkinData _characterSkinData;
    private readonly CapsuleCollider _capsuleCollider;
    private readonly CharacterController _characterController;
    
    private readonly float _capsuleHeight;
    private readonly float _capsuleRadius;

    public SizeMode CurrentHeight { get; private set; }
    public SizeMode CurrentRadius { get; private set; }

    public CharacterColliderSizer(CharacterSkinData skinData, CapsuleCollider collider, CharacterController characterController)
    {
        _characterSkinData = skinData;
        _capsuleCollider = collider;
        _characterController = characterController;
        
        _capsuleHeight = _capsuleCollider.height;
        _capsuleRadius = _capsuleCollider.radius;
    }

    public void SetSize(SizeMode height, SizeMode radius)
    {
        SetHeight(height);
        SetRadius(radius);
    }

    public void SetEnabled(bool value)
    {
        _capsuleCollider.isTrigger = !value;
        _characterController.enabled = value;
    }

    private void SetHeight(SizeMode sizeMode)
    {
        if (sizeMode == CurrentHeight)
        {
            return;
        }
    
        CurrentHeight = sizeMode;
    
        float newHeight;
        Vector3 newCenter;
    
        switch (CurrentHeight)
        {
            case SizeMode.Full:
            {
                newHeight = _capsuleHeight;
                newCenter = new Vector3(0, _capsuleHeight / 2, 0);
            }
                break;

            case SizeMode.Half:
            {
                newHeight = _capsuleHeight / 2;
                newCenter = new Vector3(0, newHeight / 2, 0);  
            }
                break;
            
            case SizeMode.Quarter:
            {
                newHeight = _capsuleHeight / 4;
                newCenter = new Vector3(0, newHeight / 2, 0); 
            }
                break;

            case SizeMode.Doubled:
            {
                newHeight = _capsuleHeight * 2;
                newCenter = new Vector3(0, newHeight / 2, 0); 
            }
                break;
            
            case SizeMode.Tripled:
            {
                newHeight = _capsuleHeight * 3;
                newCenter = new Vector3(0, newHeight / 2, 0); 
            }
                break;
        
            default:
                return;
        }
    
        _capsuleCollider.height = newHeight;
        _capsuleCollider.center = newCenter;
    
        _characterController.height = newHeight;
        _characterController.center = newCenter;
    }

    private void SetRadius(SizeMode sizeMode)
    {
        if (sizeMode == CurrentRadius)
        {
            return;
        }
        
        CurrentRadius = sizeMode;
        
        switch (CurrentRadius)
        {
            case SizeMode.Full:
            {
                _capsuleCollider.radius = 0.7f;
                _characterController.radius = 0.3f;
            }
                break;
            case SizeMode.Half:
            {
                _capsuleCollider.radius = _capsuleRadius / 2;
                _characterController.radius = _capsuleRadius / 2;
            }
                break;
            
            case SizeMode.Quarter:
            {
                _capsuleCollider.radius = _capsuleRadius / 4;
                _characterController.radius = _capsuleRadius / 4;
            }
                break;

            case SizeMode.Doubled:
            {
                _capsuleCollider.radius = _capsuleRadius * 2;
                _characterController.radius = _capsuleRadius * 2;
            }
                break;
            
            case SizeMode.Tripled:
            {
                _capsuleCollider.radius = _capsuleRadius * 3;
                _characterController.radius = _capsuleRadius * 3;
            }
                break;
            default:
                break;
        }
    }
}

public enum SizeMode
{
    Full,
    Half,
    Quarter,
    Doubled,
    Tripled
}
