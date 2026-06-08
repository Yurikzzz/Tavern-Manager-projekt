using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    public enum FloatDirection { Vertical, Horizontal }

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

    [System.Serializable]
    public class ArrowConfig
    {
        public Step step;
        public GameObject arrowObject;

        [Header("Off-screen Settings")]
        [Tooltip("Zapnout/vypnout zobrazení šipky na okraji obrazovky, když se kamera nedívá.")]
        public bool showOffScreen = true;

        [Header("Animation Override")]
        [Tooltip("Určuje osu, po které se šipka vznáší.")]
        public FloatDirection floatDir = FloatDirection.Vertical;

        [HideInInspector] public Vector3 originalPos;
        [HideInInspector] public Quaternion originalRot;
    }

    [Header("Arrow Guidance")]
    public List<ArrowConfig> arrowConfigs = new List<ArrowConfig>();

    [Header("Global Animation Settings")]
    [Tooltip("Rychlost vznášení šipky na obrazovce.")]
    public float floatSpeed = 3f;
    [Tooltip("Jak moc se má šipka vznášet.")]
    public float floatAmplitude = 0.15f;

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
    private bool barEventsSubscribed = false;

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

        foreach (var config in arrowConfigs)
        {
            if (config.arrowObject != null)
            {
                config.originalPos = config.arrowObject.transform.position;
                config.originalRot = config.arrowObject.transform.rotation;
                config.arrowObject.SetActive(false);
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
    }

    private IEnumerator InitializeNextFrame()
    {
        yield return null;

        while (GameTimeManager.Instance == null)
            yield return null;

        timeManager = GameTimeManager.Instance;
        timeManager.OnTimeChanged += OnTimeChanged;
        timeManager.OnDayChanged += OnDayChanged;

        if (SaveManager.instance != null && SaveManager.instance.currentData != null && SaveManager.instance.currentData.tutorialCompleted)
        {
            current = Step.Finished;
            HidePopup();

            timeManager.OnTimeChanged -= OnTimeChanged;
            timeManager.OnDayChanged -= OnDayChanged;
            enabled = false;
            yield break;
        }

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
        if (current != Step.ChooseDish)
            return;

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

            if (SaveManager.instance != null && SaveManager.instance.currentData != null)
            {
                SaveManager.instance.currentData.tutorialCompleted = true;
                SaveManager.instance.SaveGame();
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

    private void UpdateArrow()
    {
        foreach (var config in arrowConfigs)
        {
            if (config.arrowObject != null)
                config.arrowObject.SetActive(false);
        }

        ArrowConfig cfg = arrowConfigs.Find(a => a.step == current);
        if (cfg == null || cfg.arrowObject == null)
            return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 viewportPos = cam.WorldToViewportPoint(cfg.originalPos);

        bool isOffScreenX = viewportPos.x < 0f || viewportPos.x > 1f;
        bool isOffScreenY = viewportPos.y < 0f || viewportPos.y > 1f;

        if ((isOffScreenX || isOffScreenY) && cfg.showOffScreen)
        {
            cfg.arrowObject.SetActive(true);

            float targetX = Mathf.Clamp(viewportPos.x, 0.05f, 0.95f);
            float targetY = Mathf.Clamp(viewportPos.y, 0.05f, 0.95f);
            float zRotation = 0f;

            if (isOffScreenX)
            {
                targetY = 0.5f;
                if (viewportPos.x < 0f)
                {
                    targetX = 0.05f;
                    zRotation = -90f;
                }
                else
                {
                    targetX = 0.95f;
                    zRotation = 90f;
                }
            }
            else if (isOffScreenY)
            {
                targetX = 0.5f;
                if (viewportPos.y < 0f)
                {
                    targetY = 0.05f;
                    zRotation = 0f;
                }
                else
                {
                    targetY = 0.95f;
                    zRotation = 180f;
                }
            }

            Vector3 newWorldPos = cam.ViewportToWorldPoint(new Vector3(targetX, targetY, viewportPos.z));
            newWorldPos.z = cfg.originalPos.z;

            cfg.arrowObject.transform.position = newWorldPos;
            cfg.arrowObject.transform.rotation = Quaternion.Euler(0, 0, zRotation);
        }
        else if (!isOffScreenX && !isOffScreenY)
        {
            cfg.arrowObject.SetActive(true);

            float floatOffset = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            Vector3 newPos = cfg.originalPos;

            if (cfg.floatDir == FloatDirection.Vertical)
            {
                newPos.y += floatOffset;
            }
            else
            {
                newPos.x += floatOffset;
            }

            cfg.arrowObject.transform.position = newPos;
            cfg.arrowObject.transform.rotation = cfg.originalRot;
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
            return gm.CurrentDay == 1 &&
                   (current == Step.OpenTavern ||
                    current == Step.OpenBar ||
                    current == Step.ChooseOrder ||
                    current == Step.ChooseDish ||
                    current == Step.FirstServed);
        }
    }
}