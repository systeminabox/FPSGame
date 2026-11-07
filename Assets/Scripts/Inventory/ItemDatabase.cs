using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Inventory
{
    [CreateAssetMenu(fileName = "New Item Database", menuName = "Inventory/Item Database", order = 0)]
    public class ItemDatabase : ScriptableObject
    {
        [SerializeField] private List<ItemDefinition> items = new List<ItemDefinition>();
        [SerializeField] private int nextID = 1;   // starts at 1 so 0 can mean "unassigned"

        // Runtime only, never serialized. Rebuilt from the list.
        private Dictionary<int, ItemDefinition> lookup;

        public ItemDefinition GetItem(int id)
        {
            if (lookup == null) BuildLookup();
            lookup.TryGetValue(id, out var item);
            return item;
        }

        private void BuildLookup()
        {
            lookup = new Dictionary<int, ItemDefinition>();
            foreach (var item in items)
            {
                if (item == null) continue;
                lookup[item.ID] = item;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            lookup = null; // force a rebuild after any edit

            // Keep the counter ahead of every ID already in use.
            foreach (var item in items)
            {
                if (item != null && item.ID >= nextID)
                    nextID = item.ID + 1;
            }

            var seenIDs = new Dictionary<int, ItemDefinition>();
            var seenObjects = new HashSet<ItemDefinition>();

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item == null)
                {
                    Debug.LogWarning($"{name}: empty entry at index {i}", this);
                    continue;
                }

                // Same asset listed twice (e.g. the inspector + button). Not an ID issue.
                if (!seenObjects.Add(item))
                {
                    Debug.LogWarning($"{name}: '{item.name}' is listed more than once (index {i}). Replace it with a different item.", this);
                    continue;
                }

                bool unassigned = item.ID == 0;
                bool duplicateID = !unassigned && seenIDs.ContainsKey(item.ID);

                if (unassigned || duplicateID)
                {
                    if (duplicateID)
                        Debug.LogWarning($"{name}: ID {item.ID} on '{item.name}' collides with '{seenIDs[item.ID].name}', reassigning.", item);

                    item.EditorSetID(nextID++);
                    EditorUtility.SetDirty(item);
                    EditorUtility.SetDirty(this);
                }

                seenIDs[item.ID] = item;
            }
        }
#endif
    }
}