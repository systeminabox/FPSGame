using UnityEngine;

namespace Inventory
{
    public class InventoryManager : Singleton<InventoryManager>
    {
        public ItemDatabase itemDatabase;
        public Inventory playerInventory;
        
        public ItemDefinition GetItem(int id)
        {
            return itemDatabase.GetItem(id);
        }
    }
}