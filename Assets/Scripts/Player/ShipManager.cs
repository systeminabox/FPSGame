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
        weaponCam = GameObject.FindGameObjectWithTag("WeaponCam").GetComponent<Camera>();
        shipCam = ship.cam;
        playerCam = GameObject.FindGameObjectWithTag("PlayerCam").GetComponent<CinemachineCamera>();
        brain = FindAnyObjectByType<CinemachineBrain>();
    }

    [DebugCommand("setControl", "setControl <bool toShip>")]
    public static void SetControl(bool toShip)
    {
        Instance.StartCoroutine(Instance.SwitchControl(toShip));
    }

    public IEnumerator SwitchControl(bool toShip)
    {
        if (toShip)
        {
            shipCam.Prioritize();
            ControllingShip = true;
            weaponCam.enabled = false;
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
            weaponCam.enabled = true;
            Debug.Log("Switched to player");
        }
    }
    
    
}
