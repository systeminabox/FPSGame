using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    private static bool _quitting;

    public static GameManager Instance
    {
        get
        {
            if (_quitting) return null;

#if UNITY_EDITOR
            if (!Application.isPlaying) return null;
#endif

            if (_instance == null)
            {
                var prefab = Resources.Load<GameManager>("Managers");
                if (prefab == null)
                {
                    Debug.LogError("GameManager prefab not found at Resources/GameManager");
                    
                    Application.Quit();
                    return null;
                }
                _instance = Instantiate(prefab);
            }
            return _instance;
        }
    }

    // Systems that live on the GameManager prefab (assign in inspector).
    // Swap these for your real types as you build them.
    [Header("Persistent Systems")]
    

    // Scene-specific references, set by the objects themselves.
    public GameObject Player { get; private set; }
    

    
    
    public List<string> VisitedScenes;
    
    public bool DidIntroPlay = false;
    
    public float PlayerHealth;
    
    public void RegisterPlayer(GameObject player)
    {
        Player = player;
        Player.GetComponent<BasicBody>().OnHealthChanged += UpdatePlayerHealth;
    }

    private void UpdatePlayerHealth(HealthChangeInfo obj)
    {
        Debug.Log("Player health changed to " + obj.Percent);
        PlayerHealth = obj.Percent;
    }

    
    public void UnregisterPlayer(GameObject player)
    {
        if (Player == player) Player = null;
    }
    

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        
        
        // MoneyHandler.Instance.Init();
        //
        // TransitionController.Instance.Init();
        // DebugLog.Instance.Init();
        //
        // moneyHandler = MoneyHandler.Instance;
        // transitionController = TransitionController.Instance;
        // debugLog = DebugLog.Instance;
    }

    private void Update()
    {
        if (Keyboard.current.f5Key.wasPressedThisFrame)
        {
            //SaveSystem.Save();
        }
        if (Keyboard.current.f6Key.wasPressedThisFrame)
        {
            //SaveSystem.Load();
        }

        // if (SaveSystem.SaveExists())
        // {
        //     foreach(var gameObj in FindObjectsByType<EnabledIfSaveExists>(FindObjectsInactive.Include))
        //     {
        //         gameObj.gameObject.SetActive(true);
        //     }
        // }
        // else
        // {
        //     foreach(var gameObj in FindObjectsByType<EnabledIfSaveExists>(FindObjectsInactive.Include))
        //     {
        //         gameObj.gameObject.SetActive(false);    
        //     }
        // }
    }

    

    public void QuitGame()
    {
        Application.Quit();
    }

    public void DisablePlayer(bool disable)
    {
        // Player.canMove = !disable;
        // PlayerProjectileShooter.enabled = !disable;
    }
    // Keeps statics clean when Domain Reload is disabled in Enter Play Mode options.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        _instance = null;
        _quitting = false;
        Application.quitting -= OnQuitting;
        Application.quitting += OnQuitting;
    }

    [DebugCommand("save")]
    public static void Save()
    {
        //SaveSystem.Save();
    }

    [DebugCommand("load")]
    public static void Load()
    {
        //SaveSystem.Load();
    }

    private static void OnQuitting() => _quitting = true;
}