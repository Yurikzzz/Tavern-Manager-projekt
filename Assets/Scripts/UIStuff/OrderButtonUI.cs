using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OrderButtonUI : MonoBehaviour
{
    public Image dishIcon;
    public TextMeshProUGUI label;

    private Order order;
    private BarUIController barUI;

    public void Setup(Order order, BarUIController barUI)
    {
        this.order = order;
        this.barUI = barUI;

        if (dishIcon != null && order.dish != null && order.dish.icon != null)
        {
            dishIcon.sprite = order.dish.icon;
        }

        if (label != null && order.dish != null)
        {
            label.text = order.dish.displayName;
        }

        // Add click listener
        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }
    }

    private void OnClick()
    {
        if (barUI != null && order != null)
        {
            barUI.SelectOrder(order);
        }
    }
}
