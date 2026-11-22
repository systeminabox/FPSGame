using System.Collections.Generic;
using UnityEngine;

public class ObjectPool
{
    private PoolableObject _prefab;
    private List<PoolableObject> _availableObjects;

    private ObjectPool(PoolableObject prefab, int size)
    {
        this._prefab = prefab;
        _availableObjects = new List<PoolableObject>(size);
    }
    
    
    /// <summary>
    /// Creates a new pool of objects of the given size.
    /// </summary>
    /// <param name="prefab">This is the prefab that will be used to create the pool. It can be any object that has a PoolableObject inherited component.</param>
    /// <param name="size">This is the size of the pool. It determines how many objects will be created and stored in the pool.</param>
    /// <returns>Returns a new ObjectPool instance.</returns>
    public static ObjectPool CreateInstance(PoolableObject prefab, int size)
    {
        ObjectPool pool = new ObjectPool(prefab, size);
        
        GameObject poolObject = new GameObject(prefab + " Pool");
        
        pool.CreateObjects(poolObject.transform, size);
        
        return pool;
    }
    /// <summary>
    /// Creates the objects in the pool.
    /// </summary>
    /// <param name="parent">This is the parent transform that the objects will be instantiated under.</param>
    /// <param name="size">This is the number of objects to create and add to the pool.</param>
    private void CreateObjects(Transform parent, int size)
    {
        for (int i = 0; i < size; i++)
        {
            PoolableObject poolableObject = GameObject.Instantiate(_prefab, Vector3.zero, Quaternion.identity, parent.transform);
            poolableObject.Parent = this;
            poolableObject.gameObject.SetActive(false);
        }
    }
    /// <summary>
    /// Returns an object to the pool. Typically called by the object that was just deactivated.
    /// </summary>
    /// <param name="poolableObject">This is the object that will be returned to the pool.</param>
    public void ReturnObjectToPool(PoolableObject poolableObject)
    {
        _availableObjects.Add(poolableObject);
        poolableObject.gameObject.SetActive(false);
    }

    /// <summary>
    /// Pops the first object from the pool. This removes it while it is active.   
    /// </summary>
    /// <returns>The object that was removed from the pool. Null if the pool is empty.</returns>
    public PoolableObject GetObject()
    {
        if (_availableObjects.Count > 0)
        {
            PoolableObject instance = _availableObjects[0];
            _availableObjects.RemoveAt(0);
            
            instance.gameObject.SetActive(true);
            
            return instance;
        }
        else
        {
            Debug.LogError($"Could not get an object from pool \"{_prefab.name}\" Pool. Probably a configuration issue.");
            return null;
        }
    }
}
