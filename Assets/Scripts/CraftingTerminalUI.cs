using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Inventory;
using TMPro;
using UnityEditor;

public class CraftingTerminalUI : MonoBehaviour
{
    
    [SerializeField] ItemSlot inputSlot1;
    [SerializeField] ItemSlot inputSlot2;
    [SerializeField] ItemSlot inputSlot3;
    [SerializeField] ItemSlot outputSlot;

    [SerializeField] private Image progressBar;
    [SerializeField] private GameObject panel;
    [SerializeField] private GameObject playerInventoryUI;

    [SerializeField] private GameObject crafterInventoryUI;
    [SerializeField] private GameObject recipePickerUI;
    
    private InventoryContainer _playerInventoryContainer;
    private bool open = false;
    
    [SerializeField] private ScrollRect recipeScrollRect;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        
    }

    void RefreshCraftingUI()
    {
        _playerInventoryContainer = InventoryManager.Instance.playerInventoryContainer;
        if(!_playerInventoryContainer) Debug.LogError("No player inventory container found");
        if (playerInventoryUI)
        {
            var itemUIs = playerInventoryUI.GetComponentsInChildren<ItemUI>();

            if (itemUIs != null || itemUIs.Length > 0)
            {
                foreach (var itemUI in itemUIs)
                {
                    Destroy(itemUI.gameObject);
                }
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
                OpenCraftingUI(true);
            }
            else
            {
                OpenCraftingUI(false);
            }
        }
        
    }

    void OpenPlayerInventory(bool shouldOpen = true)
    {
        panel.SetActive(shouldOpen);
        if (shouldOpen)
        {
            playerInventoryUI.SetActive(true);
            crafterInventoryUI.SetActive(false);
            InputManager.SetCursorState(false);
            RefreshCraftingUI();
            open = true;
        }
        else
        {
            playerInventoryUI.SetActive(false);
            crafterInventoryUI.SetActive(false);
            InputManager.SetCursorState(true);
            CloseRecipePicker();
            open = false;
        }
        
    }

    void OpenCraftingUI(bool shouldOpen = true)
    {
        panel.SetActive(shouldOpen);
        if (shouldOpen)
        {
            crafterInventoryUI.SetActive(true);
            playerInventoryUI.SetActive(true);
            RefreshCraftingUI();
            InputManager.SetCursorState(false);
            open = true;
        }
        else
        {
            crafterInventoryUI.SetActive(false);
            playerInventoryUI.SetActive(false);
            CloseRecipePicker();
            InputManager.SetCursorState(true);
            open = false;
        }
    }

    public void OpenRecipePicker()
    {
        panel.SetActive(true);
        recipePickerUI.SetActive(true);
        RecipeBuilder();
    }

    public void CloseRecipePicker()
    {
        recipePickerUI.SetActive(false);
    }

    void RecipeBuilder()
    {

        RectTransform children = recipeScrollRect.content.GetComponentInChildren<RectTransform>();
        foreach (RectTransform child in children)
        {
            Destroy(child.gameObject);
        }

        var _items = InventoryManager.Instance.craftingRecipeDatabase.Recipes;

        if (_items.Count > 0)
        {
            foreach (var item in _items)
            {
                var newItem = Resources.Load("PlayerItem") as GameObject;
                var newItemInstance = Instantiate(newItem, recipeScrollRect.content.transform);
                var itemUI = newItemInstance.GetComponent<ItemUI>();
                
                itemUI.itemButton.onClick.AddListener(() => SetTargetRecipe(item));
                
                itemUI.UpdateUI(item.output, 0);
                
                
                
                
                
                
                
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(recipeScrollRect.content);
            if(recipeScrollRect.verticalNormalizedPosition <= .3f)
                recipeScrollRect.verticalNormalizedPosition = 0;
        }
    }

    private void SetTargetRecipe(CraftingRecipe recipe)
    {
        CloseRecipePicker();
        
        inputSlot1.UpdateItem(recipe.input1,1);
        inputSlot2.UpdateItem(recipe.input2,1);

        if (recipe.input3 != null)
        {
            inputSlot3.UpdateItem(recipe.input3,1);
        }
        outputSlot.UpdateItem(recipe.output,1);
        
    }
}
