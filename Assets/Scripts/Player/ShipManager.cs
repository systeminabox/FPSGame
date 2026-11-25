using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class ShipManager : Singleton<ShipManager>
{
    public ShipController ship;
    public CinemachineCamera playerCam;
    private CinemachineCamera shipCam;
    private Camera weaponCam;
    private CinemachineBrain brain;
    public bool ControllingShip { get; private set;}

    private void Start()
    {
        ship = FindAnyObjectByType<ShipController>();
        
        var wea = GameObject.FindGameObjectWithTag("WeaponCam");
        if(wea != null)
            weaponCam = wea.GetComponent<Camera>();
        
        if(ship != null)
            shipCam = ship.CinemachineCamera;
        
        var playerCamGO = GameObject.FindGameObjectWithTag("PlayerCam");
        if(playerCamGO != null)
            playerCam = playerCamGO.GetComponent<CinemachineCamera>();
        
        
        brain = FindAnyObjectByType<CinemachineBrain>();
    }

    [DebugCommand("setControl", "setControl <bool toShip>")]
    public static void SetControl(bool toShip)
    {
        Instance.StartCoroutine(Instance.SwitchControl(toShip));
    }

    public IEnumerator SwitchControl(bool toShip)
    {
        if(!ship) yield break;
        if (toShip)
        {
            shipCam.Prioritize();
            ControllingShip = true;
            weaponCam.enabled = false;
            InputManager.Instance.SetMode("Ship");
            Debug.Log("Switched to ship");
        }
        else
        {
            playerCam.Prioritize();
            ControllingShip = false;
            
            yield return new WaitForEndOfFrame();
            yield return null;
            
            while (brain.IsBlending)
            {
                yield return null;
            }
            InputManager.Instance.SetMode("Player");
            weaponCam.enabled = true;
            Debug.Log("Switched to player");
        }
    }
    
    
}
