using UnityEngine;
using System;
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

    public static event Action<GameObject> OnGuestArrived;

    void Start()
    {
        if (GameTimeManager.Instance != null)
        {
            GameTimeManager.Instance.OnTimeChanged += OnTimeChanged;

            OnTimeChanged(GameTimeManager.Instance.CurrentTime);
        }
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
            float waitTime = UnityEngine.Random.Range(minSpawnTime, maxSpawnTime);

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

        GameObject guest = Instantiate(rentalGuestPrefab, spawnPoint.position, Quaternion.identity);
        currentGuest = guest;

        RentalGuestController controller = guest.GetComponent<RentalGuestController>();
        if (controller != null)
        {
            controller.barPosition = barCheckInPoint;
            controller.exitPosition = spawnPoint;
        }

        OnGuestArrived?.Invoke(guest);
    }
}