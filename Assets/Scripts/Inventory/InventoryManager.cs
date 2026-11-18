using System;
using UnityEngine;

namespace Inventory
{
    public class InventoryManager : Singleton<InventoryManager>
    {
        public ItemDatabase itemDatabase;
        public CraftingRecipeDatabase craftingRecipeDatabase;
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

        [DebugCommand("give", "give <item> <amount>")]
        public static void GiveItem(string itemName, int amount = 1)
        {
            if (Instance.playerInventoryContainer != null)
            {
                if (Instance.itemDatabase != null)
                {
                    var item = Instance.itemDatabase.GetItem(itemName);
                    if (item)
                    {
                        Instance.playerInventoryContainer.AddItem(item, amount);
                        DebugLog.PrintLog("Item added: " + itemName + " x " + amount);
                    }
                    else
                    {
                        DebugLog.PrintLog("Item not found: " + itemName + ". Item name must match internal name in database.", LogSeverity.Warning);
                    }
                }
                else
                {
                    DebugLog.PrintLog("Item database is null", LogSeverity.Error);
                }
            }
            else
            {
                DebugLog.PrintLog("Player inventory is null", LogSeverity.Error);
            }
        }
    }
}