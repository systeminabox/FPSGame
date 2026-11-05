
using System;
using System.Collections;
using UnityEngine;

public class PlayerMine: MonoBehaviour
{
    
    private Coroutine mineCoroutine;
    [SerializeField]private float mineCooldown = 0.5f;
    private Transform cameraTransform;
    [SerializeField]private float interactRange = 10f;
    private GameObject currentTarget;
    private float mineTimer;
    [SerializeField]private float mineDamage = 10f;
    [SerializeField]private LayerMask mineLayer;

    private void Start()
    {
        cameraTransform = Camera.main.transform;
        mineTimer = mineCooldown;
    }

    private void Update()
    {
        if (InputManager.Instance.PrimaryFireInput)
        {
            var didHit = PhysicsHelper.Raycast(cameraTransform, cameraTransform.forward, interactRange, out RaycastHit hit, mineLayer);
            if (didHit)
            {
                currentTarget = hit.collider.gameObject;
            }
            else
            {
                currentTarget = null;
            }
            
            if (currentTarget != null)
            {
                mineTimer -= Time.deltaTime;
                if (mineTimer <= 0)
                {
                    mineTimer += mineCooldown;
                    var resource = currentTarget.GetComponentInParent<ResourceBody>();
                    if (resource != null)
                    {
                        resource.TakeDamage(mineDamage);
                    }
                }
            }
            else
            {
                mineTimer = mineCooldown;
            }
        }
        else
        {
            currentTarget = null;
            mineTimer = mineCooldown;
        }
    }

    
}
