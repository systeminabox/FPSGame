using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Splines;
using Spline = UnityEngine.Rendering.PostProcessing.Spline;

public class LaserBeam : MonoBehaviour
{
    public Vector3 startPos;
    public Vector3 endPos;
    
    public LineRenderer lineRenderer;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        lineRenderer.SetPosition(0, transform.position);
    }

    public void UpdateSpline(Vector3 start, Vector3 end)
    {
        //convert parent position to world space
        
        
        
        
        lineRenderer.SetPosition(1, end);
        
    }
    
    public void SetActive(bool active)
    {
        lineRenderer.enabled = active;
    }
}
