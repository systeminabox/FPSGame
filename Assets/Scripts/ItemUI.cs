using Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour
{
    
    
    
    [SerializeField] TextMeshProUGUI itemName;
    [SerializeField] Image itemImage;
    [SerializeField] TextMeshProUGUI itemQuantity;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void UpdateUI(ItemDefinition item, int quantity)
    {
        if (itemName != null) itemName.text = item.DisplayName;
        if (itemQuantity != null) itemQuantity.text = quantity.ToString();
        if (itemImage != null) itemImage.sprite = item.Icon;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
