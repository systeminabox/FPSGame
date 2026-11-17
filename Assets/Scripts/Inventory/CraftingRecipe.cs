using Inventory;
using JetBrains.Annotations;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crafting Recipe", menuName = "Inventory/Crafting Recipe")]
public class CraftingRecipe : ScriptableObject
{
    public ItemDefinition input1;
    public ItemDefinition input2;
    [CanBeNull] public ItemDefinition input3;

    public ItemDefinition output;
}
