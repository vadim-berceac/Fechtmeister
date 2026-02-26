using System.Collections.Generic;
using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "VisionSystem", menuName = "Zenject/VisionSystem")]
public class VisionSystem: ScriptableObject, ITickable
{
    [Header("Vision Settings")]
    [SerializeField] private float _defaultVisionRadius = 15f;
    [SerializeField] private float _defaultVisionAngle = 120f;
    [SerializeField] private LayerMask _obstacleMask;
    
    [Header("Performance")]
    [SerializeField] private float _cellSize = 5f;
    [SerializeField] private float _updateInterval = 0.1f;
    
    [Header("Debug")]
    [SerializeField] private bool _enableDebug = true;
    
    private SceneCharacterContainer _characterContainer;
    private SpatialGrid _spatialGrid;
    private float _updateTimer;
    private int _updateCounter;
    private bool _gridDirty = true;

    [Inject]
    private void Construct(SceneCharacterContainer characterContainer)
    {
        _characterContainer = characterContainer;
        _spatialGrid = new SpatialGrid(_cellSize);
        
        _updateTimer = 0f;
        _updateCounter = 0;
        _gridDirty = true;
    
        if (_enableDebug)
            Debug.Log("[VisionSystem] Constructed and initialized");
    }

    public void Tick()
    {
        _updateTimer += Time.deltaTime;
        
        if (_updateTimer >= _updateInterval)
        {
            _updateTimer = 0f;
            _gridDirty = true; 
        }
    }
    
    private void UpdateSpatialGrid()
    {
        _spatialGrid.Clear();
    
        var characters = _characterContainer.GetCharacters();
    
        foreach (var kvp in characters)
        {
            var character = kvp.Value;
        
            if (character == null || character.Core == null) continue;
            if (!character.Core) continue; // Unity == null для destroyed объектов
        
            var position = character.Core.transform.position;
            _spatialGrid.Add(position, character);
        }
    
        _updateCounter++;
        _gridDirty = false;
    }
   
    private void EnsureGridUpdated()
    {
        if (_gridDirty && _spatialGrid != null && _characterContainer != null)
            UpdateSpatialGrid();
    }
 
    public CharacterInfo GetClosestHostileCharacter(
        CharacterInfo observer, 
        float? visionRadius = null,
        float? visionAngle = null,
        bool checkLineOfSight = true)
    {
        EnsureGridUpdated(); 
        
        if (observer == null)
        {
            Debug.LogError("[VisionSystem] Observer is NULL!");
            return null;
        }
        
        if (observer.Core?.transform == null)
        {
            Debug.LogError($"[VisionSystem] Observer {observer.Name} has null Core or Transform!");
            return null;
        }
        
        if (observer.Faction == null)
        {
            Debug.LogError($"[VisionSystem] Observer {observer.Name} has NULL Faction!");
            return null;
        }
        
        var observerPos = observer.Core.transform.position;
        var observerForward = observer.Core.transform.forward;
        var observerFaction = observer.Faction;
        
        var radius = visionRadius ?? _defaultVisionRadius;
        var angle = visionAngle ?? _defaultVisionAngle;
        
        if (_enableDebug)
        {
            Debug.Log($"[VisionSystem] <color=cyan>{observer.Name} ({observerFaction.Name})</color> searching hostiles. " +
                      $"Radius: {radius:F1}, Angle: {angle:F1}, LOS: {checkLineOfSight}");
        }
        
        var nearbyCharacters = _spatialGrid.GetNearby(observerPos, radius);
        
        if (_enableDebug)
        {
            Debug.Log($"[VisionSystem] Found {nearbyCharacters.Count} nearby characters in grid");
        }
        
        CharacterInfo closestHostile = null;
        float closestDistance = float.MaxValue;
        
        foreach (var character in nearbyCharacters)
        {
            if (character == observer) continue;
    
            if (character == null || !character.Core) continue;
    
            if (character?.Faction == null) continue;
            if (character.Health == null || character.Health.IsDestroyed) continue;
            
            bool isHostile = observerFaction.IsHostileTo(character.Faction);
            
            if (_enableDebug)
            {
                Debug.Log($"[VisionSystem]   Checking: <color=yellow>{character.Name} ({character.Faction.Name})</color> " +
                          $"- IsHostile: {isHostile}, IsAlive: {!character.Health.IsDestroyed}");
            }
            
            if (!isHostile) continue;
            
            if (character.Core?.transform == null)
            {
                if (_enableDebug)
                    Debug.LogWarning($"[VisionSystem] Hostile {character.Name} has null Core/Transform!");
                continue;
            }
            
            var targetPos = character.Core.transform.position;
            var direction = targetPos - observerPos;
            var distance = direction.magnitude;
            
            if (_enableDebug)
            {
                Debug.Log($"[VisionSystem]     Distance: {distance:F2}m (max: {radius:F2}m)");
            }
            
            if (distance > radius) continue;
            
            var angleToTarget = Vector3.Angle(observerForward, direction);
            
            if (_enableDebug)
            {
                Debug.Log($"[VisionSystem]     Angle: {angleToTarget:F1}° (max: {angle * 0.5f:F1}°)");
            }
            
            if (angleToTarget > angle * 0.5f) continue;
            
            if (checkLineOfSight)
            {
                var rayStart = observerPos + Vector3.up * 1.5f;
                bool hasObstacle = Physics.Raycast(rayStart, direction.normalized, distance, _obstacleMask);
                
                if (_enableDebug)
                {
                    Debug.Log($"[VisionSystem]     Line of Sight: {!hasObstacle}");
                    if (hasObstacle)
                    {
                        Debug.DrawRay(rayStart, direction.normalized * distance, Color.red, 1f);
                    }
                    else
                    {
                        Debug.DrawRay(rayStart, direction.normalized * distance, Color.green, 1f);
                    }
                }
                
                if (hasObstacle) continue;
            }
            
            if (distance < closestDistance)
            {
                closestHostile = character;
                closestDistance = distance;
                
                if (_enableDebug)
                {
                    Debug.Log($"[VisionSystem]     <color=lime>✓ New closest hostile!</color>");
                }
            }
        }
        
        if (_enableDebug)
        {
            if (closestHostile != null)
            {
                Debug.Log($"[VisionSystem] <color=red>RESULT: {observer.Name} sees {closestHostile.Name} at {closestDistance:F2}m</color>");
            }
            else
            {
                Debug.Log($"[VisionSystem] <color=gray>RESULT: No hostile found</color>");
            }
        }
        
        return closestHostile;
    }
    
    // Получить всех враждебных и ЖИВЫХ персонажей в поле зрения
    public List<CharacterInfo> GetVisibleHostileCharacters(
        CharacterInfo observer,
        float? visionRadius = null,
        float? visionAngle = null,
        bool checkLineOfSight = true)
    {
        EnsureGridUpdated(); // Обновляем сетку перед поиском
        
        var results = new List<CharacterInfo>();
        var observerPos = observer.Core.transform.position;
        var observerForward = observer.Core.transform.forward;
        var observerFaction = observer.Faction;
        
        var radius = visionRadius ?? _defaultVisionRadius;
        var angle = visionAngle ?? _defaultVisionAngle;
        
        var nearbyCharacters = _spatialGrid.GetNearby(observerPos, radius);
        
        foreach (var character in nearbyCharacters)
        {
            if (character == observer) continue;
            
            // Проверка на живую цель
            if (character.Health == null || character.Health.IsDestroyed) continue;
            
            if (!observerFaction.IsHostileTo(character.Faction)) continue;
            
            var targetPos = character.Core.transform.position;
            var direction = targetPos - observerPos;
            var distance = direction.magnitude;
            
            if (distance > radius) continue;
            
            var angleToTarget = Vector3.Angle(observerForward, direction);
            if (angleToTarget > angle * 0.5f) continue;
            
            if (checkLineOfSight)
            {
                var rayStart = observerPos + Vector3.up * 1.5f;
                if (Physics.Raycast(rayStart, direction.normalized, distance, _obstacleMask))
                {
                    continue;
                }
            }
            
            results.Add(character);
        }
        
        return results;
    }

    // Быстрая проверка - является ли персонаж враждебным и видимым
    public bool CanSeeHostile(
        CharacterInfo observer,
        CharacterInfo target,
        float? visionRadius = null,
        float? visionAngle = null,
        bool checkLineOfSight = true)
    {
        EnsureGridUpdated(); // Обновляем сетку перед поиском
        
        // Проверка на живую цель
        if (target.Health == null || target.Health.IsDestroyed) return false;
        
        if (!observer.Faction.IsHostileTo(target.Faction)) return false;
        return CanSee(observer, target, visionRadius, visionAngle, checkLineOfSight);
    }
    
    // Основной метод - получить всех видимых персонажей
    public List<CharacterInfo> GetVisibleCharacters(
        CharacterInfo observer, 
        float? visionRadius = null,
        float? visionAngle = null,
        bool checkLineOfSight = true,
        bool excludeAI = false,
        bool onlyAlive = true)
    {
        EnsureGridUpdated(); // Обновляем сетку перед поиском
        
        var results = new List<CharacterInfo>();
        var observerPos = observer.Core.transform.position;
        var observerForward = observer.Core.transform.forward;
        
        var radius = visionRadius ?? _defaultVisionRadius;
        var angle = visionAngle ?? _defaultVisionAngle;
        
        var nearbyCharacters = _spatialGrid.GetNearby(observerPos, radius);
        
        foreach (var character in nearbyCharacters)
        {
            if (character == observer) continue;
            if (excludeAI && character.Core.IsAI) continue;
            
            // Проверка на живую цель
            if (onlyAlive && (character.Health == null || character.Health.IsDestroyed)) continue;
            
            var targetPos = character.Core.transform.position;
            var direction = targetPos - observerPos;
            var distance = direction.magnitude;
            
            if (distance > radius) continue;
            
            var angleToTarget = Vector3.Angle(observerForward, direction);
            if (angleToTarget > angle * 0.5f) continue;
            
            if (checkLineOfSight)
            {
                var rayStart = observerPos + Vector3.up * 1.5f;
                if (Physics.Raycast(rayStart, direction.normalized, distance, _obstacleMask))
                {
                    continue;
                }
            }
            
            results.Add(character);
        }
        
        return results;
    }
    
    // Найти ближайшего видимого персонажа
    public CharacterInfo GetClosestVisibleCharacter(
        CharacterInfo observer, 
        bool playerOnly = false,
        float? visionRadius = null,
        float? visionAngle = null,
        bool onlyAlive = true)
    {
        EnsureGridUpdated(); // Обновляем сетку перед поиском
        
        var observerPos = observer.Core.transform.position;
        var observerForward = observer.Core.transform.forward;
        
        var radius = visionRadius ?? _defaultVisionRadius;
        var angle = visionAngle ?? _defaultVisionAngle;
        
        var nearbyCharacters = _spatialGrid.GetNearby(observerPos, radius);
        
        CharacterInfo closest = null;
        float closestDistance = float.MaxValue;
        
        foreach (var character in nearbyCharacters)
        {
            if (character == observer) continue;
            if (playerOnly && character.Core.IsAI) continue;
            
            // Проверка на живую цель
            if (onlyAlive && (character.Health == null || character.Health.IsDestroyed)) continue;
            
            var targetPos = character.Core.transform.position;
            var direction = targetPos - observerPos;
            var distance = direction.magnitude;
            
            if (distance >= closestDistance || distance > radius) continue;
            
            var angleToTarget = Vector3.Angle(observerForward, direction);
            if (angleToTarget > angle * 0.5f) continue;
            
            var rayStart = observerPos + Vector3.up * 1.5f;
            if (!Physics.Raycast(rayStart, direction.normalized, distance, _obstacleMask))
            {
                closest = character;
                closestDistance = distance;
            }
        }
        
        return closest;
    }
    
    // Быстрая проверка видимости конкретного персонажа
    public bool CanSee(
        CharacterInfo observer, 
        CharacterInfo target,
        float? visionRadius = null,
        float? visionAngle = null,
        bool checkLineOfSight = true)
    {
        var observerPos = observer.Core.transform.position;
        var targetPos = target.Core.transform.position;
        var direction = targetPos - observerPos;
        var distance = direction.magnitude;
        
        var radius = visionRadius ?? _defaultVisionRadius;
        var angle = visionAngle ?? _defaultVisionAngle;
        
        if (distance > radius) return false;
        
        var angleToTarget = Vector3.Angle(observer.Core.transform.forward, direction);
        if (angleToTarget > angle * 0.5f) return false;
        
        if (checkLineOfSight)
        {
            var rayStart = observerPos + Vector3.up * 1.5f;
            return !Physics.Raycast(rayStart, direction.normalized, distance, _obstacleMask);
        }
        
        return true;
    }
    
    // Получить всех персонажей в радиусе (без проверки угла и LOS)
    public List<CharacterInfo> GetCharactersInRadius(Vector3 position, float radius, bool onlyAlive = true)
    {
        EnsureGridUpdated(); // Обновляем сетку перед поиском
        
        var allNearby = _spatialGrid.GetNearby(position, radius);
        
        if (!onlyAlive)
        {
            return allNearby;
        }
        
        // Фильтруем только живых
        var results = new List<CharacterInfo>();
        foreach (var character in allNearby)
        {
            if (character.Health != null && !character.Health.IsDestroyed)
            {
                results.Add(character);
            }
        }
        
        return results;
    }
    
    private void OnDrawGizmosSelected()
    {
        if (_spatialGrid != null)
        {
            _spatialGrid.DrawGizmos();
        }
    }
}

// Оптимизированная пространственная сетка
public class SpatialGrid
{
    private Dictionary<Vector2Int, List<CharacterInfo>> _grid;
    private readonly float _cellSize;
    
    public SpatialGrid(float cellSize)
    {
        _cellSize = cellSize;
        _grid = new Dictionary<Vector2Int, List<CharacterInfo>>();
    }
    
    public void Clear()
    {
        _grid.Clear(); 
    }
    
    public void Add(Vector3 position, CharacterInfo character)
    {
        var cellCoord = GetCellCoord(position);
        
        if (!_grid.TryGetValue(cellCoord, out var cell))
        {
            cell = new List<CharacterInfo>(8); // Предвыделяем память
            _grid[cellCoord] = cell;
        }
        
        cell.Add(character);
    }
    
    public List<CharacterInfo> GetNearby(Vector3 position, float radius)
    {
        var results = new List<CharacterInfo>();
        var centerCell = GetCellCoord(position);
        var cellRadius = Mathf.CeilToInt(radius / _cellSize);
        
        // Проверяем только ячейки в радиусе
        for (int x = -cellRadius; x <= cellRadius; x++)
        {
            for (int z = -cellRadius; z <= cellRadius; z++)
            {
                var cellCoord = new Vector2Int(centerCell.x + x, centerCell.y + z);
                
                if (_grid.TryGetValue(cellCoord, out var cell))
                {
                    results.AddRange(cell);
                }
            }
        }
        
        return results;
    }
    
    private Vector2Int GetCellCoord(Vector3 position)
    {
        return new Vector2Int(
            Mathf.FloorToInt(position.x / _cellSize),
            Mathf.FloorToInt(position.z / _cellSize)
        );
    }
    
    public void DrawGizmos()
    {
        if (_grid == null) return;
        
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        foreach (var kvp in _grid)
        {
            if (kvp.Value.Count == 0) continue;
            
            var worldPos = new Vector3(
                kvp.Key.x * _cellSize + _cellSize * 0.5f,
                0,
                kvp.Key.y * _cellSize + _cellSize * 0.5f
            );
            
            Gizmos.DrawWireCube(worldPos, new Vector3(_cellSize, 0.5f, _cellSize));
            
            // Показываем количество персонажей в ячейке
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(worldPos + Vector3.up, kvp.Value.Count.ToString());
            #endif
        }
    }
}