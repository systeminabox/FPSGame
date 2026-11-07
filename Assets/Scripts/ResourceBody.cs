using Inventory;
using UnityEngine;

public class ResourceBody : BasicBody
{
    public ItemDefinition itemDefinition;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
    
    public void Pickup()
    {
        Debug.Log("Pickup");
        InventoryManager.Instance.playerInventory.AddItem(itemDefinition);
        Destroy(gameObject);
    }
}
