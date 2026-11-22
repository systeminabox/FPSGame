
using UnityEngine.AI;

public class Enemy: PoolableObject
{
    public EnemyMovement Movement;
    public NavMeshAgent Agent;
    public BasicBody Body;
    public EnemyScriptableObject Config;

    public virtual void OnEnable()
    {
        SetupAgentFromConfig();
    }
    
    public override void OnDisable()
    {
        base.OnDisable();
        
        Agent.enabled = false;
    }
    
    public virtual void SetupAgentFromConfig()
    {
        Agent.speed = Config.Speed;
        Agent.acceleration = Config.acceleration;
        Agent.angularSpeed = Config.angularSpeed;
        Agent.areaMask = Config.areaMask;
        Agent.avoidancePriority = Config.avoidancePriority;
        Agent.baseOffset = Config.BaseOffset;
        Agent.height = Config.Height;
        Agent.obstacleAvoidanceType = Config.ObstacleAvoidanceType;
        Agent.radius = Config.Radius;
        Agent.stoppingDistance = Config.StoppingDistance;
        
        Movement.updateSpeed = Config.AIPathUpdateInterval;
        
        Body.Initialize(Config.health);
    }
}
