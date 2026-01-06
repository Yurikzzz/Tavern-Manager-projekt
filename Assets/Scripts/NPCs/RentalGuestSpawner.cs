using UnityEngine;
using System.Collections;

public class RentalGuestSpawner : MonoBehaviour
{
    [Header("Settings")]
    public GameObject rentalGuestPrefab;
    public Transform spawnPoint;
    public Transform barCheckInPoint;

    [Header("Timing")]
    public float minSpawnTime = 30f;
    public float maxSpawnTime = 40f;

    private Coroutine spawnRoutine;
    private GameObject currentGuest;

    void Start()
    {
        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnTimeChanged += OnTimeChanged;
    }

    void OnDestroy()
    {
        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnTimeChanged -= OnTimeChanged;
    }

    void OnTimeChanged(GameTimeManager.TimeOfDay time)
    {
        if (time == GameTimeManager.TimeOfDay.Afternoon || time == GameTimeManager.TimeOfDay.Night)
        {
            StartSpawning();
        }
        else
        {
            StopSpawning();
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
        yield return new WaitForSeconds(5f);

        while (true)
        {
            float waitTime = Random.Range(minSpawnTime, maxSpawnTime);
            yield return new WaitForSeconds(waitTime);

            while (currentGuest != null)
            {
                yield return new WaitForSeconds(1f);
            }

            SpawnGuest();
        }
    }

    void SpawnGuest()
    {
        if (rentalGuestPrefab == null) return;

        currentGuest = Instantiate(rentalGuestPrefab, spawnPoint.position, Quaternion.identity);

        RentalGuestController controller = currentGuest.GetComponent<RentalGuestController>();
        if (controller != null)
        {
            controller.barPosition = barCheckInPoint;
            controller.exitPosition = spawnPoint;
        }
    }
}