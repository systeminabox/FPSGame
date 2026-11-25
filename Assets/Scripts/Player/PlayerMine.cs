
using System;
using System.Collections;
using UnityEngine;

public class PlayerMine: MonoBehaviour
{
    
    private Coroutine mineCoroutine;
    [SerializeField]private float mineCooldown = 0.5f;
    [SerializeField]private float interactRange = 10f;
    private GameObject currentTarget;
    private float mineTimer;
    [SerializeField]private float mineDamage = 10f;
    [SerializeField]private LayerMask mineLayer;
    [SerializeField] private Transform mineRoot;

    [SerializeField] private LaserBeam laserBeam;
    
    private void Start()
    {
        if(mineRoot == null) mineRoot = transform;
        mineTimer = mineCooldown;
    }

    private void Update()
    {
        if (InputManager.Instance.PrimaryFireInput)
        {
            var didHit = PhysicsHelper.Raycast(mineRoot, mineRoot.forward, interactRange, out RaycastHit hit, mineLayer);
            laserBeam.SetActive(true);
            laserBeam.UpdateSpline(mineRoot.position, mineRoot.position + mineRoot.forward * interactRange);
            
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
                    var resource = currentTarget.GetComponentInParent<BasicBody>();
                    if (resource != null)
                    {
                        resource.TakeDamage(mineDamage);
                        resource.OnDeath += () => { currentTarget = null; };
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
            laserBeam.SetActive(false);
        }
    }

    
}
