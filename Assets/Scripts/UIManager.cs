// UIManager.cs
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public TimeManager timeManager;
    public CorporationManager corporationManager;

    private Label timeLabel;
    private Label cashLabel;

    // Referencje do przycisków czasu
    private Button pauseBtn;
    private Button speed1Btn;
    private Button speed3Btn;

    void OnEnable()
    {
        UIDocument uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null) return;
        VisualElement root = uiDocument.rootVisualElement;

        // Wiązanie napisów
        timeLabel = root.Q<Label>("TimeLabel");
        cashLabel = root.Q<Label>("CashLabel");

        // Wiązanie przycisków z UI Buildera po ich nazwach Name
        pauseBtn = root.Q<Button>("PauseBtn");
        speed1Btn = root.Q<Button>("Speed1Btn");
        speed3Btn = root.Q<Button>("Speed3Btn");

        // Przypisanie funkcji pod kliknięcia (Events)
        if (pauseBtn != null) pauseBtn.clicked += () => timeManager.SetPause(true);
        if (speed1Btn != null) speed1Btn.clicked += () => timeManager.SetSpeed(1.0f);
        if (speed3Btn != null) speed3Btn.clicked += () => timeManager.SetSpeed(5.0f); // 5x przyspieszenie dla wygody testu

        TimeManager.OnHourlyTick += RefreshHUD;
    }

    void OnDisable()
    {
        TimeManager.OnHourlyTick -= RefreshHUD;
    }

    void Start() { RefreshHUD(); }

    private void RefreshHUD()
    {
        if (timeManager == null || corporationManager == null) return;
        if (timeLabel != null) timeLabel.text = $"Dzień: {timeManager.currentDay} | Zegar: {timeManager.currentHour:00}:00 ({timeManager.currentPhase})";
        if (cashLabel != null) cashLabel.text = $"Fundusze: {corporationManager.cash:N2} USD";
    }
}