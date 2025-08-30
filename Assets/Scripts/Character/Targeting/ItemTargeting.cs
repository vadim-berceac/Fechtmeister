using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemTargeting : MonoBehaviour, ITargetingComponent
{
    [field: SerializeField] public Collider TargetingCollider { get; set; }
    [field: SerializeField] public CharacterCore CharacterCore { get; set; }
    [field: SerializeField] public Transform ParentTransform { get; set; }
    [field: SerializeField] public CharacterController CharacterController { get; set; }
    public HashSet<Transform> Targets { get; set; }
    public bool IsAllowed { get; set; }
    public bool IsSelectedCharacter { get; set; }
    
    public Action<Transform> OnTargetAdded { get; set; }
    public Action<Transform> OnTargetRemoved { get; set; }

    private void Awake()
    {
        Targets = new HashSet<Transform>();
        CharacterSelector.OnCharacterSelected += OnCharacterSelected;
    }
    
    public void OnCharacterSelected(CharacterCore characterCoreSelected)
    {
        if (characterCoreSelected == CharacterCore)
        {
            IsSelectedCharacter = true;
            return;
        }
        IsSelectedCharacter = false;
        foreach (var target in Targets)
        {
            target.TryGetComponent<PickupItem>(out var pickupItem);

            if (pickupItem != null)
            {
                pickupItem.ShowNamePlate(false);
            }
        }
        Targets.Clear();
    }
    
    public void ShowTargetName(Transform target, bool show)
    {
        target.TryGetComponent<PickupItem>(out var item);

        if (item == null)
        {
            return;
        }
        
        item.ShowNamePlate(show);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        this.Check(other);
    }

    private void OnTriggerStay(Collider other)
    {
        this.Check(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsAllowed)
        {
            return;
        }
        
        this.RemoveTarget(other.transform);
    }

    private void OnDisable()
    {
        CharacterSelector.OnCharacterSelected -= OnCharacterSelected;
    }
}
