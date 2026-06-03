using UnityEngine;
using UnityEngine.UIElements;

public class FactoryUIController
{
    private VisualElement root;
    private UIManager uiManager;

    private Label factoryStatusLabel;
    private Button buyFactoryBtn;
    private VisualElement factoryInventoryPanel;
    private Label factoryLocalSiliconLabel;
    private Label factoryLocalCoalLabel;
    private Label factoryLocalChipLabel;

    private Label factoryHRLabel;
    private Button hireFactoryBtn;
    private Button fireFactoryBtn;
    private Label factoryWageLabel;
    private Button raiseFactoryWageBtn;
    private Button lowerFactoryWageBtn;

    public FactoryUIController(VisualElement root, UIManager uiManager)
    {
        this.root = root;
        this.uiManager = uiManager;
    }

    public void Initialize()
    {
        factoryStatusLabel = root.Q<Label>("FactoryStatusLabel");
        buyFactoryBtn = root.Q<Button>("BuyFactoryBtn");
        if (buyFactoryBtn != null) buyFactoryBtn.RegisterCallback<ClickEvent>(OnBuyFactoryBtnClicked);

        factoryInventoryPanel = root.Q<VisualElement>("FactoryInventoryPanel");
        factoryLocalSiliconLabel = root.Q<Label>("FactoryLocalSiliconLabel");
        factoryLocalCoalLabel = root.Q<Label>("FactoryLocalCoalLabel");
        factoryLocalChipLabel = root.Q<Label>("FactoryLocalChipLabel");

        factoryHRLabel = root.Q<Label>("FactoryHRLabel");
        hireFactoryBtn = root.Q<Button>("HireFactoryBtn");
        if (hireFactoryBtn != null) hireFactoryBtn.RegisterCallback<ClickEvent>(OnHireFactoryBtnClicked);

        fireFactoryBtn = root.Q<Button>("FireFactoryBtn");
        if (fireFactoryBtn != null) fireFactoryBtn.RegisterCallback<ClickEvent>(OnFireFactoryBtnClicked);

        factoryWageLabel = root.Q<Label>("FactoryWageLabel");
        raiseFactoryWageBtn = root.Q<Button>("RaiseFactoryWageBtn");
        if (raiseFactoryWageBtn != null) raiseFactoryWageBtn.RegisterCallback<ClickEvent>(OnRaiseFactoryWageBtnClicked);

        lowerFactoryWageBtn = root.Q<Button>("LowerFactoryWageBtn");
        if (lowerFactoryWageBtn != null) lowerFactoryWageBtn.RegisterCallback<ClickEvent>(OnLowerFactoryWageBtnClicked);
    }

    public void Dispose()
    {
        if (buyFactoryBtn != null) buyFactoryBtn.UnregisterCallback<ClickEvent>(OnBuyFactoryBtnClicked);
        if (hireFactoryBtn != null) hireFactoryBtn.UnregisterCallback<ClickEvent>(OnHireFactoryBtnClicked);
        if (fireFactoryBtn != null) fireFactoryBtn.UnregisterCallback<ClickEvent>(OnFireFactoryBtnClicked);
        if (raiseFactoryWageBtn != null) raiseFactoryWageBtn.UnregisterCallback<ClickEvent>(OnRaiseFactoryWageBtnClicked);
        if (lowerFactoryWageBtn != null) lowerFactoryWageBtn.UnregisterCallback<ClickEvent>(OnLowerFactoryWageBtnClicked);
    }

    private void OnBuyFactoryBtnClicked(ClickEvent ev) => uiManager.HandleBuyFactory();
    private void OnHireFactoryBtnClicked(ClickEvent ev) { uiManager.factoryHR?.HireWorker(); uiManager.RefreshHUD(); }
    private void OnFireFactoryBtnClicked(ClickEvent ev) { uiManager.factoryHR?.FireWorker(); uiManager.RefreshHUD(); }
    private void OnRaiseFactoryWageBtnClicked(ClickEvent ev) { uiManager.factoryHR?.AdjustWage(10); uiManager.RefreshHUD(); }
    private void OnLowerFactoryWageBtnClicked(ClickEvent ev) { uiManager.factoryHR?.AdjustWage(-10); uiManager.RefreshHUD(); }

    public void RefreshHUD()
    {
        if (uiManager.factoryBase != null)
        {
            if (factoryHRLabel != null && uiManager.factoryHR != null)
                factoryHRLabel.text = $"Inżynierowie: {uiManager.factoryHR.currentWorkers} / {uiManager.factoryHR.maxWorkers} | Efektywność: {Mathf.RoundToInt(uiManager.factoryHR.laborEfficiency * 100f)}%";

            if (factoryWageLabel != null && uiManager.factoryHR != null)
                factoryWageLabel.text = $"Oferowana Pensja: {uiManager.factoryHR.currentSalary:F2} USD/h";

            if (uiManager.factoryBase.isBuilt)
            {
                if (factoryInventoryPanel != null) factoryInventoryPanel.style.display = DisplayStyle.Flex;
                if (uiManager.factoryInventory != null)
                {
                    if (factoryLocalSiliconLabel != null) factoryLocalSiliconLabel.text = $"Krzem: {uiManager.factoryInventory.GetStock(ResourceType.Silicon)} / {uiManager.factoryInventory.GetCapacity(ResourceType.Silicon)} t";
                    if (factoryLocalCoalLabel != null) factoryLocalCoalLabel.text = $"Węgiel: {uiManager.factoryInventory.GetStock(ResourceType.Coal)} / {uiManager.factoryInventory.GetCapacity(ResourceType.Coal)} t";
                    if (factoryLocalChipLabel != null) factoryLocalChipLabel.text = $"Procesory: {uiManager.factoryInventory.GetStock(ResourceType.Microchip)} / {uiManager.factoryInventory.GetCapacity(ResourceType.Microchip)} szt.";
                }
            }
            else
            {
                if (factoryInventoryPanel != null) factoryInventoryPanel.style.display = DisplayStyle.None;
            }
        }
    }

    public void UpdateTextsDynamic()
    {
        if (uiManager.factoryBase != null)
        {
            if (!uiManager.factoryBase.isBuilt)
            {
                if (factoryStatusLabel != null) factoryStatusLabel.text = "Fabryka: Nie wybudowano";
                if (buyFactoryBtn != null) buyFactoryBtn.style.display = DisplayStyle.Flex;
            }
            else
            {
                if (buyFactoryBtn != null) buyFactoryBtn.style.display = DisplayStyle.None;
                if (uiManager.factoryHR != null && uiManager.factoryHR.currentWorkers <= 0)
                {
                    if (factoryStatusLabel != null) factoryStatusLabel.text = "Fabryka: Brak pracowników! Przestój.";
                }
                else if (uiManager.factoryManufacturer != null && uiManager.factoryManufacturer.recipe == null)
                {
                    if (factoryStatusLabel != null) factoryStatusLabel.text = "Fabryka: BŁĄD! Brak receptury (Dodaj w Edytorze).";
                }
                else if (uiManager.factoryManufacturer != null && uiManager.factoryManufacturer.isProducing)
                {
                    if (factoryStatusLabel != null) factoryStatusLabel.text = $"Fabryka: Produkcja ({uiManager.factoryManufacturer.currentProductionTimer}/{uiManager.factoryManufacturer.recipe.productionTimeHours}h)";
                }
                else
                {
                    if (factoryStatusLabel != null) factoryStatusLabel.text = "Fabryka: Oczekiwanie na surowce";
                }
            }
        }
    }
}
