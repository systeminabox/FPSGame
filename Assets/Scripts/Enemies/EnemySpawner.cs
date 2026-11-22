using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;


public class EnemySpawner : MonoBehaviour
{
    public int NumberOfEnemies = 5;
    public float SpawnInterval = 1f;
    public List<Enemy> EnemyPrefabs = new List<Enemy>();
    public SpawnMethod spawnMethod = SpawnMethod.RoundRobin;
    public float spawnRadius = 100f;
    
    
    private Dictionary<int, ObjectPool> EnemyObjectPools = new Dictionary<int, ObjectPool>();


    private void Awake()
    {
        for (int i = 0; i < EnemyPrefabs.Count; i++)
        {
            EnemyObjectPools.Add(i, ObjectPool.CreateInstance(EnemyPrefabs[i], NumberOfEnemies));
        }
    }

    private void Start()
    {
        
        StartCoroutine(SpawnEnemies());
    }

    private IEnumerator SpawnEnemies()
    {
        WaitForSeconds wait = new WaitForSeconds(SpawnInterval);

        yield return wait;

        int spawnedEnemies = 0;

        while (spawnedEnemies < NumberOfEnemies)
        {
            if (spawnMethod == SpawnMethod.RoundRobin)
            {
                SpawnRoundRobin(spawnedEnemies);
            }
            else if (spawnMethod == SpawnMethod.Random)
            {
                SpawnRandom();
            }
            
            spawnedEnemies++;
            
            yield return wait;
        }
    }
    
    private void SpawnRoundRobin(int spawnedEnemies)
    {
        int spawnIndex = spawnedEnemies % EnemyPrefabs.Count;
        
        DoSpawnEnemy(spawnIndex);
    }

    private void SpawnRandom()
    {
        DoSpawnEnemy(Random.Range(0, EnemyPrefabs.Count));
    }
    
    private void DoSpawnEnemy(int spawnIndex)
    {
        PoolableObject poolableObject = EnemyObjectPools[spawnIndex].GetObject();

        if (poolableObject != null)
        {
            Enemy enemy = poolableObject.GetComponent<Enemy>();
            
            
            
           

            var randomPosition = Random.insideUnitCircle * spawnRadius;
            Vector3 position = new Vector3(randomPosition.x + transform.position.x,transform.position.y , randomPosition.y + transform.position.z);
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(position, out hit, spawnRadius, NavMesh.AllAreas))
            {
                enemy.Agent.Warp(hit.position);
                //enable the enemy
                GameObject player = GameManager.Instance.Player;
                if (player)
                {
                    enemy.Movement.target = player.transform;
                    enemy.Agent.enabled = true;
                    enemy.Movement.StartChasing();
                }
                else
                {
                    Debug.LogError("No player found from GameManager when trying to spawn enemy. Did you forget to register the player?", gameObject);
                }
            }
            else
            {
                Debug.LogError($"Unable to place NavMeshAgent on NavMesh. Tried to use {position}.", gameObject);
            }
        }
        else
        {
            Debug.LogError($"Unable to fetch enemy of type {spawnIndex} from object pool. Out of objects?", gameObject);
        }
    }

    public enum SpawnMethod
    {
        RoundRobin,
        Random
    }
}
