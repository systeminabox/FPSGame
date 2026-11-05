using UnityEngine;

public class ResourceBody : BasicBody
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
    
    public void Pickup()
    {
        Debug.Log("Pickup");
        Destroy(gameObject);
    }
}
