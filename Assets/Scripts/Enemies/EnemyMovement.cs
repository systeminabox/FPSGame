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

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
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
            _agent.SetDestination(target.position);
            
            yield return wait;
        }
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
