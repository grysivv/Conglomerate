using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    public TimeManager timeManager;
    public CorporationManager corporationManager;
    private Label timeLabel;
    private Label cashLabel;

    void OnEnable()
    {
        UIDocument uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null) return;
        VisualElement root = uiDocument.rootVisualElement;
        timeLabel = root.Q<Label>("TimeLabel");
        cashLabel = root.Q<Label>("CashLabel");
        TimeManager.OnHourlyTick += RefreshHUD;
    }

    void OnDisable() { TimeManager.OnHourlyTick -= RefreshHUD; }
    void Start() { RefreshHUD(); }

    private void RefreshHUD()
    {
        if (timeManager == null || corporationManager == null) return;
        if (timeLabel != null) timeLabel.text = $"Dzień: {timeManager.currentDay} | Zegar: {timeManager.currentHour:00}:00 ({timeManager.currentPhase})";
        if (cashLabel != null) cashLabel.text = $"Fundusze: {corporationManager.cash:N2} USD";
    }
}