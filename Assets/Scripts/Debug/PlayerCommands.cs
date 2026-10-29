using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static class PlayerCommands
{
    [DebugCommand("damagePlayer","damagePlayer <int damage>")]
    private static void DamagePlayer(int damage)
    {
        DebugLog.PrintLog("Damage player for " + damage + "");
        GameObject.Find("Player").GetComponent<BasicBody>().TakeDamage(damage);
    }

    

    

    [DebugCommand("loadScene", "loadScene <string sceneName>")]
    private static void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    [DebugCommand("saveDir")]
    private static void SaveDir()
    {
        DebugLog.PrintLog("Copied save directory to clipboard!");
        GUIUtility.systemCopyBuffer = Application.persistentDataPath;
    }

    [DebugCommand("identifyScene")]
    private static void IdentifyScene()
    {
        DebugLog.PrintLog("Identified scene: " + SceneManager.GetActiveScene().name);
    }
    
    [DebugCommand("quit")]
    private static void Quit()
    {
        Application.Quit();
    }
    
    

    
    
}
