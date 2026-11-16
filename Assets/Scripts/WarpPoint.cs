using System;
using System.Collections;
using StarterAssets;
using UnityEngine;


public class WarpPoint : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(LateStart());
    }
    
    private IEnumerator LateStart()
    {
        yield return null;
        GameManager.Instance.Player.transform.position = transform.position;
    }
    
    
}
