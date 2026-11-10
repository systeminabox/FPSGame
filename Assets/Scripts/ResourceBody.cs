using Inventory;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshCollider), typeof(MeshRenderer))]
public class ResourceBody : BasicBody
{
    public ItemDefinition itemDefinition;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public MeshCollider meshCollider;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        meshRenderer = GetComponent<MeshRenderer>();

        if (itemDefinition.resourceMesh)
        {
            meshFilter.mesh = itemDefinition.resourceMesh;
            meshCollider.sharedMesh = itemDefinition.resourceMesh;
            if (itemDefinition.resourceMaterials.Length >= 1)
            {
                meshRenderer.materials = itemDefinition.resourceMaterials;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    protected override void HandleDeath()
    {
        Debug.Log("Pickup");
        InventoryManager.Instance.playerInventory.AddItem(itemDefinition);
        Destroy(gameObject);
        base.HandleDeath();
    }

    
}
