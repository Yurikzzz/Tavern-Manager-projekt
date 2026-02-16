using UnityEngine;
using System.IO;

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;

    public GameData currentData;

    private string savePath;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        savePath = Application.persistentDataPath + "/savefile.json";
        LoadGame(); 
    }

    public void SaveGame()
    {
        if (PlayerProgress.Instance != null)
        {
            currentData.coins = PlayerProgress.Instance.Coins;
            currentData.popularity = PlayerProgress.Instance.Popularity;
        }

        if (UpgradeManager.Instance != null)
        {
            currentData.boughtUpgrades = UpgradeManager.Instance.GetPurchasedUpgradesList();
        }

        if (GameTimeManager.Instance != null)
        {
            currentData.dayNumber = GameTimeManager.Instance.CurrentDay;
        }

        string json = JsonUtility.ToJson(currentData, true);
        System.IO.File.WriteAllText(savePath, json);

        Debug.Log($"Game Saved! Day: {currentData.dayNumber}");
    }

    public void LoadGame()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            currentData = JsonUtility.FromJson<GameData>(json);
            Debug.Log("Save file loaded!");
        }
        else
        {
            currentData = new GameData();
            Debug.Log("No save file found. Starting fresh.");
        }
    }
}