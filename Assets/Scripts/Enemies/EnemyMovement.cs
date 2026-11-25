using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMovement : MonoBehaviour
{
    public Transform target;
    public float updateSpeed = .1f; //How frequent we recalc path based on target position
    
    protected NavMeshAgent _agent;
    
    
    private Coroutine _chaseCoroutine;
    
    [SerializeField]
    protected Animator animator;

    public float turnSpeed = 4f;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (target == null)
        {
            target = GameManager.Instance.Player.transform;
        }
    }

    public void StartChasing()
    {
        if( _chaseCoroutine == null)
        {
            _chaseCoroutine = StartCoroutine(FollowTarget());
        }
        else
        {
            Debug.LogWarning("Called StartChasing when already chasing. This is probably a bug in the calling class", gameObject);
        }
    }

    private IEnumerator FollowTarget()
    {
        WaitForSeconds wait = new WaitForSeconds(updateSpeed);
        while (enabled)
        {
            if(_agent.enabled)
            {
                _agent.SetDestination(target.position);
            }
            else
            {
                RotateTowardsTarget();
            }
            
            
            yield return wait;
        }
    }
    
    private void RotateTowardsTarget()
    {
        // Direction from the enemy to the target, flattened so it doesn't tilt up/down
        Vector3 toTarget = target.position - transform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude < 0.001f)
        {
            animator.SetFloat("Angular", 0f, 0.1f, Time.deltaTime);
            return;
        }

        // Signed angle between where we face and where we want to face (-180 to 180)
        float signedAngle = Vector3.SignedAngle(transform.forward, toTarget, Vector3.up);

        // Rotate towards the target at a fixed turn speed (degrees per second)
        Quaternion targetRotation = Quaternion.LookRotation(toTarget, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

        // Normalize to -1..1 (negative = turning left, positive = turning right)
        float angular = Mathf.Clamp(signedAngle / 180f, -1f, 1f);

        // The damp time smooths the value so the animator blend doesn't jitter
        animator.SetFloat("Angular", angular, 0.1f, Time.deltaTime);
    }

    

    protected virtual void OnAttack()
    {
        Debug.Log("Attacking");
    }

    protected virtual void OnSeeTarget()
    {
        Debug.Log("Seeing target");
    }
    
    protected virtual void OnLoseTarget()
    {
        Debug.Log("Lost target");
    }

    protected virtual void OnReload()
    {
        Debug.Log("Reloading");
    }
}
