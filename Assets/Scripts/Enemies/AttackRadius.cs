using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class AttackRadius : MonoBehaviour
{
    public SphereCollider sphereCollider;
    protected List<BasicBody> damageables = new List<BasicBody>();
    public int damage = 10;
    public float attackDelay = .5f;

    public delegate void AttackEvent(BasicBody Target);
    public AttackEvent OnAttack;
    
    protected Coroutine _attackCoroutine;
    protected Animator _animator;
    
    [Tooltip("Animation to play when gaining vision. Should be a Trigger.")]
    [SerializeField] 
    protected string onVisionAnimation = "OnVision";
    [Tooltip("Animation to play when losing vision. Should be a trigger.")]
    [SerializeField] 
    protected string onLoseVisionAnimation = "OnLoseVision";
    [Tooltip("Animation to play when attacking. Should be a bool parameter.")]
    [SerializeField]
    protected string onAttackAnimation = "OnAttack";
    

    protected virtual void Awake()
    {
        sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.isTrigger = true;
        _animator = GetComponent<Animator>();
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        BasicBody body = other.GetComponent<BasicBody>();
        if (body)
        {
            damageables.Add(body);

            if (_attackCoroutine == null)
            {
                _attackCoroutine = StartCoroutine(Attack());
                if (_animator != null)
                {
                    _animator.SetTrigger(onVisionAnimation);
                }
                
            }
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        BasicBody body = other.GetComponent<BasicBody>();
        if (body)
        {
            damageables.Remove(body);
            if (damageables.Count == 0)
            {
                StopCoroutine(_attackCoroutine);
                _attackCoroutine = null;
                if (_animator != null)
                {
                    _animator.SetTrigger(onLoseVisionAnimation);
                }
            }
        }
    }

    protected virtual IEnumerator Attack()
    {
        WaitForSeconds wait = new WaitForSeconds(attackDelay);
        
        yield return wait;
        if(_animator != null) _animator.SetBool(onAttackAnimation, true);

        BasicBody closestDamageable = null;
        float closestDistance = float.MaxValue;
        while (damageables.Count > 0)
        {
            for(int  i = 0; i < damageables.Count; i++)
            {
                float distance = Vector3.Distance(transform.position, damageables[i].transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestDamageable = damageables[i];
                }
            }
            if (closestDamageable)
            {
                OnAttack?.Invoke(closestDamageable);
            }
            
            closestDamageable = null;
            closestDistance = float.MaxValue;
            
            yield return wait;

            damageables.RemoveAll(DisabledDamageables);
        }
        _animator.SetBool(onAttackAnimation, false);
        _attackCoroutine = null;

    }
    
    protected bool DisabledDamageables(BasicBody body)
    {
        return body != null && !body.gameObject.activeSelf;
    }
}
