using UnityEngine;
using UnityEngine.UIElements;

public class DashboardUIController
{
    private VisualElement root;
    private UIManager uiManager;

    private Label siliconDepositLabel;
    private Button buySiliconPlotBtn;
    private Label coalDepositLabel;
    private Button buyCoalPlotBtn;

    public DashboardUIController(VisualElement root, UIManager uiManager)
    {
        this.root = root;
        this.uiManager = uiManager;
    }

    public void Initialize()
    {
        siliconDepositLabel = root.Q<Label>("SiliconDepositLabel");
        buySiliconPlotBtn = root.Q<Button>("BuySiliconPlotBtn");
        if (buySiliconPlotBtn != null)
            buySiliconPlotBtn.RegisterCallback<ClickEvent>(OnBuySiliconPlotBtnClicked);

        coalDepositLabel = root.Q<Label>("CoalDepositLabel");
        buyCoalPlotBtn = root.Q<Button>("BuyCoalPlotBtn");
        if (buyCoalPlotBtn != null)
            buyCoalPlotBtn.RegisterCallback<ClickEvent>(OnBuyCoalPlotBtnClicked);
    }

    public void Dispose()
    {
        if (buySiliconPlotBtn != null)
            buySiliconPlotBtn.UnregisterCallback<ClickEvent>(OnBuySiliconPlotBtnClicked);
        if (buyCoalPlotBtn != null)
            buyCoalPlotBtn.UnregisterCallback<ClickEvent>(OnBuyCoalPlotBtnClicked);
    }

    private void OnBuySiliconPlotBtnClicked(ClickEvent ev) => uiManager.HandleBuySiliconPlot();
    private void OnBuyCoalPlotBtnClicked(ClickEvent ev) => uiManager.HandleBuyCoalPlot();

    public void UpdateTextsDynamic()
    {
        if (uiManager.siliconMine != null)
        {
            if (!uiManager.siliconMine.hasPlotPurchased)
            {
                if (siliconDepositLabel != null) siliconDepositLabel.text = "Złoże Krzemu: Brak dostępu";
                if (buySiliconPlotBtn != null) buySiliconPlotBtn.style.display = DisplayStyle.Flex;
            }
            else
            {
                if (siliconDepositLabel != null)
                    siliconDepositLabel.text = uiManager.siliconMine.remainingDeposit <= 0 ? "Złoże Krzemu: WYCZERPANE" : $"Złoże Krzemu: {uiManager.siliconMine.remainingDeposit} t";
                if (buySiliconPlotBtn != null) buySiliconPlotBtn.style.display = DisplayStyle.None;
            }
        }

        if (uiManager.coalMine != null)
        {
            if (!uiManager.coalMine.hasPlotPurchased)
            {
                if (coalDepositLabel != null) coalDepositLabel.text = "Złoże Węgla: Brak dostępu";
                if (buyCoalPlotBtn != null) buyCoalPlotBtn.style.display = DisplayStyle.Flex;
            }
            else
            {
                if (coalDepositLabel != null)
                    coalDepositLabel.text = uiManager.coalMine.remainingDeposit <= 0 ? "Złoże Węgla: WYCZERPANE" : $"Złoże Węgla: {uiManager.coalMine.remainingDeposit} t";
                if (buyCoalPlotBtn != null) buyCoalPlotBtn.style.display = DisplayStyle.None;
            }
        }
    }
}
