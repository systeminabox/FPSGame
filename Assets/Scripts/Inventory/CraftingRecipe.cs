using Inventory;
using JetBrains.Annotations;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crafting Recipe", menuName = "Inventory/Crafting Recipe")]
public class CraftingRecipe : ScriptableObject
{
    public ItemDefinition input1;
    public int input1Count = 1;
    public ItemDefinition input2;
    public int input2Count = 1;
    [CanBeNull] public ItemDefinition input3;
    public int input3Count = 1;

    public ItemDefinition output;
    public int outputCount = 1;
}
