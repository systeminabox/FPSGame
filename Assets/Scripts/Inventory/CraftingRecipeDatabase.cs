using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Inventory
{
    [CreateAssetMenu(fileName = "New Crafting Recipe Database", menuName = "Inventory/Crafting Recipe Database")]
    public class CraftingRecipeDatabase : ScriptableObject
    {
        [SerializeField] private List<CraftingRecipe> _recipes = new List<CraftingRecipe>();

        public IReadOnlyList<CraftingRecipe> Recipes => _recipes;
        
        
#if UNITY_EDITOR
        [ContextMenu("Refresh Recipes")]
        public void Refresh()
        {
            var found = AssetDatabase.FindAssets($"t:{nameof(CraftingRecipe)}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<CraftingRecipe>)
                .Where(recipe => recipe != null)
                .OrderBy(recipe => recipe.name)
                .ToList();

            if (found.SequenceEqual(_recipes))
                return;

            _recipes = found;
            EditorUtility.SetDirty(this);
        }
#endif
    }
}