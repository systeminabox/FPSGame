using System;
using Inventory;
using UnityEngine;

[Serializable]
public struct DroppedItem
{
    public ItemDefinition item;
    public int quantity;
}

public class EnemyBody : BasicBody
{
    [SerializeField]
    DroppedItem[] itemsDroppedOnDeath;
    
    

    protected override void HandleDeath()
    {
        foreach (var item in itemsDroppedOnDeath)
        {
            InventoryManager.Instance.playerInventoryContainer.AddItem(item.item, item.quantity);
        }
        
        base.HandleDeath();
    }
}
