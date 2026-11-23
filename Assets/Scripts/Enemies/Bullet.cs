using System;
using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class Bullet : AutoDestroyPoolableObject
{
    public float MoveSpeed = 10f;
    public int Damage = 1;
    public Rigidbody Rigidbody;
    public TrailRenderer TrailRenderer;

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        BasicBody body;

        if (other.TryGetComponent<BasicBody>(out body))
        {
            body.TakeDamage(Damage);
        }
        
        Debug.Log("Bullet hit " + other.name);
        
        Disable();
    }

    
    public override void Disable()
    {
        CancelInvoke(DisableMethodName);
        Rigidbody.linearVelocity = Vector3.zero;
        TrailRenderer.emitting = false;
        base.Disable();
    }

    public override void OnEnable()
    {
        base.OnEnable();
        StartCoroutine(Activate());
        
    }

    private IEnumerator Activate()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        TrailRenderer.emitting = true;
    }
}
