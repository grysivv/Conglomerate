// UIManager.cs
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    [Header("Powiązania z menedżerami")]
    public TimeManager timeManager;
    public CorporationManager corporationManager;
    public GlobalInventoryManager globalInventory;
    public MarketManager marketManager;
    public FleetManager fleetManager;
    public MicrochipFactory microchipFactory;

    [Header("Powiązania z Kopalniami")]
    public SiliconMine siliconMine;
    public SiliconMine coalMine;

    [Header("Koszty")]
    public double siliconPlotCost = 100000.00;
    public int siliconDepositAmount = 500;
    public double coalPlotCost = 50000.00;
    public int coalDepositAmount = 1500;
    public double factoryCost = 250000.00;

    private VisualElement screenDashboard, screenMarket, screenFactory;
    private Button navDashboardBtn, navMarketBtn, navFactoryBtn, globalBackBtn;
    private Label timeLabel, cashLabel, truckLabel;
    private Label siliconDepositLabel, coalDepositLabel;
    private Button buySiliconPlotBtn, buyCoalPlotBtn;
    private Label siliconLabel, coalLabel, microchipLabel, siliconMarketLabel, coalMarketLabel, microchipMarketLabel;
    private Label factoryStatusLabel, factoryHRLabel, factoryWageLabel;
    private Button buyFactoryBtn, hireFactoryBtn, fireFactoryBtn, raiseFactoryWageBtn, lowerFactoryWageBtn;
    private Button pauseBtn, speed1Btn, speed3Btn;

    // Logistics Route Buttons
    private Button routeMineToGlobalSiliconBtn, routeMineToFactorySiliconBtn, routeMineToGlobalCoalBtn;
    private Button routeGlobalToFactorySiliconBtn, routeGlobalToFactoryCoalBtn, routeFactoryToGlobalChipBtn;

    // Market Sell Buttons
    private Button sellSiliconBtn, sellCoalBtn, sellChipBtn;

    private Label activeRoutesLabel;

    // Factory Local UI
    private VisualElement factoryInventoryPanel;
    private Label factoryLocalSiliconLabel, factoryLocalCoalLabel, factoryLocalChipLabel;

    private enum ScreenType { Dashboard, Market, Factory }
    private ScreenType currentScreen = ScreenType.Dashboard;

    void OnEnable()
    {
        UIDocument uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null) return;
        VisualElement root = uiDocument.rootVisualElement;

        screenDashboard = root.Q<VisualElement>("ScreenDashboard");
        screenMarket = root.Q<VisualElement>("ScreenMarket");
        screenFactory = root.Q<VisualElement>("ScreenFactory");

        navDashboardBtn = root.Q<Button>("NavDashboardBtn");
        navMarketBtn = root.Q<Button>("NavMarketBtn");
        navFactoryBtn = root.Q<Button>("NavFactoryBtn");
        globalBackBtn = root.Q<Button>("GlobalBackBtn");

        navDashboardBtn?.RegisterCallback<ClickEvent>(ev => SwitchScreen(ScreenType.Dashboard));
        navMarketBtn?.RegisterCallback<ClickEvent>(ev => SwitchScreen(ScreenType.Market));
        navFactoryBtn?.RegisterCallback<ClickEvent>(ev => SwitchScreen(ScreenType.Factory));
        globalBackBtn?.RegisterCallback<ClickEvent>(ev => SwitchScreen(ScreenType.Dashboard));

        timeLabel = root.Q<Label>("TimeLabel");
        cashLabel = root.Q<Label>("CashLabel");
        truckLabel = root.Q<Label>("TruckLabel");

        siliconDepositLabel = root.Q<Label>("SiliconDepositLabel");
        coalDepositLabel = root.Q<Label>("CoalDepositLabel");

        buySiliconPlotBtn = root.Q<Button>("BuySiliconPlotBtn");
        buySiliconPlotBtn?.RegisterCallback<ClickEvent>(ev => HandleBuySiliconPlot());

        buyCoalPlotBtn = root.Q<Button>("BuyCoalPlotBtn");
        buyCoalPlotBtn?.RegisterCallback<ClickEvent>(ev => HandleBuyCoalPlot());

        siliconLabel = root.Q<Label>("SiliconLabel");
        coalLabel = root.Q<Label>("CoalLabel");
        siliconMarketLabel = root.Q<Label>("SiliconMarketLabel");
        coalMarketLabel = root.Q<Label>("CoalMarketLabel");
        microchipLabel = root.Q<Label>("MicrochipLabel");
        microchipMarketLabel = root.Q<Label>("MicrochipMarketLabel");

        // UI Hookups for New Buttons
        sellSiliconBtn = root.Q<Button>("SellSiliconBtn");
        sellCoalBtn = root.Q<Button>("SellCoalBtn");
        sellChipBtn = root.Q<Button>("SellChipBtn");

        sellSiliconBtn?.RegisterCallback<ClickEvent>(ev => SellInstant(ResourceType.Silicon, 10));
        sellCoalBtn?.RegisterCallback<ClickEvent>(ev => SellInstant(ResourceType.Coal, 10));
        sellChipBtn?.RegisterCallback<ClickEvent>(ev => SellInstant(ResourceType.Microchip, 1));

        routeMineToGlobalSiliconBtn = root.Q<Button>("RouteMineToGlobalSiliconBtn");
        routeMineToFactorySiliconBtn = root.Q<Button>("RouteMineToFactorySiliconBtn");
        routeMineToGlobalCoalBtn = root.Q<Button>("RouteMineToGlobalCoalBtn");
        routeGlobalToFactorySiliconBtn = root.Q<Button>("RouteGlobalToFactorySiliconBtn");
        routeGlobalToFactoryCoalBtn = root.Q<Button>("RouteGlobalToFactoryCoalBtn");
        routeFactoryToGlobalChipBtn = root.Q<Button>("RouteFactoryToGlobalChipBtn");

        activeRoutesLabel = root.Q<Label>("ActiveRoutesLabel");

        routeMineToGlobalSiliconBtn?.RegisterCallback<ClickEvent>(ev => AddSpecificRoute(siliconMine, DestinationType.GlobalInventory, null, ResourceType.Silicon));
        routeMineToFactorySiliconBtn?.RegisterCallback<ClickEvent>(ev => AddSpecificRoute(siliconMine, DestinationType.Factory, microchipFactory, ResourceType.Silicon));
        routeMineToGlobalCoalBtn?.RegisterCallback<ClickEvent>(ev => AddSpecificRoute(coalMine, DestinationType.GlobalInventory, null, ResourceType.Coal));
        routeGlobalToFactorySiliconBtn?.RegisterCallback<ClickEvent>(ev => AddSpecificRoute(null, DestinationType.Factory, microchipFactory, ResourceType.Silicon));
        routeGlobalToFactoryCoalBtn?.RegisterCallback<ClickEvent>(ev => AddSpecificRoute(null, DestinationType.Factory, microchipFactory, ResourceType.Coal));
        routeFactoryToGlobalChipBtn?.RegisterCallback<ClickEvent>(ev => AddSpecificRoute(microchipFactory, DestinationType.GlobalInventory, null, ResourceType.Microchip));

        factoryInventoryPanel = root.Q<VisualElement>("FactoryInventoryPanel");
        factoryLocalSiliconLabel = root.Q<Label>("FactoryLocalSiliconLabel");
        factoryLocalCoalLabel = root.Q<Label>("FactoryLocalCoalLabel");
        factoryLocalChipLabel = root.Q<Label>("FactoryLocalChipLabel");

        factoryStatusLabel = root.Q<Label>("FactoryStatusLabel");
        buyFactoryBtn = root.Q<Button>("BuyFactoryBtn");
        buyFactoryBtn?.RegisterCallback<ClickEvent>(ev => HandleBuyFactory());

        factoryHRLabel = root.Q<Label>("FactoryHRLabel");
        factoryWageLabel = root.Q<Label>("FactoryWageLabel");

        root.Q<Button>("HireFactoryBtn")?.RegisterCallback<ClickEvent>(ev => { microchipFactory?.HireWorker(); RefreshHUD(); });
        root.Q<Button>("FireFactoryBtn")?.RegisterCallback<ClickEvent>(ev => { microchipFactory?.FireWorker(); RefreshHUD(); });
        root.Q<Button>("RaiseFactoryWageBtn")?.RegisterCallback<ClickEvent>(ev => { microchipFactory?.AdjustWage(10); RefreshHUD(); });
        root.Q<Button>("LowerFactoryWageBtn")?.RegisterCallback<ClickEvent>(ev => { microchipFactory?.AdjustWage(-10); RefreshHUD(); });

        root.Q<Button>("PauseBtn")?.RegisterCallback<ClickEvent>(ev => timeManager?.SetPause(true));
        root.Q<Button>("Speed1Btn")?.RegisterCallback<ClickEvent>(ev => timeManager?.SetSpeed(1.0f));
        root.Q<Button>("Speed3Btn")?.RegisterCallback<ClickEvent>(ev => timeManager?.SetSpeed(5.0f));

        TimeManager.OnHourlyTick += RefreshHUD;
        SwitchScreen(ScreenType.Dashboard);
    }

    void OnDisable() { TimeManager.OnHourlyTick -= RefreshHUD; }
    void Start() { RefreshHUD(); }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame && currentScreen != ScreenType.Dashboard)
        {
            SwitchScreen(ScreenType.Dashboard);
        }

        if (truckLabel != null && fleetManager != null)
        {
            bool routeFound = false;
            if (fleetManager.activeRoutes != null)
            {
                foreach (var route in fleetManager.activeRoutes)
                {
                    if (route.isEnRoute)
                    {
                        int progressPercent = Mathf.RoundToInt(route.currentJourneyProgress * 100f);
                        truckLabel.text = $"Transport: {route.resourceType} ({progressPercent}%)";
                        routeFound = true;
                        break;
                    }
                }
            }
            if (!routeFound) truckLabel.text = "Transport: Oczekiwanie w bazie";
        }

        UpdateTextsDynamic();
    }

    private void SwitchScreen(ScreenType targetScreen)
    {
        if (screenDashboard == null || screenMarket == null || screenFactory == null) return;
        currentScreen = targetScreen;

        screenDashboard.style.display = DisplayStyle.None;
        screenMarket.style.display = DisplayStyle.None;
        screenFactory.style.display = DisplayStyle.None;

        if (targetScreen == ScreenType.Dashboard) { screenDashboard.style.display = DisplayStyle.Flex; if (globalBackBtn != null) globalBackBtn.style.display = DisplayStyle.None; }
        if (targetScreen == ScreenType.Market) { screenMarket.style.display = DisplayStyle.Flex; if (globalBackBtn != null) globalBackBtn.style.display = DisplayStyle.Flex; }
        if (targetScreen == ScreenType.Factory) { screenFactory.style.display = DisplayStyle.Flex; if (globalBackBtn != null) globalBackBtn.style.display = DisplayStyle.Flex; }
    }

    private void AddSpecificRoute(ProductionBuilding source, DestinationType destType, ProductionBuilding dest, ResourceType resType)
    {
        if (fleetManager == null || corporationManager == null) return;

        double truckCost = 5000.00;
        if (corporationManager.cash >= truckCost)
        {
            corporationManager.cash -= truckCost;
            TransportRoute newRoute = new TransportRoute
            {
                sourceBuilding = source,
                destinationType = destType,
                destinationBuilding = dest,
                resourceType = resType,
                batchSize = (resType == ResourceType.Microchip) ? 5 : 20,
                transportDurationHours = fleetManager.defaultTransportDurationHours,
                fuelCostPerDelivery = fleetManager.defaultFuelCostPerDelivery
            };
            fleetManager.activeRoutes.Add(newRoute);
            RefreshHUD();
        }
    }

    private void SellInstant(ResourceType type, int amount)
    {
        if (globalInventory != null && marketManager != null)
        {
            if (globalInventory.GetStock(type) >= amount)
            {
                globalInventory.RemoveResource(type, amount);
                marketManager.SellResourceFromDelivery(type, amount);
                RefreshHUD();
            }
        }
    }

    private void RefreshHUD()
    {
        if (timeLabel != null && timeManager != null) timeLabel.text = $"Dzień: {timeManager.currentDay} | Zegar: {timeManager.currentHour:00}:00 ({timeManager.currentPhase})";
        if (cashLabel != null && corporationManager != null) cashLabel.text = $"Fundusze: {corporationManager.cash:N2} USD";
        if (globalInventory != null)
        {
            if (siliconLabel != null) siliconLabel.text = $"Silos Krzemu: {globalInventory.GetStock(ResourceType.Silicon)} / {globalInventory.GetCapacity(ResourceType.Silicon)} t";
            if (coalLabel != null) coalLabel.text = $"Hałda Węgla: {globalInventory.GetStock(ResourceType.Coal)} / {globalInventory.GetCapacity(ResourceType.Coal)} t";
            if (microchipLabel != null) microchipLabel.text = $"Magazyn Procesorów: {globalInventory.GetStock(ResourceType.Microchip)} / {globalInventory.GetCapacity(ResourceType.Microchip)} szt.";
        }
        if (marketManager != null)
        {
            if (siliconMarketLabel != null) siliconMarketLabel.text = $"Cena Krzemu: {marketManager.GetCurrentPrice(ResourceType.Silicon):F2} USD";
            if (coalMarketLabel != null) coalMarketLabel.text = $"Cena Węgla: {marketManager.GetCurrentPrice(ResourceType.Coal):F2} USD";
            if (microchipMarketLabel != null) microchipMarketLabel.text = $"Cena Procesorów: {marketManager.GetCurrentPrice(ResourceType.Microchip):F2} USD";
        }
        if (microchipFactory != null)
        {
            if (factoryHRLabel != null) factoryHRLabel.text = $"Inżynierowie: {microchipFactory.currentWorkers} / {microchipFactory.maxWorkers} | Efektywność: {Mathf.RoundToInt(microchipFactory.laborEfficiency * 100f)}%";
            if (factoryWageLabel != null) factoryWageLabel.text = $"Oferowana Pensja: {microchipFactory.currentSalary:F2} USD/h";

            if (microchipFactory.isBuilt)
            {
                if (factoryInventoryPanel != null) factoryInventoryPanel.style.display = DisplayStyle.Flex;
                if (factoryLocalSiliconLabel != null) factoryLocalSiliconLabel.text = $"Krzem: {microchipFactory.GetLocalStock(ResourceType.Silicon)} / {microchipFactory.GetLocalCapacity(ResourceType.Silicon)} t";
                if (factoryLocalCoalLabel != null) factoryLocalCoalLabel.text = $"Węgiel: {microchipFactory.GetLocalStock(ResourceType.Coal)} / {microchipFactory.GetLocalCapacity(ResourceType.Coal)} t";
                if (factoryLocalChipLabel != null) factoryLocalChipLabel.text = $"Procesory: {microchipFactory.GetLocalStock(ResourceType.Microchip)} / {microchipFactory.GetLocalCapacity(ResourceType.Microchip)} szt.";
            }
            else
            {
                if (factoryInventoryPanel != null) factoryInventoryPanel.style.display = DisplayStyle.None;
            }
        }
        if (activeRoutesLabel != null && fleetManager != null)
        {
            activeRoutesLabel.text = $"Zlecone trasy (Aktywne ciężarówki): {fleetManager.activeRoutes.Count}";
        }
    }

    private void UpdateTextsDynamic()
    {
        if (siliconMine)
        {
            if (!siliconMine.hasPlotPurchased) { if (siliconDepositLabel != null) siliconDepositLabel.text = "Złoże Krzemu: Brak dostępu"; if (buySiliconPlotBtn != null) buySiliconPlotBtn.style.display = DisplayStyle.Flex; }
            else { if (siliconDepositLabel != null) siliconDepositLabel.text = siliconMine.remainingDeposit <= 0 ? "Złoże Krzemu: WYCZERPANE" : $"Złoże Krzemu: {siliconMine.remainingDeposit} t"; if (buySiliconPlotBtn != null) buySiliconPlotBtn.style.display = DisplayStyle.None; }
        }
        if (coalMine)
        {
            if (!coalMine.hasPlotPurchased) { if (coalDepositLabel != null) coalDepositLabel.text = "Złoże Węgla: Brak dostępu"; if (buyCoalPlotBtn != null) buyCoalPlotBtn.style.display = DisplayStyle.Flex; }
            else { if (coalDepositLabel != null) coalDepositLabel.text = coalMine.remainingDeposit <= 0 ? "Złoże Węgla: WYCZERPANE" : $"Złoże Węgla: {coalMine.remainingDeposit} t"; if (buyCoalPlotBtn != null) buyCoalPlotBtn.style.display = DisplayStyle.None; }
        }
        if (microchipFactory)
        {
            if (!microchipFactory.isBuilt) { if (factoryStatusLabel != null) factoryStatusLabel.text = "Fabryka: Nie wybudowano"; if (buyFactoryBtn != null) buyFactoryBtn.style.display = DisplayStyle.Flex; }
            else
            {
                if (buyFactoryBtn != null) buyFactoryBtn.style.display = DisplayStyle.None;
                if (microchipFactory.currentWorkers <= 0) { if (factoryStatusLabel != null) factoryStatusLabel.text = "Fabryka: Brak pracowników! Przestój."; }
                else if (microchipFactory.recipe == null) { if (factoryStatusLabel != null) factoryStatusLabel.text = "Fabryka: BŁĄD! Brak receptury (Dodaj w Edytorze)."; }
                else if (microchipFactory.isProducing) { if (factoryStatusLabel != null) factoryStatusLabel.text = $"Fabryka: Produkcja ({microchipFactory.currentProductionTimer}/{microchipFactory.recipe.productionTimeHours}h)"; }
                else { if (factoryStatusLabel != null) factoryStatusLabel.text = "Fabryka: Oczekiwanie na surowce"; }
            }
        }
    }

    // DODANE BRAKUJĄCE METODY OBSŁUGI INVESTYCJI
    private void HandleBuySiliconPlot()
    {
        if (siliconMine == null || corporationManager == null || siliconMine.hasPlotPurchased) return;
        if (corporationManager.cash >= siliconPlotCost)
        {
            corporationManager.cash -= siliconPlotCost;
            siliconMine.PurchasePlot(siliconPlotCost, siliconDepositAmount);
            RefreshHUD();
        }
    }

    private void HandleBuyCoalPlot()
    {
        if (coalMine == null || corporationManager == null || coalMine.hasPlotPurchased) return;
        if (corporationManager.cash >= coalPlotCost)
        {
            corporationManager.cash -= coalPlotCost;
            coalMine.PurchasePlot(coalPlotCost, coalDepositAmount);
            RefreshHUD();
        }
    }

    private void HandleBuyFactory()
    {
        if (microchipFactory == null || corporationManager == null || microchipFactory.isBuilt) return;
        if (corporationManager.cash >= factoryCost)
        {
            corporationManager.cash -= factoryCost;
            microchipFactory.BuildFactory();
            RefreshHUD();
        }
    }
}