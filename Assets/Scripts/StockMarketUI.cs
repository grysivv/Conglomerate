using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class StockMarketUI : MonoBehaviour
{
    public CorporationManager corporationManager;
    private UIDocument uiDocument;
    private VisualElement root;
    private ListView competitorsList;
    private Label playerStockValueLabel;

    // Prosta symulacja wartości akcji
    private double playerStockPrice = 100.0;
    private int playerSharesIssued = 0;

    private List<NPCCompany> activeCompanies = new List<NPCCompany>();

    void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument != null)
        {
            root = uiDocument.rootVisualElement;
            if (root != null)
            {
                competitorsList = root.Q<ListView>("competitors-list");
                playerStockValueLabel = root.Q<Label>("player-stock-value");

                Button issueSharesBtn = root.Q<Button>("issue-shares-btn");
                if (issueSharesBtn != null)
                {
                    issueSharesBtn.clicked += OnIssueSharesClicked;
                }
            }
        }
        TimeManager.OnHourlyTick += UpdateMarket;
        FindCompetitors();
    }

    void OnDisable()
    {
        TimeManager.OnHourlyTick -= UpdateMarket;
        if (root != null)
        {
            Button issueSharesBtn = root.Q<Button>("issue-shares-btn");
            if (issueSharesBtn != null)
            {
                issueSharesBtn.clicked -= OnIssueSharesClicked;
            }
        }
    }

    private void FindCompetitors()
    {
        activeCompanies = new List<NPCCompany>(FindObjectsOfType<NPCCompany>());
    }

    private void UpdateMarket()
    {
        if (corporationManager == null) return;

        // Aktualizacja wartości akcji gracza (bardzo prosta logika: zależy od gotówki)
        playerStockPrice = 10.0 + (corporationManager.cash / 10000.0);

        if (playerStockValueLabel != null)
        {
            playerStockValueLabel.text = $"Wartość akcji: {playerStockPrice:F2} USD\nWyemitowano: {playerSharesIssued}";
        }

        // W pełnej wersji tutaj aktualizowalibyśmy ListView konkurentów
    }

    private void OnIssueSharesClicked()
    {
        if (corporationManager == null) return;

        int sharesToIssue = 100;
        double capitalRaised = sharesToIssue * playerStockPrice;

        corporationManager.cash += capitalRaised;
        playerSharesIssued += sharesToIssue;

        Debug.Log($"<b><color=#4db6ac>[GIEŁDA]</color></b> Wyemitowano {sharesToIssue} akcji. Pozyskano {capitalRaised:F2} USD kapitału.");
        UpdateMarket();
    }
}
