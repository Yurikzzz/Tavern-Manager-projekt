using System.Collections;
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

    private GameTimeManager timeManager;
    private OrderManager orderManager;
    private BarUIController barUI;

    private RectTransform popupRect;
    private Vector3 recordedDefaultLocalPos;
    private Vector3 recordedDefaultWorldPos;
    private bool hasRecordedDefault = false;

    private enum Step
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

        if (current == Step.ServeUntilClose)
        {
        }

        UpdatePopupLocation();
    }

    private void SetStep(Step step, string message)
    {
        current = step;
        if (string.IsNullOrEmpty(message))
            HidePopup();
        else
            ShowPopup(message);
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

        if (barOpen && barOpenAnchor != null)
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