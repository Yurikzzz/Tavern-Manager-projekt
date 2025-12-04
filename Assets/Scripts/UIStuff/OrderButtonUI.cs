using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OrderButtonUI : MonoBehaviour
{
    public Image dishIcon;
    public TextMeshProUGUI label;

    // Nové UI prvky pro trpìlivost zákazníka
    [Header("Patience UI (optional)")]
    public Image patienceFill;                 // nastavte Image.Type = Filled (Horizontal) v prefab
    public TextMeshProUGUI patienceLabel;      // zobrazí zbývající sekundy (volitelné)

    private Order order;
    private BarUIController barUI;
    private CustomerPatience customerPatience;

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

        // najdi CustomerPatience komponentu na NPC (pokud existuje)
        customerPatience = order.customer != null ? order.customer.GetComponent<CustomerPatience>() : null;

        // nastavit poèáteèní stav trpìlivosti
        UpdatePatienceUI();

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

    void Update()
    {
        // Pollujeme hodnotu trpìlivosti každým snímkem, aby UI bylo aktuální
        if (customerPatience != null && order != null && !order.isServed)
        {
            UpdatePatienceUI();
        }
        else
        {
            // skryjeme / nastavíme nulu pokud není zákazník nebo už je objednávka vyøízena
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