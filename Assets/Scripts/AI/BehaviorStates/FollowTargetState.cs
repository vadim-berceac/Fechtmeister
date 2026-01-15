using UnityEngine;

public struct FollowTargetState : INavMeshState
{
    private bool _isWeaponRanged;
    private float _preferredDistance;
    private float _minDistance;
    private float _maxDistance;
    
    // Кэшируем квадраты дистанций
    private float _minDistanceSqr;
    private float _maxDistanceSqr;
    
    public void Enter(ref NavMeshStateData data, NavMeshCharacterInput input)
    {
        ResetTrackingData(ref data);
        var weaponData = ((WeaponData)input.Character.Inventory.WeaponSystem.InstanceInHands.EquppiedItemData);
        _isWeaponRanged = weaponData.IsRanged;
        
        _preferredDistance = weaponData.WeaponParams.PreferredDistance;
        
        // ВОТ КЛЮЧЕВОЕ ОТЛИЧИЕ:
        if (_isWeaponRanged)
        {
            // Для дальнобойки: широкая зона "Optimal" (можем атаковать с близкой дистанции)
            _minDistance = 3f; // Отступаем только если ОЧЕНЬ близко
            _maxDistance = _preferredDistance * 1.3f; // Чуть дальше предпочтительной
        }
        else
        {
            // Для ближнего боя: узкая зона вокруг preferred
            _minDistance = _preferredDistance * 0.8f;
            _maxDistance = _preferredDistance * 1.2f;
        }
        
        _minDistanceSqr = _minDistance * _minDistance;
        _maxDistanceSqr = _maxDistance * _maxDistance;
        
        NavMeshUtility.CalculatePath(ref data);
        
        if (!data.HasWeaponDrawn)
        {
            input.InvokeDrawWeapon();
            data.HasWeaponDrawn = true;
            data.WeaponDrawTime = Time.time;
            data.IsWeaponReady = false;
            Debug.Log($"[NavMesh] Drawing {(weaponData.IsRanged ? "ranged" : "melee")} weapon, preferred: {_preferredDistance:F1}m, min: {_minDistance:F1}m, max: {_maxDistance:F1}m");
        }
    }

    public void Update(ref NavMeshStateData data, NavMeshCharacterInput input)
    {
        if (!ValidateTarget(ref data, input)) return;

        RotateTowardsTarget(ref data, input);
        UpdateWeaponReadiness(ref data);

        var vectorToTarget = data.TargetTransform.position - data.Transform.position;
        var distanceSqrToTarget = vectorToTarget.sqrMagnitude;
        
        UpdateDistanceManagement(ref data, input, vectorToTarget, distanceSqrToTarget);

        if (UpdateStuckDetection(ref data, input, distanceSqrToTarget)) return;

        UpdatePathRecalculation(ref data);
    }

    public void Exit(ref NavMeshStateData data, NavMeshCharacterInput input)
    {
        input.InvokeMove(Vector2.zero);
        input.SetRunState(false);
        data.IsStuck = false;
        data.HasReachedDestination = false;
    }

    private bool ValidateTarget(ref NavMeshStateData data, NavMeshCharacterInput input)
    {
        if (data.TargetTransform != null) return true;
        
        input.InvokeMove(Vector2.zero);
        input.SetRunState(false);
        return false;
    }

    private void RotateTowardsTarget(ref NavMeshStateData data, NavMeshCharacterInput input)
    {
        var directionToTarget = (data.TargetTransform.position - data.Transform.position).normalized;
        directionToTarget.y = 0;
        
        if (directionToTarget.sqrMagnitude > 0.001f)
        {
            var targetRotation = Quaternion.LookRotation(directionToTarget);
            var currentRotation = data.Transform.rotation;
            
            var newRotation = Quaternion.Slerp(
                currentRotation, 
                targetRotation, 
                data.Settings.IdleRotationSpeed * Time.deltaTime
            );
            
            data.Transform.rotation = newRotation;
            
            var lookAngle = Vector3.SignedAngle(data.Transform.forward, directionToTarget, Vector3.up);
            input.InvokeLook(new Vector2(lookAngle * 0.1f, 0f));
        }
    }

    private void UpdateWeaponReadiness(ref NavMeshStateData data)
    {
        if (!data.IsWeaponReady && data.HasWeaponDrawn)
        {
            var timeSinceDrawn = Time.time - data.WeaponDrawTime;
            
            if (timeSinceDrawn >= data.Settings.WeaponDrawDelay)
            {
                data.IsWeaponReady = true;
                Debug.Log($"[NavMesh] Weapon ready after {timeSinceDrawn:F2}s");
            }
        }
    }

    private void UpdateDistanceManagement(ref NavMeshStateData data, NavMeshCharacterInput input, Vector3 vectorToTarget, float distanceSqrToTarget)
    {
        DistanceState distanceState = GetDistanceState(distanceSqrToTarget);
    
        Debug.Log($"<color=yellow>[UpdateDistance] State: {distanceState}, Distance: {Mathf.Sqrt(distanceSqrToTarget):F1}m</color>");
    
        switch (distanceState)
        {
            case DistanceState.TooClose:
                Debug.Log("[UpdateDistance] Calling HandleTooClose");
                HandleTooClose(ref data, input, vectorToTarget, distanceSqrToTarget);
                break;
        
            case DistanceState.Optimal:
                Debug.Log("[UpdateDistance] Calling HandleOptimalDistance");
                HandleOptimalDistance(ref data, input, distanceSqrToTarget);
                break;
        
            case DistanceState.TooFar:
                Debug.Log("[UpdateDistance] Calling HandleTooFar");
                HandleTooFar(ref data, input, vectorToTarget, distanceSqrToTarget);
                break;
        }
    
        Debug.Log("[UpdateDistance] Done");
    }

    private DistanceState GetDistanceState(float distanceSqrToTarget)
    {
        if (distanceSqrToTarget < _minDistanceSqr)
        {
            return DistanceState.TooClose;
        }
        else if (distanceSqrToTarget > _maxDistanceSqr)
        {
            return DistanceState.TooFar;
        }
        else
        {
            return DistanceState.Optimal;
        }
    }

    private void HandleTooClose(ref NavMeshStateData data, NavMeshCharacterInput input, Vector3 vectorToTarget, float distanceSqrToTarget)
    {
        data.HasReachedDestination = false;
        
        // И для дальнобойки, и для ближнего боя: ОТСТУПАЕМ
        BackAwayFromTarget(ref data, input, vectorToTarget);
        
        // Дальнобойка отступает быстрее
        input.SetRunState(_isWeaponRanged);
        
        // Атакуем во время отступления (если оружие готово)
        if (data.IsWeaponReady)
        {
            input.InvokeAttack();
        }
        
        var dist = Mathf.Sqrt(distanceSqrToTarget);
        Debug.Log($"[NavMesh] {(_isWeaponRanged ? "Ranged" : "Melee")}: TOO CLOSE ({dist:F1}m < {_minDistance:F1}m), backing up {(_isWeaponRanged ? "FAST" : "slow")}");
    }

    private void BackAwayFromTarget(ref NavMeshStateData data, NavMeshCharacterInput input, Vector3 vectorToTarget)
    {
        var moveInput = new Vector2(0f, -1f);
        input.InvokeMove(moveInput);
    }

    private void HandleOptimalDistance(ref NavMeshStateData data, NavMeshCharacterInput input, float distanceSqrToTarget)
    {
        if (!data.HasReachedDestination)
        {
            data.HasReachedDestination = true;
            var dist = Mathf.Sqrt(distanceSqrToTarget);
            Debug.Log($"[NavMesh] Optimal distance reached ({dist:F1}m)");
        }
        
        // Стрейфим (для дальнобойки активнее)
        UpdateStrafing(ref data, input);
        
        input.SetRunState(false);
        
        // Атакуем
        if (data.IsWeaponReady)
        {
            input.InvokeAttack();
        }
    }

    private void HandleTooFar(ref NavMeshStateData data, NavMeshCharacterInput input, Vector3 vectorToTarget, float distanceSqrToTarget)
    {
        if (data.HasReachedDestination)
        {
            data.HasReachedDestination = false;
            ResetTrackingData(ref data);
            NavMeshUtility.CalculatePath(ref data);
            var dist = Mathf.Sqrt(distanceSqrToTarget);
            Debug.Log($"[NavMesh] Target too far ({dist:F1}m > {_maxDistance:F1}m), pursuing");
        }
    
        // Дальнобойка: стреляем на подходе если не слишком далеко
        if (_isWeaponRanged && data.IsWeaponReady)
        {
            var maxShootingDistanceSqr = (_preferredDistance * 1.8f) * (_preferredDistance * 1.8f);
            if (distanceSqrToTarget < maxShootingDistanceSqr)
            {
                input.InvokeAttack();
            }
        }
    
        var directionToTarget = vectorToTarget.normalized;
        var localDirection = data.Transform.InverseTransformDirection(directionToTarget);
    
        // ДИАГНОСТИКА
        var dist2 = Mathf.Sqrt(distanceSqrToTarget);
        Debug.Log($"[TooFar] Distance: {dist2:F1}m, LocalDir: ({localDirection.x:F2}, {localDirection.z:F2}), Sending: (0, 1)");
    
        // Вперёд (для Root Motion)
        var moveInput = new Vector2(0f, 1f);
        input.InvokeMove(moveInput);
    
        var runThresholdSqr = data.Settings.RunDistanceThreshold * data.Settings.RunDistanceThreshold;
        var shouldRun = distanceSqrToTarget > runThresholdSqr;
        input.SetRunState(shouldRun);
    
        Debug.Log($"[TooFar] shouldRun: {shouldRun}, threshold: {Mathf.Sqrt(runThresholdSqr):F1}m");
    }

    private void UpdateStrafing(ref NavMeshStateData data, NavMeshCharacterInput input)
    {
        // Меняем направление стрейфа периодически
        if (Time.time - data.LastStrafeChangeTime > data.Settings.StrafeChangeInterval)
        {
            if (_isWeaponRanged)
            {
                // Дальнобойка: почти всегда двигаемся (5% стоим)
                var rand = Random.value;
                if (rand < 0.475f)
                    data.StrafeDirection = -1;
                else if (rand < 0.95f)
                    data.StrafeDirection = 1;
                else
                    data.StrafeDirection = 0;
            }
            else
            {
                // Ближний бой: стрейфим если цель движется
                var targetDelta = data.TargetTransform.position - data.LastTargetPosition;
                var targetVelocitySqr = targetDelta.sqrMagnitude / (Time.deltaTime * Time.deltaTime);
                var velocityThresholdSqr = 0.5f * 0.5f;
                
                if (targetVelocitySqr > velocityThresholdSqr)
                {
                    var rand = Random.value;
                    if (rand < 0.4f)
                        data.StrafeDirection = -1;
                    else if (rand < 0.8f)
                        data.StrafeDirection = 1;
                    else
                        data.StrafeDirection = 0;
                }
                else
                {
                    data.StrafeDirection = 0; // Стоим
                }
            }
            
            data.LastStrafeChangeTime = Time.time;
        }
        
        if (data.StrafeDirection != 0)
        {
            // Чисто влево/вправо (для Root Motion)
            var moveInput = new Vector2(data.StrafeDirection * 1f, 0f);
            input.InvokeMove(moveInput);
        }
        else
        {
            input.InvokeMove(Vector2.zero);
        }
    }

    private bool UpdateStuckDetection(ref NavMeshStateData data, NavMeshCharacterInput input, float distanceSqrToTarget)
    {
        if (data.HasReachedDestination)
        {
            return false;
        }
        
        var finalDestinationSqr = data.Settings.FinalDestinationDistance * data.Settings.FinalDestinationDistance;
        var resetThresholdSqr = (data.Settings.FinalDestinationDistance * 2f) * (data.Settings.FinalDestinationDistance * 2f);
        
        if (data.IsStuck && distanceSqrToTarget > resetThresholdSqr)
        {
            ResetTrackingData(ref data);
            NavMeshUtility.CalculatePath(ref data);
            return false;
        }

        if (Time.time - data.LastProgressCheckTime > data.Settings.StuckDetectionTime)
        {
            var progressDelta = data.Transform.position - data.LastCharacterPosition;
            var progressDistanceSqr = progressDelta.sqrMagnitude;
            var minProgressSqr = data.Settings.MinProgressDistance * data.Settings.MinProgressDistance;
            
            if (progressDistanceSqr < minProgressSqr && distanceSqrToTarget > finalDestinationSqr)
            {
                data.IsStuck = true;
                input.InvokeMove(Vector2.zero);
                input.SetRunState(false);
                Debug.LogWarning("[NavMeshInput] Character stuck!");
                
                if (data.IsWeaponReady && distanceSqrToTarget <= _maxDistanceSqr * 2f)
                {
                    input.InvokeAttack();
                }
                
                return true;
            }
            
            data.LastCharacterPosition = data.Transform.position;
            data.LastProgressCheckTime = Time.time;
        }

        if (data.IsStuck)
        {
            input.SetRunState(false);
            input.InvokeMove(Vector2.zero);
            
            if (data.IsWeaponReady && distanceSqrToTarget <= _maxDistanceSqr * 2f)
            {
                input.InvokeAttack();
            }
            
            return true;
        }

        return false;
    }

    private void UpdatePathRecalculation(ref NavMeshStateData data)
    {
        var shouldRecalculate = Time.time >= data.NextPathUpdateTime;

        if (data.TargetTransform != null)
        {
            var targetDelta = data.TargetTransform.position - data.LastTargetPosition;
            var targetMovedSqr = targetDelta.sqrMagnitude;
            var recalcThresholdSqr = data.Settings.PathRecalculationThreshold * data.Settings.PathRecalculationThreshold;
            
            if (targetMovedSqr > recalcThresholdSqr)
            {
                shouldRecalculate = true;
            }
        }

        if (shouldRecalculate)
        {
            data.LastTargetPosition = data.TargetTransform.position;
            data.NextPathUpdateTime = Time.time + data.Settings.PathUpdateInterval;
        }
    }

    private void ResetTrackingData(ref NavMeshStateData data)
    {
        data.LastCharacterPosition = data.Transform.position;
        data.LastProgressCheckTime = Time.time;
        data.CurrentWaypointIndex = 0;
        data.IsStuck = false;
        data.HasReachedDestination = false;
    }
    
    private enum DistanceState
    {
        TooClose,
        Optimal,
        TooFar
    }
}