using UnityEngine;

public class SpringSpawner : MonoBehaviour
{
    [field: SerializeField] public SpringSpawnerData SpringSpawnerData { get; set; }
    
    private CharacterSpring _spawnedObject;

    private void Awake()
    {
        Spawn();
    }

    private void Spawn()
    {
        _spawnedObject = Instantiate(SpringSpawnerData.SpringPrefab, transform.position, Quaternion.identity).GetComponent<CharacterSpring>();
        
        _spawnedObject.Initialize(SpringSpawnerData.CharacterTransform, SpringSpawnerData.DeformationBodyTransform, SpringSpawnerData.CharacterRigidBody);
    }
}

[System.Serializable]
public struct SpringSpawnerData
{
    [field: SerializeField] public GameObject SpringPrefab { get; set; }
    [field: SerializeField] public Rigidbody CharacterRigidBody { get; set; }
    [field: SerializeField] public Transform CharacterTransform { get; set; }
    [field: SerializeField] public Transform DeformationBodyTransform { get; set; }
}
