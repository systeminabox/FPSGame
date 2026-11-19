using Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour
{
    
    
    
    [SerializeField] TextMeshProUGUI itemName;
    [SerializeField] Image itemImage;
    [SerializeField] TextMeshProUGUI itemQuantity;
    [SerializeField] public Button itemButton;
    
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void UpdateUI(ItemDefinition item, int quantity)
    {
        if (itemName != null) itemName.text = item.DisplayName;
        if (itemQuantity != null)
        {
            if (quantity > 1) itemQuantity.text = quantity.ToString();
            else itemQuantity.text = "";
            
        }
        if (itemImage != null) itemImage.sprite = item.Icon;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
