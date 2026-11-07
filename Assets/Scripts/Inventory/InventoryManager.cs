using System;
using UnityEngine;

namespace Inventory
{
    public class InventoryManager : Singleton<InventoryManager>
    {
        public ItemDatabase itemDatabase;
        public Inventory playerInventory;

        private void Start()
        {
            if(GameManager.Instance.Player != null)
            {
                playerInventory = GameManager.Instance.Player.GetComponent<Inventory>();
            }
        }

        public ItemDefinition GetItem(int id)
        {
            return itemDatabase.GetItem(id);
        }
        
        [DebugCommand("getInventory")]
        private static void GetPlayerInventory()
        {
            if(Instance.playerInventory != null)
            {
                foreach (var item in Instance.playerInventory.items)
                {
                    DebugLog.PrintLog("Item: " + item.Key.name + " Count: " + item.Value + "");
                }
            }
        }
    }
}