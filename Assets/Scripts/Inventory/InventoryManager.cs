using System;
using UnityEngine;

namespace Inventory
{
    public class InventoryManager : Singleton<InventoryManager>
    {
        public ItemDatabase itemDatabase;
        public InventoryContainer playerInventoryContainer;

        private void Start()
        {
            if(GameManager.Instance.Player != null)
            {
                playerInventoryContainer = GameManager.Instance.Player.GetComponent<InventoryContainer>();
            }
        }

        public ItemDefinition GetItem(int id)
        {
            return itemDatabase.GetItem(id);
        }
        
        [DebugCommand("getInventory")]
        private static void GetPlayerInventory()
        {
            if(Instance.playerInventoryContainer != null)
            {
                DebugLog.PrintLog("Player inventory size: " + Instance.playerInventoryContainer.items.Count);
                foreach (var item in Instance.playerInventoryContainer.items)
                {
                    DebugLog.PrintLog("Item: " + item.Key.name + " Count: " + item.Value + "");
                }
                
            }
            else
            {
                DebugLog.PrintLog("Player inventory is null");
            }
        }
    }
}