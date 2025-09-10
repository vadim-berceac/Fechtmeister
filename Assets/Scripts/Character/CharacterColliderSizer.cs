using UnityEngine;

public class CharacterColliderSizer
{
    private readonly CharacterSkinData _characterSkinData;
    private readonly CapsuleCollider _capsuleCollider;
    private readonly CharacterController _characterController;
    
    private readonly float _capsuleHeight;
    private readonly float _capsuleRadius;

    public CharacterColliderSizer(CharacterSkinData skinData, CapsuleCollider collider, CharacterController characterController)
    {
        _characterSkinData = skinData;
        _capsuleCollider = collider;
        _characterController = characterController;
        
        _capsuleHeight = _capsuleCollider.height;
        _capsuleRadius = _capsuleCollider.radius;
    }

    public void SetSize(SizeMode sizeMode)
    {
        switch (sizeMode)
        {
            case SizeMode.Full:
            {
                _capsuleCollider.height = _capsuleHeight;
                _characterController.height = _capsuleHeight;
            }
                break;

            case SizeMode.Half:
            {
                _capsuleCollider.height = _capsuleHeight / 2;
                _characterController.height = _capsuleHeight / 2;
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
    Half
}
