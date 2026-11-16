using UnityEngine;
using UnityEngine.UI;
using Inventory;

public class CraftingTerminalUI : MonoBehaviour
{
    
    [SerializeField] ItemSlot inputSlot1;
    [SerializeField] ItemSlot inputSlot2;
    [SerializeField] ItemSlot inputSlot3;
    [SerializeField] ItemSlot outputSlot;

    [SerializeField] private Image progressBar;
    [SerializeField] private GameObject panel;
    [SerializeField] private GameObject playerInventoryUI;
    
    private InventoryContainer _playerInventoryContainer;
    private bool open = false;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        
    }

    void RefreshCraftingUI()
    {
        _playerInventoryContainer = InventoryManager.Instance.playerInventoryContainer;
        if (playerInventoryUI)
        {
            var itemUIs = playerInventoryUI.GetComponentsInChildren<ItemUI>();
            foreach (var itemUI in itemUIs)
            {
                Destroy(itemUI.gameObject);
            }
            
            foreach (var item in _playerInventoryContainer.items)
            {
                var newItem = Resources.Load("PlayerItem") as GameObject;
                var newItemInstance = Instantiate(newItem, playerInventoryUI.transform);
                var itemUI = newItemInstance.GetComponent<ItemUI>();
                itemUI.UpdateUI(item.Key, item.Value);
            }
        }
    }
    
    
    

    // Update is called once per frame
    void Update()
    {
        if (InputManager.Instance.InventoryInput)
        {
            if(!open)
            {
                panel.SetActive(true);
                RefreshCraftingUI();
                open = true;
            }
            else
            {
                panel.SetActive(false);
                open = false;
            }
        }
        
    }
}
