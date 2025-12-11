using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    [Header("References")]
    public PlayerCarry playerCarry;
    public GameObject carryingRoot;
    public Text carryingLabel;         
    public Image carryingIcon;     
    public Button dropButton;  

    [Header("Resources")]
    public TMP_Text coinsText;
    public TMP_Text popularityText;

    [Header("Input")]
    public KeyCode dropKey = KeyCode.Q;

    void Start()
    {
        if (playerCarry == null)
            playerCarry = FindObjectOfType<PlayerCarry>();

        if (playerCarry == null)
        {
            Debug.LogWarning("PlayerHUD: no PlayerCarry found in scene.");
            return;
        }

        UpdateUI(playerCarry.carriedDish);

        playerCarry.OnCarryChanged += UpdateUI;

        if (dropButton != null)
        {
            dropButton.onClick.RemoveAllListeners();
            dropButton.onClick.AddListener(OnDropButtonClicked);
        }

        if (PlayerProgress.Instance != null)
        {
            PlayerProgress.Instance.OnCoinsChanged += UpdateCoins;
            PlayerProgress.Instance.OnPopularityChanged += UpdatePopularity;
            // Initialize
            UpdateCoins(PlayerProgress.Instance.Coins);
            UpdatePopularity(PlayerProgress.Instance.Popularity);
        }
    }

    void OnDestroy()
    {
        if (playerCarry != null)
            playerCarry.OnCarryChanged -= UpdateUI;

        if (PlayerProgress.Instance != null)
        {
            PlayerProgress.Instance.OnCoinsChanged -= UpdateCoins;
            PlayerProgress.Instance.OnPopularityChanged -= UpdatePopularity;
        }

        if (dropButton != null)
            dropButton.onClick.RemoveAllListeners();
    }

    void Update()
    {
        if (Input.GetKeyDown(dropKey))
        {
            if (playerCarry != null && playerCarry.HasDish)
            {
                playerCarry.Drop();
            }
        }
    }

    void UpdateUI(Dish dish)
    {
        if (carryingRoot == null || carryingIcon == null)
            return;

        if (dish == null)
        {
            carryingRoot.SetActive(false);

            if (dropButton != null)
                dropButton.interactable = false;
        }
        else
        {
            carryingRoot.SetActive(true);

            if (carryingLabel != null)
                carryingLabel.text = "Carrying:"; 

            carryingIcon.sprite = dish.icon;
            carryingIcon.enabled = (dish.icon != null);

            if (dropButton != null)
                dropButton.interactable = true;
        }
    }

    void OnDropButtonClicked()
    {
        if (playerCarry != null && playerCarry.HasDish)
        {
            playerCarry.Drop();
        }
    }

    void UpdateCoins(int amount)
    {
        if (coinsText != null)
            coinsText.text = "Coins: " + amount.ToString();
    }

    void UpdatePopularity(int amount)
    {
        if (popularityText != null)
            popularityText.text = "Popularity: " + amount.ToString();
    }
}
