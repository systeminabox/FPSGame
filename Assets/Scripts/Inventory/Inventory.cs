using System.Collections.Generic;
using UnityEngine;



namespace Inventory
{
    public class Inventory : MonoBehaviour
    {
        public Dictionary<ItemDefinition, int> items = new Dictionary<ItemDefinition, int>(); 
        
        public int GetItemQuantity(ItemDefinition item)
        {
            return items.ContainsKey(item) ? items[item] : 0;
        }

        public void AddItem(ItemDefinition item)
        {
            if (items.ContainsKey(item))
            {
                items[item]++;
            }
            else
            {
                items.Add(item, 1);
            }
        }

        public bool RemoveItem(ItemDefinition item)
        {
            if (items.ContainsKey(item))
            {
                if (items[item] - 1 <= 0) return false;
                items[item]--;
                return true;
            }
            return false;
        }
    }
}