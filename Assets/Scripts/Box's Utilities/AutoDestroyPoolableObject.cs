using System;
using UnityEngine;


public class AutoDestroyPoolableObject : PoolableObject
{
    public int AutoDestroyAfterSeconds = 5;
    protected const string DisableMethodName = "Disable";

    public virtual void OnEnable()
    {
        CancelInvoke(DisableMethodName);
        Invoke(DisableMethodName, AutoDestroyAfterSeconds);
    }

    public virtual void Disable()
    {
        CancelInvoke(DisableMethodName);
        gameObject.SetActive(false);
    }
}
