using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Base class for all enemies.
/// </summary>
[CreateAssetMenu(fileName = "New Enemy Config", menuName = "Enemy/Enemy Config")]
public class EnemyScriptableObject : ScriptableObject
{
    [Header("Enemy Stats")]
    public int health;
    
    [Header("Enemy NavMesh Agent")]
    
    public float BaseOffset = 0;
    
    [Header("Steering")]
    public float Speed = 3f;
    public float angularSpeed = 120f;
    public float acceleration = 8f;
    public float StoppingDistance = 0.5f;
    
    [Header("Obstacle Avoidance")]
    public float Radius = .5f;
    public float Height = 2f;
    public ObstacleAvoidanceType ObstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
    public int avoidancePriority = 50;
    
    [Header("Pathfinding")]
    public float AIPathUpdateInterval = 0.1f;
    public int areaMask = -1;
    
    
    

}
