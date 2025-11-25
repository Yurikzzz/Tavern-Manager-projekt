using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    public static event Action<GameObject> OnNpcSpawned;

    public GameObject npcPrefab;
    public Transform spawnPoint;
    public int maxNpcs = 5;
    public float spawnInterval = 3f;

    [Header("Spawn timing")]
    public float initialSpawnDelay = 5f;

    private Coroutine spawnRoutine;
    private List<GameObject> spawnedNpcs = new List<GameObject>();

    void Start()
    {
        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnTimeChanged += OnTimeChanged;

        if (GameTimeManager.Instance != null)
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
            StartSpawning(initialSpawnDelay);
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

    void StartSpawning(float initialDelay = 0f)
    {
        if (spawnRoutine == null)
            spawnRoutine = StartCoroutine(SpawnLoop(initialDelay));
    }

    void StopSpawning()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    IEnumerator SpawnLoop(float initialDelay)
    {
        if (initialDelay > 0f)
            yield return new WaitForSeconds(initialDelay);

        while (true)
        {
            spawnedNpcs.RemoveAll(npc => npc == null);

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

        NPCController controller = npc.GetComponent<NPCController>();
        if (controller != null)
        {
            controller.exitPoint = spawnPoint;
        }

        OnNpcSpawned?.Invoke(npc);
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