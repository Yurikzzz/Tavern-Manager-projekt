using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GuideManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject guideUIRoot;
    public TextMeshProUGUI contentText;

    [Header("Settings")]
    public KeyCode toggleKey = KeyCode.U;

    private bool isOpen = false;

    [TextArea(5, 10)]
    public string movementText = "WASD to Move\nShift to Run\nE to Interact";
    [TextArea(5, 10)]
    public string ordersText = "1. Open bar to view all orders.\n2. Select an order and choose a dish.\n3. Deliver the dish to the correct customer. Get less rewards if you deliver a dish that wasn't ordered.";
    [TextArea(5, 10)]
    public string roomsText = "Rent rooms at night.\nCustomers that want a room come during open hours, give them the room keys at the bar whenever they come in.\nIf a guest stays in a room overnight, the room will be messy in the morning. Clean it up to get full rewards from the next customer.";
    [TextArea(5, 10)]
    public string upgradesText = "Use the Shop to buy decorations and upgrades.\nUpgrades increase your tavern's efficiency by increasing reward income.";
    [TextArea(5, 10)]
    public string popularityText = "Happy customers increase popularity.\nHigh popularity brings in more customers each day.\nYou need to get a certain amount of popularity in the given time limit to not lose ownership of the tavern.";

    void Start()
    {
        if (guideUIRoot != null) guideUIRoot.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleGuide();
        }
    }

    public void ToggleGuide()
    {
        isOpen = !isOpen;
        guideUIRoot.SetActive(isOpen);

        Time.timeScale = isOpen ? 0f : 1f;

        if (isOpen) ShowSection("Movement");
    }

    public void ShowSection(string sectionName)
    {
        switch (sectionName)
        {
            case "Movement": contentText.text = movementText; break;
            case "Orders": contentText.text = ordersText; break;
            case "Rooms": contentText.text = roomsText; break;
            case "Upgrades": contentText.text = upgradesText; break;
            case "Popularity": contentText.text = popularityText; break;
        }
    }
}