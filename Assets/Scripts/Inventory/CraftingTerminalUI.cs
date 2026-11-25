using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Inventory;
using TMPro;

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
    
    [SerializeField] private bool isCrafting = false;
    
    [SerializeField] private float timePerCraft = 1f;
    
    private CraftingRecipe _targetRecipe;
    private int maxCraftable = 0;
    
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
                OpenPlayerInventory(true);
            }
            else
            {
                OpenCraftingUI(false);
            }
        }
        
    }

    public void OpenPlayerInventory(bool shouldOpen = true)
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

    public void OpenCraftingUI(bool shouldOpen = true)
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
    
    public void ClearRecipeSelection()
    {
        inputSlot1.UpdateItem(null,0);
        inputSlot2.UpdateItem(null,0);
        inputSlot3.UpdateItem(null,0);
        outputSlot.UpdateItem(null,0);
    }

    void RecipeBuilder()
    {

        RectTransform children = recipeScrollRect.content.GetComponentInChildren<RectTransform>();
        foreach (RectTransform child in children)
        {
            Destroy(child.gameObject);
        }

        var recipes = InventoryManager.Instance.craftingRecipeDatabase.Recipes;

        var playerItems = _playerInventoryContainer.items;
        
        if (recipes.Count > 0)
        {
            foreach (var craftingRecipe in recipes)
            {
                //check if all inputs are in player inventory
                bool allInputsInInventory = false;
                
                bool containsInput1 = false;
                bool containsInput2 = false;
                bool containsInput3 = false;
                if(playerItems.ContainsKey(craftingRecipe.input1)) containsInput1 = true;
                if(playerItems.ContainsKey(craftingRecipe.input2)) containsInput2 = true;
                if (craftingRecipe.input3 != null)
                {
                    containsInput3 = playerItems.ContainsKey(craftingRecipe.input3);
                }
                else
                {
                    containsInput3 = true;
                }
                
                allInputsInInventory = containsInput1 && containsInput2 && containsInput3;
                
                if(!allInputsInInventory) continue;
                
                var newItem = Resources.Load("PlayerItem") as GameObject;
                var newItemInstance = Instantiate(newItem, recipeScrollRect.content.transform);
                var itemUI = newItemInstance.GetComponent<ItemUI>();
                
                itemUI.itemButton.onClick.AddListener(() => SetTargetRecipe(craftingRecipe));
                
                itemUI.UpdateUI(craftingRecipe.output, 0);
                
                
                
                
                
                
                
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(recipeScrollRect.content);
            if(recipeScrollRect.verticalNormalizedPosition <= .3f)
                recipeScrollRect.verticalNormalizedPosition = 0;
        }
    }

    private void SetTargetRecipe(CraftingRecipe recipe)
    {
        CloseRecipePicker();
        
        if(recipe == null) return;
        _targetRecipe = recipe;
        
        //pull info from player inventory
        
        var playerinput1 = _playerInventoryContainer.items[recipe.input1]/recipe.input1Count;
        var playerinput2 = _playerInventoryContainer.items[recipe.input2]/recipe.input2Count;
        int playerinput3 = -1;
        
        inputSlot1.UpdateItem(recipe.input1,playerinput1);
        inputSlot2.UpdateItem(recipe.input2,playerinput2);

        maxCraftable = 0;
        
        if (recipe.input3 != null)
        {
            playerinput3 = _playerInventoryContainer.items[recipe.input3]/recipe.input3Count;
            inputSlot3.UpdateItem(recipe.input3,playerinput3);
            maxCraftable = Mathf.Min(playerinput1, playerinput2, playerinput3);
        }
        else
        {
            inputSlot3.UpdateItem(null,0);
            playerinput3 = 0;
            maxCraftable = Mathf.Min(playerinput1, playerinput2);
        }
        
        
        
        outputSlot.UpdateItem(recipe.output,maxCraftable);
        
    }

    public void Craft()
    {
        Debug.Log("Checking if we can craft");
        if(_targetRecipe == null) return;
        if (isCrafting) return;
        
        Debug.Log("Crafting");
        
        isCrafting = true;
        var maxTime = maxCraftable * timePerCraft;
        StartCoroutine(CraftRoutine());
        StartCoroutine(AnimateProgressBar(maxTime));
        
    }
    
    private IEnumerator AnimateProgressBar(float maxTime)
    {
        yield return null;
        progressBar.fillAmount = 0;
        var startTime = Time.time;
        while (Time.time - startTime < maxTime)
        {
            progressBar.fillAmount = (Time.time - startTime) / maxTime;
            yield return null;
        }
        progressBar.fillAmount = 1;
    }

    private IEnumerator CraftRoutine()
    {
        Debug.Log("Crafting on next frame");
        yield return null;

        int quantityPerCraftInput1 = _targetRecipe.input1Count;
        int quantityPerCraftInput2 = _targetRecipe.input2Count;
        int quantityPerCraftInput3 = _targetRecipe.input3Count;
        int quantityPerCraftOutput = _targetRecipe.outputCount;

        for (int i = 0; i < maxCraftable; i++)
        {
            RefreshCraftingUI();
            Debug.Log("Crafting item " + i);
            yield return new WaitForSeconds(timePerCraft);
            _playerInventoryContainer.RemoveItem(_targetRecipe.input1, quantityPerCraftInput1);
            inputSlot1.UpdateItem(_targetRecipe.input1, _playerInventoryContainer.GetItemQuantity( _targetRecipe.input1));
            _playerInventoryContainer.RemoveItem(_targetRecipe.input2, quantityPerCraftInput2);
            inputSlot2.UpdateItem(_targetRecipe.input2, _playerInventoryContainer.GetItemQuantity( _targetRecipe.input2));
            if (_targetRecipe.input3 != null)
            {
                _playerInventoryContainer.RemoveItem(_targetRecipe.input3, quantityPerCraftInput3);
                inputSlot3.UpdateItem(_targetRecipe.input3, _playerInventoryContainer.GetItemQuantity( _targetRecipe.input3));
            }
            _playerInventoryContainer.AddItem(_targetRecipe.output, quantityPerCraftOutput);
            RefreshCraftingUI();
        }
        RefreshCraftingUI();
        isCrafting = false;
    }
}
