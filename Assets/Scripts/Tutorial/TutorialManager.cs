using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("UI")]
    public GameObject popupRoot;
    public TextMeshProUGUI popupText;

    [Header("Placement Anchors")]
    [Tooltip("Optional: place UI empties (RectTransform) under your Canvas and assign them here.")]
    public Transform defaultAnchor;
    [Tooltip("Popup position while the tavern is open (afternoon).")]
    public Transform tavernOpenAnchor;
    [Tooltip("Popup position while the bar menu is open.")]
    public Transform barOpenAnchor;

    // Arrow guidance configuration - now spawns world objects at anchors and falls back to off-screen placement
    [System.Serializable]
    public class ArrowConfig
    {
        public Step step;                      // Which tutorial step this config applies to
        public Transform anchor;               // World / UI empty assigned in inspector
        [Tooltip("Prefab spawned directly at the anchor. Should be a small world object with SpriteRenderer or MeshRenderer.")]
        public GameObject worldArrowPrefab;    // Prefab instantiated at anchor when on-screen
        [Tooltip("Prefab spawned near the camera edge when anchor is off-screen (optional). If null, worldArrowPrefab will be moved to edge position.")]
        public GameObject offscreenArrowPrefab; // Prefab instantiated at screen edge in world space
        public Vector3 worldOffset = Vector3.up * 0.5f; // offset from anchor when placing world arrow
        [Tooltip("Distance from camera for off-screen arrow placement (world units).")]
        public float offscreenDistance = 5f;
        [Tooltip("Viewport margin when calculating off-screen edge (0..0.5)")]
        public float offscreenViewportMargin = 0.05f;
    }

    [Header("Arrow Guidance")]
    [Tooltip("Assign arrow configs for tutorial steps. The system will spawn small world objects at anchors or near camera edge if off-screen.")]
    public List<ArrowConfig> arrowConfigs = new List<ArrowConfig>();

    [Tooltip("Scale applied to the world arrow instances (optional).")]
    public Vector3 arrowDefaultScale = new Vector3(1f, 1f, 1f);

    private GameTimeManager timeManager;
    private OrderManager orderManager;
    private BarUIController barUI;

    private RectTransform popupRect;
    private Vector3 recordedDefaultLocalPos;
    private Vector3 recordedDefaultWorldPos;
    private bool hasRecordedDefault = false;

    public enum Step
    {
        Idle,
        WaitingStart,
        CleanRooms,
        OpenTavern,
        OpenBar,
        ChooseOrder,
        ChooseDish,
        FirstServed,
        ServeUntilClose,
        GoToBed,
        Finished
    }

    private Step current = Step.Idle;

    private int initialRoomCount = 0;
    private int previousActiveOrders = 0;
    private bool deliveryMarkerShown = false;

    private bool barEventsSubscribed = false;

    // runtime spawned arrow instances (per step)
    private readonly Dictionary<Step, GameObject> spawnedWorldArrows = new Dictionary<Step, GameObject>();
    private readonly Dictionary<Step, GameObject> spawnedOffscreenArrows = new Dictionary<Step, GameObject>();

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (popupRoot != null) popupRoot.SetActive(false);

        timeManager = GameTimeManager.Instance;
        orderManager = OrderManager.Instance;

        if (orderManager != null)
            orderManager.OnOrdersChanged += OnOrdersChanged;

        if (popupRoot != null)
        {
            popupRect = popupRoot.GetComponent<RectTransform>();
            if (popupRect != null)
            {
                recordedDefaultLocalPos = popupRect.localPosition;
                hasRecordedDefault = true;
            }
            else
            {
                recordedDefaultWorldPos = popupRoot.transform.position;
                hasRecordedDefault = true;
            }
        }

        StartCoroutine(InitializeNextFrame());
    }

    void OnDestroy()
    {
        if (orderManager != null)
            orderManager.OnOrdersChanged -= OnOrdersChanged;

        if (timeManager != null)
        {
            timeManager.OnTimeChanged -= OnTimeChanged;
            timeManager.OnDayChanged -= OnDayChanged;
        }

        if (barEventsSubscribed && barUI != null)
        {
            barUI.OnBarOpened -= HandleBarOpened;
            barUI.OnBarClosed -= HandleBarClosed;
            barUI.OnOrderSelected -= HandleOrderSelected;
            barUI.OnDishSelected -= HandleDishSelected;
            barUI.OnDishConfirmed -= HandleDishConfirmed;
            barEventsSubscribed = false;
        }

        if (Instance == this) Instance = null;

        // Clean up spawned arrows
        foreach (var kv in spawnedWorldArrows) if (kv.Value != null) Destroy(kv.Value);
        spawnedWorldArrows.Clear();
        foreach (var kv in spawnedOffscreenArrows) if (kv.Value != null) Destroy(kv.Value);
        spawnedOffscreenArrows.Clear();
    }

    private IEnumerator InitializeNextFrame()
    {
        yield return null;

        while (GameTimeManager.Instance == null)
            yield return null;

        timeManager = GameTimeManager.Instance;
        timeManager.OnTimeChanged += OnTimeChanged;
        timeManager.OnDayChanged += OnDayChanged;

        barUI = FindObjectOfType<BarUIController>();
        TrySubscribeBarEvents();

        current = Step.WaitingStart;
    }

    private void TrySubscribeBarEvents()
    {
        if (barUI != null && !barEventsSubscribed)
        {
            barUI.OnBarOpened += HandleBarOpened;
            barUI.OnBarClosed += HandleBarClosed;
            barUI.OnOrderSelected += HandleOrderSelected;
            barUI.OnDishSelected += HandleDishSelected;
            barUI.OnDishConfirmed += HandleDishConfirmed;
            barEventsSubscribed = true;
        }
    }

    private void HandleBarOpened()
    {
        if (current == Step.OpenBar || current == Step.OpenTavern)
        {
            SetStep(Step.ChooseOrder, "Bar menu open. First select an order from the orders list.");
        }
    }

    private void HandleBarClosed()
    {
        if (current == Step.ChooseOrder || current == Step.ChooseDish)
        {
            SetStep(Step.OpenBar, "Bar closed. Open it again to continue.");
        }
    }

    private void HandleOrderSelected(Order order)
    {
        if (current == Step.ChooseOrder)
        {
            SetStep(Step.ChooseDish, "Order selected. Now pick the correct dish from the menu.");
        }
    }

    private void HandleDishSelected(Dish dish)
    {
        if (current == Step.ChooseDish)
        {
            ShowPopup("Dish selected. Click Confirm to prepare the dish.");
        }
    }

    private void HandleDishConfirmed(Dish dish)
    {
        previousActiveOrders = OrderManager.Instance != null ? OrderManager.Instance.ActiveOrders.Count : 0;
        SetStep(Step.FirstServed, "Dish prepared. Deliver it to the customer. Serve the first customer to continue.");
    }

    private void OnTimeChanged(GameTimeManager.TimeOfDay t)
    {
        if (current == Step.OpenTavern && t == GameTimeManager.TimeOfDay.Afternoon)
        {
            SetStep(Step.OpenBar, "Tavern is open! Go to the bar and open the menu (interact with the bar).");
        }

        if ((current == Step.FirstServed || current == Step.ServeUntilClose || current == Step.ChooseDish) &&
            t == GameTimeManager.TimeOfDay.Night)
        {
            SetStep(Step.GoToBed, "The tavern is closed. Go to bed to advance to the next day.");
        }
    }

    private void OnDayChanged(int newDay)
    {
        if (current != Step.Finished)
        {
            SetStep(Step.Finished, "");
            HidePopup();
            if (timeManager != null)
            {
                timeManager.OnTimeChanged -= OnTimeChanged;
                timeManager.OnDayChanged -= OnDayChanged;
            }

            if (barEventsSubscribed && barUI != null)
            {
                barUI.OnBarOpened -= HandleBarOpened;
                barUI.OnBarClosed -= HandleBarClosed;
                barUI.OnOrderSelected -= HandleOrderSelected;
                barUI.OnDishSelected -= HandleDishSelected;
                barUI.OnDishConfirmed -= HandleDishConfirmed;
                barEventsSubscribed = false;
            }

            enabled = false;
        }
    }

    private void OnOrdersChanged()
    {
        if (orderManager == null) return;

        if (current == Step.FirstServed)
            return;

        previousActiveOrders = orderManager.ActiveOrders?.Count ?? 0;
    }

    void Update()
    {
        if (!barEventsSubscribed)
        {
            if (barUI == null) barUI = FindObjectOfType<BarUIController>();
            TrySubscribeBarEvents();
        }

        if (current == Step.WaitingStart)
        {
            if (timeManager != null && timeManager.CurrentDay == 1 && timeManager.CurrentTime == GameTimeManager.TimeOfDay.Morning)
            {
                var rooms = FindObjectsOfType<RoomManager>();
                initialRoomCount = rooms != null ? rooms.Length : 0;

                SetStep(Step.CleanRooms, $"Welcome! First, please clean all rental rooms ({initialRoomCount} rooms). Clean every mess in the rooms to continue.");
            }
            UpdatePopupLocation();
            UpdateArrow();
            return;
        }

        if (current == Step.CleanRooms)
        {
            var rooms = FindObjectsOfType<RoomManager>();
            if (rooms != null && rooms.Length > 0)
            {
                bool allClean = true;
                foreach (var r in rooms)
                {
                    if (!r.isClean)
                    {
                        allClean = false;
                        break;
                    }
                }

                if (allClean)
                {
                    SetStep(Step.OpenTavern, "Great! Now open the tavern by interacting with the tavern door.");
                }
            }
        }

        if (current == Step.OpenBar)
        {
            if (barUI == null) barUI = FindObjectOfType<BarUIController>();
            if (barUI != null && barUI.barUIRoot != null && barUI.barUIRoot.activeInHierarchy)
            {
                SetStep(Step.ChooseOrder, "Bar menu open. First select an order from the orders list.");
            }
        }

        if (current == Step.FirstServed)
        {
            if (OrderManager.Instance != null)
            {
                int currentCount = OrderManager.Instance.ActiveOrders.Count;
                if (currentCount < previousActiveOrders)
                {
                    SetStep(Step.ServeUntilClose, "Nice! Serve customers until the tavern closes, or close it manually.");
                }
            }
        }

        UpdatePopupLocation();
        UpdateArrow();
    }

    private void SetStep(Step step, string message)
    {
        current = step;
        if (string.IsNullOrEmpty(message))
            HidePopup();
        else
            ShowPopup(message);
        // when step changes we refresh arrows immediately
        UpdateArrow();
    }

    private void ShowPopup(string message)
    {
        if (popupRoot != null)
            popupRoot.SetActive(true);

        if (popupText != null)
            popupText.text = message;

        UpdatePopupLocation();
    }

    private void HidePopup()
    {
        if (popupRoot != null)
            popupRoot.SetActive(false);
    }

    private void UpdatePopupLocation()
    {
        if (popupRoot == null) return;
        if (!popupRoot.activeInHierarchy) return;

        if (barUI == null) barUI = FindObjectOfType<BarUIController>();

        bool barOpen = (barUI != null && barUI.barUIRoot != null && barUI.barUIRoot.activeInHierarchy);
        bool tavernOpen = (timeManager != null && timeManager.CurrentTime == GameTimeManager.TimeOfDay.Afternoon);

        if (barOpen && tavernOpenAnchor != null)
        {
            MovePopupTo(barOpenAnchor);
        }
        else if (tavernOpen && tavernOpenAnchor != null)
        {
            MovePopupTo(tavernOpenAnchor);
        }
        else if (defaultAnchor != null)
        {
            MovePopupTo(defaultAnchor);
        }
        else
        {
            MovePopupTo(null);
        }
    }

    // --- ARROW: spawn world objects at anchor; when anchor off-screen place arrow near camera edge in world space ---
    private void UpdateArrow()
    {
        // find config for current step
        ArrowConfig cfg = arrowConfigs.Find(a => a.step == current);

        // hide any arrows for other steps
        foreach (var kv in new List<Step>(spawnedWorldArrows.Keys))
            if (kv != current && spawnedWorldArrows[kv] != null) { Destroy(spawnedWorldArrows[kv]); spawnedWorldArrows.Remove(kv); }
        foreach (var kv in new List<Step>(spawnedOffscreenArrows.Keys))
            if (kv != current && spawnedOffscreenArrows[kv] != null) { Destroy(spawnedOffscreenArrows[kv]); spawnedOffscreenArrows.Remove(kv); }

        if (cfg == null || cfg.anchor == null || cfg.worldArrowPrefab == null)
        {
            // nothing to show
            return;
        }

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 anchorPos = cfg.anchor.position;
        Vector3 viewport = cam.WorldToViewportPoint(anchorPos);
        bool inFront = viewport.z > 0f;
        bool onScreen = inFront && viewport.x >= 0f && viewport.x <= 1f && viewport.y >= 0f && viewport.y <= 1f;

        if (onScreen)
        {
            // ensure offscreen arrow removed
            if (spawnedOffscreenArrows.TryGetValue(cfg.step, out var off))
            {
                if (off != null) Destroy(off);
                spawnedOffscreenArrows.Remove(cfg.step);
            }

            // spawn or move world arrow to anchor + offset
            if (!spawnedWorldArrows.TryGetValue(cfg.step, out var go) || go == null)
            {
                var inst = Instantiate(cfg.worldArrowPrefab, cfg.anchor.position + cfg.worldOffset, Quaternion.identity);
                inst.transform.localScale = arrowDefaultScale;
                spawnedWorldArrows[cfg.step] = inst;
            }
            else
            {
                go.transform.position = cfg.anchor.position + cfg.worldOffset;
                go.transform.localScale = arrowDefaultScale;
            }

            // billboard to camera if possible
            if (spawnedWorldArrows[cfg.step] != null)
            {
                spawnedWorldArrows[cfg.step].transform.LookAt(cam.transform);
                // keep arrow upright: zero X/Z rotation if sprite needs it
                Vector3 e = spawnedWorldArrows[cfg.step].transform.eulerAngles;
                spawnedWorldArrows[cfg.step].transform.eulerAngles = new Vector3(0f, e.y, 0f);
            }
        }
        else
        {
            // anchor off-screen -> remove world arrow if created (we will show edge arrow)
            if (spawnedWorldArrows.TryGetValue(cfg.step, out var existingWorld) && existingWorld != null)
            {
                Destroy(existingWorld);
                spawnedWorldArrows.Remove(cfg.step);
            }

            // compute clamped viewport position (Y clamp used to set vertical)
            float margin = Mathf.Clamp01(cfg.offscreenViewportMargin);
            float clampedY = Mathf.Clamp(viewport.y, margin, 1f - margin);
            bool placeLeft = (viewport.x < 0.5f);

            // compute screen coordinates at left/right edge
            float screenX = (placeLeft ? (Screen.width * margin) : (Screen.width * (1f - margin)));
            float screenY = clampedY * Screen.height;

            // compute world position at specified distance from camera
            float distance = Mathf.Max(2f, Vector3.Distance(cam.transform.position, cfg.anchor.position));
            distance = Mathf.Max(distance, cfg.offscreenDistance);
            Vector3 screenPoint = new Vector3(screenX, screenY, distance);
            Vector3 worldPos = cam.ScreenToWorldPoint(screenPoint);

            // spawn or move offscreen arrow prefab (or spawn worldArrowPrefab if offscreen prefab not assigned)
            GameObject prefabToUse = cfg.offscreenArrowPrefab != null ? cfg.offscreenArrowPrefab : cfg.worldArrowPrefab;
            if (!spawnedOffscreenArrows.TryGetValue(cfg.step, out var offGo) || offGo == null)
            {
                var inst = Instantiate(prefabToUse, worldPos, Quaternion.identity);
                inst.transform.localScale = arrowDefaultScale;
                spawnedOffscreenArrows[cfg.step] = inst;
            }
            else
            {
                offGo.transform.position = worldPos;
                offGo.transform.localScale = arrowDefaultScale;
            }

            // orient offscreen arrow: face camera and rotate sprite to point inward based on camera vs anchor X position
            var arrowObj = spawnedOffscreenArrows[cfg.step];
            if (arrowObj != null)
            {
                // face the camera
                // use camera forward inverted so sprite faces camera normal
                Quaternion faceCam = Quaternion.LookRotation(-cam.transform.forward, Vector3.up);

                // decide horizontal pointing direction using camera (player) X vs anchor X
                bool playerLeftOfAnchor = cam.transform.position.x < cfg.anchor.position.x;

                // Default sprite is DOWN. To point RIGHT -> rotate Z by -90; to point LEFT -> +90
                float zAngle = playerLeftOfAnchor ? -90f : 90f;

                // combine facing rotation with Z rotation
                arrowObj.transform.rotation = faceCam * Quaternion.Euler(0f, 0f, zAngle);
            }
        }
    }

    private void MovePopupTo(Transform anchor)
    {
        if (popupRect == null)
        {
            if (popupRoot == null) return;

            if (anchor != null)
            {
                popupRoot.transform.position = anchor.position;
            }
            else if (hasRecordedDefault)
            {
                popupRoot.transform.position = recordedDefaultWorldPos;
            }

            return;
        }

        if (anchor == null)
        {
            if (defaultAnchor != null)
            {
                RectTransform defRect = defaultAnchor.GetComponent<RectTransform>();
                if (defRect != null)
                {
                    popupRect.anchoredPosition = defRect.anchoredPosition;
                    return;
                }
                else
                {
                    SetRectPositionFromWorldPoint(defaultAnchor.position);
                    return;
                }
            }

            if (hasRecordedDefault)
            {
                popupRect.localPosition = recordedDefaultLocalPos;
            }
            return;
        }

        RectTransform anchorRect = anchor.GetComponent<RectTransform>();
        if (anchorRect != null)
        {
            popupRect.anchoredPosition = anchorRect.anchoredPosition;
            return;
        }

        SetRectPositionFromWorldPoint(anchor.position);
    }

    private void SetRectPositionFromWorldPoint(Vector3 worldPos)
    {
        Canvas canvas = popupRect.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Vector3 screenPos = (Camera.main != null) ? Camera.main.WorldToScreenPoint(worldPos) : new Vector3(worldPos.x, worldPos.y, 0f);
            popupRect.position = screenPos;
            return;
        }

        Camera cam = (canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : (canvas.worldCamera ?? Camera.main);
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, worldPos);
        RectTransform canvasRect = canvas.transform as RectTransform;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, cam, out Vector2 localPoint))
        {
            popupRect.localPosition = new Vector3(localPoint.x, localPoint.y, popupRect.localPosition.z);
        }
    }

    // Public helpers for other systems (TavernDoor) to ask about tutorial blocking rules
    public bool IsBlockingRoomOpenRequirement
    {
        get
        {
            var gm = timeManager ?? GameTimeManager.Instance;
            if (gm == null) return false;
            return gm.CurrentDay == 1 && (current == Step.WaitingStart || current == Step.CleanRooms);
        }
    }

    public bool IsBlockingManualClose
    {
        get
        {
            var gm = timeManager ?? GameTimeManager.Instance;
            if (gm == null) return false;
            // Block manual close while player hasn't served the first customer during the tutorial
            return gm.CurrentDay == 1 &&
                   (current == Step.OpenTavern ||
                    current == Step.OpenBar ||
                    current == Step.ChooseOrder ||
                    current == Step.ChooseDish ||
                    current == Step.FirstServed);
        }
    }
}