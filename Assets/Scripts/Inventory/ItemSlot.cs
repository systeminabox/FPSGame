using Inventory;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    [SerializeField] Image itemImage;
    [SerializeField] TextMeshProUGUI itemName;
    [SerializeField] TextMeshProUGUI itemQuantity;
    public ItemDefinition Item { get; private set; }
    [SerializeField] private int quantity;
    
    private Button slotButton;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        slotButton = GetComponent<Button>();
        slotButton.onClick.AddListener(HandleClick);
        if (Item == null)
        {
            HideItem();
        }
    }

    void HideItem()
    {
        itemImage.sprite = null;
        itemName.text = "";
        itemQuantity.text = "";
    }

    public void UpdateItem(ItemDefinition item, int quantityIn)
    {
        if (item == null)
        {
            HideItem();
            return;
        }
        
        this.quantity = quantityIn;
        if (itemName != null) itemName.text = item.DisplayName;
        if (itemQuantity != null)
        {
            if (quantity > 1) itemQuantity.text = quantity.ToString();
            else itemQuantity.text = "";
            
        }
        if (itemImage != null) itemImage.sprite = item.Icon;
        
    }

    void HandleClick()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
