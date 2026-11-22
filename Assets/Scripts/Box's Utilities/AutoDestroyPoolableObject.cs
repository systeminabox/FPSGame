using System;
using UnityEngine;


public class AutoDestroyPoolableObject : PoolableObject
{
    public int AutoDestroyAfterSeconds = 5;
    private const string DisableMethodName = "Disable";

    public void OnEnable()
    {
        CancelInvoke(DisableMethodName);
        Invoke(DisableMethodName, AutoDestroyAfterSeconds);
    }

    public virtual void Disable()
    {
        gameObject.SetActive(false);
    }
}
