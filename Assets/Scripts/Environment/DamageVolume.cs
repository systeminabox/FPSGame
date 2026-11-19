using System;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class DamageVolume : MonoBehaviour
{
    private BoxCollider _collider;

    private BasicBody _body;
    
    [SerializeField] private float damage = 10;

    private void Awake()
    {
        _collider = GetComponent<BoxCollider>();
        _collider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _body = other.GetComponent<BasicBody>();
            if (_body)
            {
                _body.TakeDamage(damage);
            }
            
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _body = null;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if(_body) _body.TakeDamage(damage * Time.deltaTime);
        }
    }
}
