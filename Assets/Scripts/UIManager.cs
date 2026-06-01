// UIManager.cs
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    [Header("Powiązania z menedżerami")]
    public TimeManager timeManager;
    public CorporationManager corporationManager;
    public GlobalInventoryManager globalInventoryManager; // Zmieniono z siliconMine
    public MarketManager marketManager;
    public FleetManager fleetManager; // Dodano FleetManager

    private Label timeLabel;
    private Label cashLabel;
    private Label siliconLabel;
    private Label marketLabel;
    private Label truckLabel; // Nowy label dla ciężarówki

    // Referencje do przycisków czasu
    private Button pauseBtn;
    private Button speed1Btn;
    private Button speed3Btn;

    void OnEnable()
    {
        UIDocument uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null) return;
        VisualElement root = uiDocument.rootVisualElement;

        // Wiązanie napisów z UI Buildera
        timeLabel = root.Q<Label>("TimeLabel");
        cashLabel = root.Q<Label>("CashLabel");
        siliconLabel = root.Q<Label>("SiliconLabel");
        marketLabel = root.Q<Label>("MarketLabel");
        truckLabel = root.Q<Label>("TruckLabel"); // Odnalezienie nowego napisu po Name, może go nie być

        // Wiązanie przycisków
        pauseBtn = root.Q<Button>("PauseBtn");
        speed1Btn = root.Q<Button>("Speed1Btn");
        speed3Btn = root.Q<Button>("Speed3Btn");

        if (pauseBtn != null) pauseBtn.clicked += OnPauseClicked;
        if (speed1Btn != null) speed1Btn.clicked += OnSpeed1Clicked;
        if (speed3Btn != null) speed3Btn.clicked += OnSpeed3Clicked;

        TimeManager.OnHourlyTick += RefreshHUD;
    }

    void OnDisable()
    {
        if (pauseBtn != null) pauseBtn.clicked -= OnPauseClicked;
        if (speed1Btn != null) speed1Btn.clicked -= OnSpeed1Clicked;
        if (speed3Btn != null) speed3Btn.clicked -= OnSpeed3Clicked;

        TimeManager.OnHourlyTick -= RefreshHUD;
    }

    private void OnPauseClicked()
    {
        if (timeManager != null) timeManager.SetPause(true);
    }

    private void OnSpeed1Clicked()
    {
        if (timeManager != null) timeManager.SetSpeed(1.0f);
    }

    private void OnSpeed3Clicked()
    {
        if (timeManager != null) timeManager.SetSpeed(5.0f);
    }

    void Start() { RefreshHUD(); }

    void Update()
    {
        // Aktualizacja TruckLabel co klatkę by pokazać postęp procentowy w czasie rzeczywistym
        if (truckLabel != null && fleetManager != null)
        {
            if (fleetManager.isEnRoute)
            {
                float realSecondsNeeded = fleetManager.transportDurationHours * (timeManager != null ? timeManager.baseSecondsPerHour : 1.0f);
                float progress = 0f;
                if (realSecondsNeeded > 0)
                {
                    progress = Mathf.Clamp01(fleetManager.currentJourneyTimer / realSecondsNeeded) * 100f;
                }
                truckLabel.text = $"Transport: W trasie ({progress:F0}%)";
            }
            else
            {
                truckLabel.text = "Transport: Oczekiwanie";
            }
        }
    }

    private void RefreshHUD()
    {
        if (timeManager == null || corporationManager == null) return;

        // 1. Aktualizacja czasu
        if (timeLabel != null)
            timeLabel.text = $"Dzień: {timeManager.currentDay} | Zegar: {timeManager.currentHour:00}:00 ({timeManager.currentPhase})";

        // 2. Aktualizacja finansów
        if (cashLabel != null)
            cashLabel.text = $"Fundusze: {corporationManager.cash:N2} USD";

        // 3. Aktualizacja stanu magazynu głównego
        if (siliconLabel != null && globalInventoryManager != null)
        {
            siliconLabel.text = $"Magazyn Główny: {globalInventoryManager.siliconInStock} t";
        }

        // 4. Aktualizacja rynku (jeśli rynek jest przypisany)
        if (marketLabel != null && marketManager != null)
        {
            marketLabel.text = $"Cena Krzemu: {marketManager.currentSiliconPrice:N2} USD / t";
        }
    }
}