using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    // Event raised when a new NPC is spawned
    public static event Action<GameObject> OnNpcSpawned;

    public GameObject npcPrefab;
    public Transform spawnPoint;
    public int maxNpcs = 5;
    public float spawnInterval = 3f;

    [Header("Spawn timing")]
    [Tooltip("Delay before the first NPC spawns when spawning starts (seconds)")]
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
                // Instead of destroying NPCs immediately, ask them to leave
                ClearAllNpcs();
            }
        }
    }

    // optional initialDelay (defaults to 0)
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

        // Notify listeners that an NPC was spawned
        OnNpcSpawned?.Invoke(npc);
    }

    void ClearAllNpcs()
    {
        // Request all existing NPCs to start leaving instead of destroying them immediately.
        // We keep them in spawnedNpcs so the normal destroy flow (when they reach exit) will remove them.
        for (int i = 0; i < spawnedNpcs.Count; i++)
        {
            var npc = spawnedNpcs[i];
            if (npc == null) continue;

            var controller = npc.GetComponent<NPCController>();
            if (controller != null)
            {
                controller.StartLeaving();
            }
            else
            {
                // fallback: destroy if it doesn't have a controller
                Destroy(npc);
            }
        }
        // don't clear spawnedNpcs here — entries will be removed over time when NPCs are destroyed
    }
}