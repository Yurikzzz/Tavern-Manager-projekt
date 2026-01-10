using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OrderButtonUI : MonoBehaviour
{
    public Image dishIcon;
    public TextMeshProUGUI label;

    [Header("Visuals")]
    public GameObject selectedBorderContainer; 

    [Header("Patience UI")]
    public Image patienceFill;
    public TextMeshProUGUI patienceLabel;

    private Order order;
    private BarUIController barUI;
    private CustomerPatience customerPatience;

    public Order Order => order;

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

        customerPatience = order.customer != null ? order.customer.GetComponent<CustomerPatience>() : null;

        SetSelected(false);

        UpdatePatienceUI();

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
            barUI.SelectOrder(this);
        }
    }

    public void SetSelected(bool value)
    {
        if (selectedBorderContainer != null)
        {
            selectedBorderContainer.SetActive(value);
        }
    }

    void Update()
    {
        if (customerPatience != null && order != null && !order.isServed)
        {
            UpdatePatienceUI();
        }
        else
        {
            if (patienceFill != null) patienceFill.fillAmount = 0f;
            if (patienceLabel != null) patienceLabel.text = string.Empty;
        }
    }

    private void UpdatePatienceUI()
    {
        if (customerPatience == null) return;

        float t = 0f;
        if (customerPatience.maxPatience > 0f)
            t = Mathf.Clamp01(customerPatience.currentPatience / customerPatience.maxPatience);

        if (patienceFill != null)
            patienceFill.fillAmount = t;

        if (patienceLabel != null)
            patienceLabel.text = Mathf.CeilToInt(customerPatience.currentPatience).ToString();
    }
}