using UnityEngine;
using UnityEngine.UI;

public class DishButtonUI : MonoBehaviour
{
    public Image icon;
    public Text label;                // or TextMeshProUGUI if you use TMP
    public Image selectedBorder;      // border when player selects this dish
    public Image requestedBorder;     // border when this is the requested dish (optional)

    private Dish dish;
    private BarUIController barUI;

    public Dish Dish => dish;

    public void Setup(Dish dish, BarUIController barUI)
    {
        this.dish = dish;
        this.barUI = barUI;

        if (icon != null && dish.icon != null)
            icon.sprite = dish.icon;

        if (label != null)
            label.text = dish.displayName;

        SetSelected(false);
        SetRequested(false);

        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }
    }

    void OnClick()
    {
        if (barUI != null)
        {
            barUI.SelectDish(this);
        }
    }

    public void SetSelected(bool value)
    {
        if (selectedBorder != null)
            selectedBorder.enabled = value;
    }

    public void SetRequested(bool value)
    {
        if (requestedBorder != null)
            requestedBorder.enabled = value;
    }
}
