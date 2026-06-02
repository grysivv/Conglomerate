using UnityEngine;
using UnityEngine.UIElements;

public class CompanyPanelUI : MonoBehaviour
{
    public CorporationManager corporationManager;
    private UIDocument uiDocument;
    private Label cashLabel;
    private Label expensesLabel;
    private VisualElement root;

    void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument != null)
        {
            root = uiDocument.rootVisualElement;
            if (root != null)
            {
                cashLabel = root.Q<Label>("cash-label");
                expensesLabel = root.Q<Label>("expenses-label");
            }
        }
        TimeManager.OnHourlyTick += UpdatePanel;
    }

    void OnDisable()
    {
        TimeManager.OnHourlyTick -= UpdatePanel;
    }

    private void UpdatePanel()
    {
        if (corporationManager == null || cashLabel == null) return;

        cashLabel.text = $"Gotówka: {corporationManager.cash:F2} USD";
        if (expensesLabel != null)
        {
            expensesLabel.text = $"Stałe koszty (HQ): {corporationManager.baseHQMaintenanceCost:F2} USD/h";
        }
    }
}
