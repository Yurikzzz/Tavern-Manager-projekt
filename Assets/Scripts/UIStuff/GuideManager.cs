using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GuideManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject guideUIRoot;
    public TextMeshProUGUI contentText;

    [Header("Button Frames")]
    public GameObject[] sectionFrames;

    [Header("Settings")]
    public KeyCode toggleKey = KeyCode.U;
    private bool isOpen = false;

    [TextArea(5, 10)]
    public string movementText = "WASD to Move\nShift to Run\nE to Interact";
    [TextArea(5, 10)]
    public string ordersText = "1. Interact with customers to take orders.\n2. Go to the Bar to prepare food.\n3. Deliver the dish to the correct table.";
    [TextArea(5, 10)]
    public string roomsText = "Rent rooms at night.\nClean messes and fix beds to gain more popularity and coins!";
    [TextArea(5, 10)]
    public string upgradesText = "Use the Shop to buy decorations.\nBetter items increase your tavern's efficiency.";
    [TextArea(5, 10)]
    public string popularityText = "Happy customers increase popularity.\nHigh popularity brings in more customers each day!";
    [TextArea(5, 10)]
    public string dayCycleText = "Time is divided into three parts: morning, afternoon, and night. During the afternoon, the tavern is open to customers.\nYou can change time by opening/closing the tavern or sleeping.";
    [TextArea(5, 10)]
    public string customersText = "Customers that come to eat have a patience bar, if that bar reaches 0 they leave and lower your popularity.";

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
        foreach (GameObject frame in sectionFrames)
        {
            if (frame != null) frame.SetActive(false);
        }

        switch (sectionName)
        {
            case "Movement":
                contentText.text = movementText;
                if (sectionFrames.Length > 0) sectionFrames[0].SetActive(true);
                break;
            case "Orders":
                contentText.text = ordersText;
                if (sectionFrames.Length > 1) sectionFrames[1].SetActive(true);
                break;
            case "Rooms":
                contentText.text = roomsText;
                if (sectionFrames.Length > 2) sectionFrames[2].SetActive(true);
                break;
            case "Upgrades":
                contentText.text = upgradesText;
                if (sectionFrames.Length > 3) sectionFrames[3].SetActive(true);
                break;
            case "Popularity":
                contentText.text = popularityText;
                if (sectionFrames.Length > 4) sectionFrames[4].SetActive(true);
                break;
            case "Day cycle":
                contentText.text = dayCycleText;
                if (sectionFrames.Length > 5) sectionFrames[5].SetActive(true);
                break;
            case "Customers":
                contentText.text = customersText;
                if (sectionFrames.Length > 6) sectionFrames[6].SetActive(true);
                break;
        }
    }
}