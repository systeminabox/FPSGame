
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class RangedAttackRadius: AttackRadius
{
    
    
    
    private Coroutine _fireCoroutine;
    
    public NavMeshAgent agent;
    public Bullet bulletPrefab;
    
    public Muzzle[] bulletSpawnPoints;
    public LayerMask mask;
    public ObjectPool bulletPool;
    [SerializeField]
    private float sphereCastRadius = 0.1f;
    private RaycastHit hit;
    private BasicBody _targetBody;
    
    public int MaxAmmo  = 10;
    private int _shotsFired = 0;
    [Tooltip("Animation to play when reloading. Should be a trigger.")]
    [SerializeField]
    private string onReloadAnimation = "OnReload";
    public float ReloadDelay  = 1f;
    

    

    protected override void Awake()
    {
        base.Awake();
        
        bulletPool = ObjectPool.CreateInstance(bulletPrefab, Mathf.CeilToInt((1 / attackDelay) * bulletPrefab.AutoDestroyAfterSeconds));
        
        
    }

  

    

    protected override IEnumerator Attack()
    {
        WaitForSeconds wait = new WaitForSeconds(attackDelay);
        
        yield return wait;
        if (_animator)
        {
            _animator.SetBool(onAttackAnimation, true);
        }
        while (damageables.Count > 0)
        {
            
            for (int i = 0; i < damageables.Count; i++)
            {
                if (HasLineOfSightTo(damageables[i].transform))
                {
                    _targetBody = damageables[i];
                    OnAttack?.Invoke(damageables[i]);
                    agent.enabled = false;
                    break;
                }
            }

            if (_targetBody != null)
            {
                
                foreach (Muzzle bulletSpawnPoint in bulletSpawnPoints)
                {
                    if (_shotsFired >= MaxAmmo)
                    {
                        if (_animator != null)
                        {
                            _animator.SetTrigger(onReloadAnimation);
                            yield return new WaitForSeconds(ReloadDelay);
                        }
                        _shotsFired = 0;
                    }
                    
                    
                    PoolableObject poolableObject = bulletPool.GetObject();
                    if (poolableObject != null)
                    {
                        var bullet = poolableObject.GetComponent<Bullet>();

                        bullet.Damage = damage;

                        bullet.transform.position = bulletSpawnPoint.transform.position;
                        bullet.transform.rotation = bulletSpawnPoint.transform.rotation;
                        bulletSpawnPoint.PlayMuzzleEffect();
                        bullet.Rigidbody.AddForce(agent.transform.forward * bulletPrefab.MoveSpeed,
                            ForceMode.VelocityChange);
                        _shotsFired++;
                    }
                }
            }
            else
            {
                agent.enabled = true;
            }
            
            yield return wait;

            if (_targetBody == null || !HasLineOfSightTo(_targetBody.transform))
            {
                agent.enabled = true;
            }

            damageables.RemoveAll(DisabledDamageables);
        }
        if (_animator)
        {
            _animator.SetBool(onAttackAnimation, false);
        }
        agent.enabled = true;
        _attackCoroutine = null;
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);

        if (_attackCoroutine == null)
        {
            agent.enabled = true;
        }
    }
    
    private bool HasLineOfSightTo(Transform target)
    {
        Vector3 origin = bulletSpawnPoints[0].transform.position;

        // Aim at the middle of the player's collider instead of their pivot
        Collider targetCollider = target.GetComponentInChildren<Collider>();
        Vector3 aimPoint = targetCollider != null
            ? targetCollider.bounds.center
            : target.position + Vector3.up;

        Vector3 toTarget = aimPoint - origin;
        float distance = toTarget.magnitude;
        Vector3 direction = toTarget / distance;

        Debug.DrawRay(origin, toTarget, Color.red, 0.5f);

        // Distance is the real distance to the target plus a small margin
        if (Physics.SphereCast(origin, sphereCastRadius, direction, out RaycastHit hit,
                distance + 0.5f, mask, QueryTriggerInteraction.Ignore))
        {
            // Look up the hierarchy in case the collider is on a child
            BasicBody damageable = hit.collider.GetComponentInParent<BasicBody>();

            if (damageable != null && damageable.transform == target)
            {
                return true;
            }

            Debug.Log("Line of sight blocked by: " + hit.collider.name, gameObject);
            return false;
        }

        return false;
    }
}
