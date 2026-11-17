using System.Collections.Generic;
using UnityEngine;



namespace Inventory
{
    public class InventoryContainer : MonoBehaviour
    {
        public Dictionary<ItemDefinition, int> items = new Dictionary<ItemDefinition, int>(); 
        
        public int GetItemQuantity(ItemDefinition item)
        {
            return items.ContainsKey(item) ? items[item] : 0;
        }

        public void AddItem(ItemDefinition item, int quantity = 1)
        {
            if (items.ContainsKey(item))
            {
                items[item] += quantity;
                Debug.Log("Item: " + item.name + " Count: " + items[item]);
            }
            else
            {
                items.Add(item, quantity);
                Debug.Log("Item: " + item.name + " Count: " + items[item]);
            }
        }

        public bool RemoveItem(ItemDefinition item, int quantity = 1)
        {
            if (items.ContainsKey(item))
            {
                if (items[item] - quantity <= 0) return false;
                items[item] -= quantity;
                return true;
            }
            return false;
        }
    }
}