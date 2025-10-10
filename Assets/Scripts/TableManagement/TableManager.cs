using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TableManager : MonoBehaviour
{
    private List<Table> tables = new List<Table>();

    void Start()
    {
        tables.AddRange(FindObjectsByType<Table>(FindObjectsSortMode.None));
        Check();
    }

    public void Check() 
    {
        int tablecount = 0;
        foreach (Table table in tables) 
        {
            tablecount++;
        }
        Debug.Log("Tables detected: " + tablecount);
    }

    public Table GetFreeTable()
    {
        return tables.FirstOrDefault(t => t.HasFreeSeat);
    }
}
