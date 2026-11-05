using System;
using UnityEngine;

public class FollowSun : MonoBehaviour
{
    
    [SerializeField] Transform sun;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private void OnValidate()
    {
        if(sun != null) transform.rotation = sun.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if(sun != null) transform.rotation = sun.rotation;
    }
}
