using Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    [SerializeField] Image image;
    [SerializeField] TextMeshProUGUI itemName;
    [SerializeField] TextMeshProUGUI itemQuantity;
    [SerializeField] private int quantity;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void UpdateItem(ItemDefinition item, int quantityIn)
    {
        this.quantity = quantityIn;
        if(image != null) image.sprite = item.Icon;
        if(itemName != null) itemName.text = item.DisplayName;
        if(itemQuantity != null) itemQuantity.text = quantity.ToString();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
