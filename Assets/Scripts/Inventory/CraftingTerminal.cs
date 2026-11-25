using UnityEngine;

public class CraftingTerminal : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenCraftingUI(bool open)
    {
        var craftingUI = FindAnyObjectByType<CraftingTerminalUI>();
        craftingUI.OpenCraftingUI(open);
    }
}
