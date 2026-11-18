using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class ItemCardUI : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text field;
    [SerializeField] private TMP_Text itemName;
    [SerializeField] private TMP_Text stackSize;
    [SerializeField] private Color occupiedColor;
    [SerializeField] private Color emptyColor;

    internal void Display(InventoryEntry entry, int? draggedQuantity = null)
    {
        var occupied = entry != null;
        var showsFieldText = occupied && entry.ShowsFieldText;
        background.color = occupied ? occupiedColor : emptyColor;
        title.gameObject.SetActive(showsFieldText);
        field.gameObject.SetActive(showsFieldText);
        itemName.gameObject.SetActive(occupied && !showsFieldText);
        stackSize.gameObject.SetActive(occupied);

        if (!occupied)
            return;

        stackSize.text = (draggedQuantity ?? entry.Quantity).ToString();
        if (showsFieldText)
        {
            title.text = entry.Item.fieldLabel;
            field.text = entry.Item.fieldText;
        }
        else
        {
            itemName.text = entry.Item.name;
        }
    }
}
