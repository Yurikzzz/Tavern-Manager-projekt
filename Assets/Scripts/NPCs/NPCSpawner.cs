using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    public GameObject npcPrefab;
    public Transform spawnPoint;      
    public int maxNpcs = 5;
    public float spawnInterval = 3f;  

    private Coroutine spawnRoutine;
    private List<GameObject> spawnedNpcs = new List<GameObject>();

    void Start()
    {
        GameTimeManager.Instance.OnTimeChanged += OnTimeChanged;

        OnTimeChanged(GameTimeManager.Instance.CurrentTime);
    }

    void OnDestroy()
    {
        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnTimeChanged -= OnTimeChanged;
    }

    void OnTimeChanged(GameTimeManager.TimeOfDay time)
    {
        if (time == GameTimeManager.TimeOfDay.Afternoon)
        {
            StartSpawning();
        }
        else
        {
            StopSpawning();

            if (time == GameTimeManager.TimeOfDay.Night)
            {
                ClearAllNpcs();
            }
        }
    }

    void StartSpawning()
    {
        if (spawnRoutine == null)
            spawnRoutine = StartCoroutine(SpawnLoop());
    }

    void StopSpawning()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            if (spawnedNpcs.Count < maxNpcs)
            {
                SpawnOneNpc();
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnOneNpc()
    {
        if (npcPrefab == null || spawnPoint == null)
        {
            Debug.LogWarning("NPCSpawner: Missing npcPrefab or spawnPoint.");
            return;
        }

        GameObject npc = Instantiate(npcPrefab, spawnPoint.position, Quaternion.identity);
        spawnedNpcs.Add(npc);
    }

    void ClearAllNpcs()
    {
        foreach (var npc in spawnedNpcs)
        {
            if (npc != null)
                Destroy(npc);
        }

        spawnedNpcs.Clear();
    }
}
