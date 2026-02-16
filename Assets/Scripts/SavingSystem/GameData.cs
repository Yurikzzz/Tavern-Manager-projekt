using System.Collections.Generic;

[System.Serializable]
public class GameData
{
    public int coins = 0;
    public int popularity = 0;
    public int dayNumber = 1;

    public List<string> boughtUpgrades = new List<string>();
}