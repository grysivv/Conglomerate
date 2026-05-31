// UIManager.cs
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    [Header("Powiązania z menedżerami")]
    public TimeManager timeManager;
    public CorporationManager corporationManager;
    public SiliconMine siliconMine; // Nowe powiązanie z kopalnią

    private Label timeLabel;
    private Label cashLabel;
    private Label siliconLabel; // Referencja do nowego napisu

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
        siliconLabel = root.Q<Label>("SiliconLabel"); // Odnalezienie nowego napisu po Name

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

    private void RefreshHUD()
    {
        if (timeManager == null || corporationManager == null) return;

        // 1. Aktualizacja czasu
        if (timeLabel != null)
            timeLabel.text = $"Dzień: {timeManager.currentDay} | Zegar: {timeManager.currentHour:00}:00 ({timeManager.currentPhase})";

        // 2. Aktualizacja finansów
        if (cashLabel != null)
            cashLabel.text = $"Fundusze: {corporationManager.cash:N2} USD";

        // 3. Aktualizacja stanu magazynu krzemu (jeśli kopalnia jest przypisana)
        if (siliconLabel != null && siliconMine != null)
        {
            siliconLabel.text = $"Magazyn Krzemu: {siliconMine.siliconStorage} t";
        }
    }
}