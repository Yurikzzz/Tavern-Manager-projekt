using UnityEngine;

public class CustomerPatience : MonoBehaviour
{
    [Header("Patience Settings")]
    public float maxPatience = 10f;
    public float currentPatience;
    public float drainMultiplier = 1f;
    public bool waiting = false;
    private bool served = false;

    [Header("Bar Elements")]
    public Transform barRoot;
    public Transform barFill;                  
    private Vector3 barFillBaseScale;

    private NPCController npcController;

    private void Awake()
    {
        currentPatience = maxPatience;

        if (barFill != null)
            barFillBaseScale = barFill.localScale; 

        npcController = GetComponent<NPCController>();
        if (npcController == null)
        {
            npcController = GetComponentInParent<NPCController>();
        }
    }

    public void SitAndStartWaiting()
    {
        waiting = true;
        served = false;
        currentPatience = maxPatience;

        if (barRoot != null)
            barRoot.gameObject.SetActive(true);

        UpdateBar();
    }

    private void Update()
    {
        if (!waiting || served) return;

        currentPatience -= Time.deltaTime * drainMultiplier;
        if (currentPatience < 0f) currentPatience = 0f;

        UpdateBar();

        if (currentPatience <= 0f)
        {
            waiting = false;
            OnPatienceExpired();
        }
    }

    private void UpdateBar()
    {
        if (barFill == null) return;

        float t = currentPatience / maxPatience;

        // scale horizontally
        Vector3 s = barFillBaseScale;
        s.x *= t;
        barFill.localScale = s;

        // optional: color change
        var sr = barFill.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = Color.Lerp(Color.red, Color.green, t);
        }
    }

    public void OnServed()
    {
        served = true;
        waiting = false;

        if (barRoot != null)
            barRoot.gameObject.SetActive(false);
    }

    private void OnPatienceExpired()
    {
        
        if (barRoot != null)
            barRoot.gameObject.SetActive(false);

        if (npcController != null)
        {
            npcController.StartLeaving();
        }
        else
        {
            Debug.LogWarning($"{name}: NPCController not found — destroying because patience expired.");
            Destroy(gameObject, 0.1f);
        }
    }
}
