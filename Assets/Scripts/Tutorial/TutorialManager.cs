using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    // Small popup UI (assign in inspector)
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

    // Runtime references
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

    // Tracking helpers
    private int initialRoomCount = 0;
    private int previousActiveOrders = 0;
    private bool deliveryMarkerShown = false;

    private bool barEventsSubscribed = false;

    void Start()
    {
        if (popupRoot != null) popupRoot.SetActive(false);

        timeManager = GameTimeManager.Instance;
        orderManager = OrderManager.Instance;

        if (orderManager != null)
            orderManager.OnOrdersChanged += OnOrdersChanged;

        // Cache popup RectTransform and default position (used if no defaultAnchor assigned)
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

        // Start initialization a frame later so other systems can register
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
    }

    private IEnumerator InitializeNextFrame()
    {
        yield return null;

        // Wait until GameTimeManager exists
        while (GameTimeManager.Instance == null)
            yield return null;

        timeManager = GameTimeManager.Instance;
        timeManager.OnTimeChanged += OnTimeChanged;
        timeManager.OnDayChanged += OnDayChanged;

        // Try to find BarUIController (may be null until scene fully initialized)
        barUI = FindObjectOfType<BarUIController>();
        TrySubscribeBarEvents();

        // Only run the tutorial at day 1
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
        // record current active orders for later comparison
        previousActiveOrders = OrderManager.Instance != null ? OrderManager.Instance.ActiveOrders.Count : 0;
        SetStep(Step.FirstServed, "Dish prepared. Deliver it to the customer. Serve the first customer to continue.");
    }

    private void OnTimeChanged(GameTimeManager.TimeOfDay t)
    {
        // If tavern was opened (time -> Afternoon) advance step if appropriate
        if (current == Step.OpenTavern && t == GameTimeManager.TimeOfDay.Afternoon)
        {
            SetStep(Step.OpenBar, "Tavern is open! Go to the bar and open the menu (interact with the bar).");
        }

        // When tavern closes (becomes Night)
        if ((current == Step.FirstServed || current == Step.ServeUntilClose || current == Step.ChooseDish) &&
            t == GameTimeManager.TimeOfDay.Night)
        {
            SetStep(Step.GoToBed, "The tavern is closed. Go to bed to advance to the next day.");
        }
    }

    private void OnDayChanged(int newDay)
    {
        // Player slept / next day started -> finish tutorial
        if (current != Step.Finished)
        {
            SetStep(Step.Finished, "");
            HidePopup();
            enabled = false;
        }
    }

    private void OnOrdersChanged()
    {
        if (orderManager == null) return;
        previousActiveOrders = orderManager.ActiveOrders?.Count ?? 0;
    }

    void Update()
    {
        // Ensure bar events are subscribed if bar was created later
        if (!barEventsSubscribed)
        {
            if (barUI == null) barUI = FindObjectOfType<BarUIController>();
            TrySubscribeBarEvents();
        }

        if (current == Step.WaitingStart)
        {
            // Begin tutorial only on first day morning
            if (timeManager != null && timeManager.CurrentDay == 1 && timeManager.CurrentTime == GameTimeManager.TimeOfDay.Morning)
            {
                // Count rental rooms in the scene
                var rooms = FindObjectsOfType<RoomManager>();
                initialRoomCount = rooms != null ? rooms.Length : 0;

                // Start with cleaning step
                SetStep(Step.CleanRooms, $"Welcome! First, please clean all rental rooms ({initialRoomCount} rooms). Clean every mess in the rooms to continue.");
            }
            UpdatePopupLocation();
            return;
        }

        if (current == Step.CleanRooms)
        {
            // Consider the step complete when all RoomManager instances are clean
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
            // Fallback: if bar UI is open but events are not firing for some reason, advance to choose-order
            if (barUI == null) barUI = FindObjectOfType<BarUIController>();
            if (barUI != null && barUI.barUIRoot != null && barUI.barUIRoot.activeInHierarchy)
            {
                SetStep(Step.ChooseOrder, "Bar menu open. First select an order from the orders list.");
            }
        }

        if (current == Step.FirstServed)
        {
            // Detect first served by observing ActiveOrders count drop (or explicit removal)
            if (OrderManager.Instance != null)
            {
                int currentCount = OrderManager.Instance.ActiveOrders.Count;
                if (currentCount < previousActiveOrders)
                {
                    SetStep(Step.ServeUntilClose, "Nice! Serve customers until the tavern closes. Keep serving until the tavern closes or you close it manually.");
                }
            }
        }

        if (current == Step.ServeUntilClose)
        {
            // Wait handled in OnTimeChanged: when time becomes Night we show go to bed step
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

        // Update location immediately when showing
        UpdatePopupLocation();
    }

    private void HidePopup()
    {
        if (popupRoot != null)
            popupRoot.SetActive(false);
    }

    // Move popup to the correct anchor depending on game state.
    private void UpdatePopupLocation()
    {
        if (popupRoot == null) return;
        if (!popupRoot.activeInHierarchy) return; // only reposition when visible

        // Ensure we have a BarUI reference
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
            // No inspector anchors assigned - restore recorded default position
            MovePopupTo(null);
        }
    }

    // Anchor == null means "restore recorded default position"
    private void MovePopupTo(Transform anchor)
    {
        if (popupRect == null)
        {
            // Non-UI object: move world/local position
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

        // UI element (RectTransform) handling
        if (anchor == null)
        {
            if (defaultAnchor != null)
            {
                // Use defaultAnchor if assigned
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

            // No default anchor: restore recorded local pos
            if (hasRecordedDefault)
            {
                popupRect.localPosition = recordedDefaultLocalPos;
            }
            return;
        }

        // If anchor is a UI RectTransform -> copy anchored position
        RectTransform anchorRect = anchor.GetComponent<RectTransform>();
        if (anchorRect != null)
        {
            popupRect.anchoredPosition = anchorRect.anchoredPosition;
            return;
        }

        // Anchor is a world transform -> convert to canvas local position
        SetRectPositionFromWorldPoint(anchor.position);
    }

    private void SetRectPositionFromWorldPoint(Vector3 worldPos)
    {
        Canvas canvas = popupRect.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            // No canvas found - fallback to screen point positioning
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
}