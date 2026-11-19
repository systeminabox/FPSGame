using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Inventory
{
    [CustomEditor(typeof(InventoryContainer))]
    public class InventoryContainerEditor : Editor
    {
     
        private int _selectedItem = 0;
        private int _selectedCount = 1;
        
        private ItemDefinition[] _items;
        private GUIContent[] _labels; // index 0 = "None"

        private void OnEnable()
        {
            RefreshItems();
        }

        private void RefreshItems()
        {
            _items = AssetDatabase.FindAssets($"t:{nameof(ItemDefinition)}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<ItemDefinition>)
                .Where(item => item != null)
                .OrderBy(item => item.name)
                .ToArray();

            _labels = new[] { new GUIContent("None") }
                .Concat(_items.Select(item => new GUIContent(item.name)))
                .ToArray();
            
            
        }
        
        
        public override void OnInspectorGUI()
        {
            
            EditorGUILayout.Space();
            
            
            if (GUILayout.Button("Clear Items"))
            {
                var container = target as InventoryContainer;
                container.ClearItems();
            }
            
            //dropdown with each item??

            EditorGUILayout.BeginHorizontal();
            _selectedItem = EditorGUILayout.Popup(_selectedItem, _labels);
            _selectedCount = EditorGUILayout.IntField(_selectedCount);  
            
            if (GUILayout.Button("Add Item"))
            {
                var container = target as InventoryContainer;
                container.AddItem(_items[_selectedItem - 1], _selectedCount);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
            if(GUILayout.Button("Add 999 of every item"))
            {
                var container = target as InventoryContainer;
                foreach (var item in _items)
                {
                    container.AddItem(item, 999);
                }
            }
        }
    }
}