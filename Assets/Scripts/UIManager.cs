using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

public enum ScreenType { Dashboard, Market, Factory }

public class UIManager : MonoBehaviour
{
    [Header("Menedżerowie")]
    public TimeManager timeManager;
    public CorporationManager corporationManager;
    public GlobalInventoryManager globalInventory;
    public MarketManager marketManager;
    public FleetManager fleetManager;

    [Header("UI Document")]
    public UIDocument uiDoc;

    [Header("Referencje do Kopalni")]
    public ResourceExtractor siliconMine;
    public ResourceExtractor coalMine;

    [Header("Koszty i Złoża")]
    public double siliconPlotCost = 100000.00;
    public int siliconDepositAmount = 10000;
    public double coalPlotCost = 50000.00;
    public int coalDepositAmount = 25000;

    [Header("Referencja do Fabryki")]
    public BuildingBase factoryBase;
    public ResourceManufacturer factoryManufacturer;
    public InventoryComponent factoryInventory;
    public HumanResourcesComponent factoryHR;

    [Header("Koszty Fabryki")]
    public double factoryCost = 250000.00;

    // UI Elements
    private Label timeLabel;
    private Label cashLabel;
    private Label truckLabel;

    private Button navDashboardBtn;
    private Button navMarketBtn;
    private Button navFactoryBtn;

    private VisualElement screenDashboard;
    private VisualElement screenMarket;
    private VisualElement screenFactory;

    private Label siliconDepositLabel;
    private Button buySiliconPlotBtn;
    private Label coalDepositLabel;
    private Button buyCoalPlotBtn;

    private Label siliconLabel;
    private Button sellSiliconBtn;
    private Label coalLabel;
    private Button sellCoalBtn;
    private Label microchipLabel;
    private Button sellChipBtn;

    private Label siliconMarketLabel;
    private Label coalMarketLabel;
    private Label microchipMarketLabel;

    private Button routeMineToGlobalSiliconBtn;
    private Button routeMineToFactorySiliconBtn;
    private Button routeMineToGlobalCoalBtn;
    private Button routeGlobalToFactorySiliconBtn;
    private Button routeGlobalToFactoryCoalBtn;
    private Button routeFactoryToGlobalChipBtn;
    private Label activeRoutesLabel;

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

    private Button globalBackBtn;
    private Button pauseBtn;
    private Button speed1Btn;
    private Button speed3Btn;

    private ScreenType currentScreen = ScreenType.Dashboard;

    private void OnEnable()
    {
        TimeManager.OnHourlyTick += RefreshHUD;

        if (uiDoc == null) return;
        var root = uiDoc.rootVisualElement;

        // Header
        timeLabel = root.Q<Label>("TimeLabel");
        cashLabel = root.Q<Label>("CashLabel");
        truckLabel = root.Q<Label>("TruckLabel");

        // Nav
        navDashboardBtn = root.Q<Button>("NavDashboardBtn");
        navMarketBtn = root.Q<Button>("NavMarketBtn");
        navFactoryBtn = root.Q<Button>("NavFactoryBtn");

        navDashboardBtn?.RegisterCallback<ClickEvent>(ev => SwitchScreen(ScreenType.Dashboard));
        navMarketBtn?.RegisterCallback<ClickEvent>(ev => SwitchScreen(ScreenType.Market));
        navFactoryBtn?.RegisterCallback<ClickEvent>(ev => SwitchScreen(ScreenType.Factory));

        // Screens
        screenDashboard = root.Q<VisualElement>("ScreenDashboard");
        screenMarket = root.Q<VisualElement>("ScreenMarket");
        screenFactory = root.Q<VisualElement>("ScreenFactory");

        // Dashboard
        siliconDepositLabel = root.Q<Label>("SiliconDepositLabel");
        buySiliconPlotBtn = root.Q<Button>("BuySiliconPlotBtn");
        buySiliconPlotBtn?.RegisterCallback<ClickEvent>(OnBuySiliconPlotBtnClicked);

        coalDepositLabel = root.Q<Label>("CoalDepositLabel");
        buyCoalPlotBtn = root.Q<Button>("BuyCoalPlotBtn");
        buyCoalPlotBtn?.RegisterCallback<ClickEvent>(OnBuyCoalPlotBtnClicked);

        // Market
        siliconLabel = root.Q<Label>("SiliconLabel");
        sellSiliconBtn = root.Q<Button>("SellSiliconBtn");
        sellSiliconBtn?.RegisterCallback<ClickEvent>(ev => SellInstant(ResourceType.Silicon, 10));

        coalLabel = root.Q<Label>("CoalLabel");
        sellCoalBtn = root.Q<Button>("SellCoalBtn");
        sellCoalBtn?.RegisterCallback<ClickEvent>(ev => SellInstant(ResourceType.Coal, 20));

        microchipLabel = root.Q<Label>("MicrochipLabel");
        sellChipBtn = root.Q<Button>("SellChipBtn");
        sellChipBtn?.RegisterCallback<ClickEvent>(ev => SellInstant(ResourceType.Microchip, 1));

        siliconMarketLabel = root.Q<Label>("SiliconMarketLabel");
        coalMarketLabel = root.Q<Label>("CoalMarketLabel");
        microchipMarketLabel = root.Q<Label>("MicrochipMarketLabel");

        // Fleet Routes
        routeMineToGlobalSiliconBtn = root.Q<Button>("RouteMineToGlobalSiliconBtn");
        routeMineToGlobalSiliconBtn?.RegisterCallback<ClickEvent>(OnRouteMineToGlobalSiliconBtnClicked);

        routeMineToFactorySiliconBtn = root.Q<Button>("RouteMineToFactorySiliconBtn");
        routeMineToFactorySiliconBtn?.RegisterCallback<ClickEvent>(OnRouteMineToFactorySiliconBtnClicked);

        routeMineToGlobalCoalBtn = root.Q<Button>("RouteMineToGlobalCoalBtn");
        routeMineToGlobalCoalBtn?.RegisterCallback<ClickEvent>(OnRouteMineToGlobalCoalBtnClicked);

        routeGlobalToFactorySiliconBtn = root.Q<Button>("RouteGlobalToFactorySiliconBtn");
        routeGlobalToFactorySiliconBtn?.RegisterCallback<ClickEvent>(OnRouteGlobalToFactorySiliconBtnClicked);

        routeGlobalToFactoryCoalBtn = root.Q<Button>("RouteGlobalToFactoryCoalBtn");
        routeGlobalToFactoryCoalBtn?.RegisterCallback<ClickEvent>(OnRouteGlobalToFactoryCoalBtnClicked);

        routeFactoryToGlobalChipBtn = root.Q<Button>("RouteFactoryToGlobalChipBtn");
        routeFactoryToGlobalChipBtn?.RegisterCallback<ClickEvent>(OnRouteFactoryToGlobalChipBtnClicked);

        activeRoutesLabel = root.Q<Label>("ActiveRoutesLabel");

        // Factory
        factoryStatusLabel = root.Q<Label>("FactoryStatusLabel");
        buyFactoryBtn = root.Q<Button>("BuyFactoryBtn");
        buyFactoryBtn?.RegisterCallback<ClickEvent>(OnBuyFactoryBtnClicked);

        factoryInventoryPanel = root.Q<VisualElement>("FactoryInventoryPanel");
        factoryLocalSiliconLabel = root.Q<Label>("FactoryLocalSiliconLabel");
        factoryLocalCoalLabel = root.Q<Label>("FactoryLocalCoalLabel");
        factoryLocalChipLabel = root.Q<Label>("FactoryLocalChipLabel");

        factoryHRLabel = root.Q<Label>("FactoryHRLabel");
        hireFactoryBtn = root.Q<Button>("HireFactoryBtn");
        hireFactoryBtn?.RegisterCallback<ClickEvent>(OnHireFactoryBtnClicked);

        fireFactoryBtn = root.Q<Button>("FireFactoryBtn");
        fireFactoryBtn?.RegisterCallback<ClickEvent>(OnFireFactoryBtnClicked);

        factoryWageLabel = root.Q<Label>("FactoryWageLabel");
        raiseFactoryWageBtn = root.Q<Button>("RaiseFactoryWageBtn");
        raiseFactoryWageBtn?.RegisterCallback<ClickEvent>(OnRaiseFactoryWageBtnClicked);

        lowerFactoryWageBtn = root.Q<Button>("LowerFactoryWageBtn");
        lowerFactoryWageBtn?.RegisterCallback<ClickEvent>(OnLowerFactoryWageBtnClicked);

        // Footer Controls
        globalBackBtn = root.Q<Button>("GlobalBackBtn");
        globalBackBtn?.RegisterCallback<ClickEvent>(ev => SwitchScreen(ScreenType.Dashboard));

        pauseBtn = root.Q<Button>("PauseBtn");
        pauseBtn?.RegisterCallback<ClickEvent>(OnPauseBtnClicked);

        speed1Btn = root.Q<Button>("Speed1Btn");
        speed1Btn?.RegisterCallback<ClickEvent>(OnSpeed1BtnClicked);

        speed3Btn = root.Q<Button>("Speed3Btn");
        speed3Btn?.RegisterCallback<ClickEvent>(OnSpeed3BtnClicked);

        SwitchScreen(ScreenType.Dashboard);
        RefreshHUD();
        UpdateTextsDynamic();
    }

    private void OnDisable()
    {
        TimeManager.OnHourlyTick -= RefreshHUD;
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            SwitchScreen(ScreenType.Dashboard);
        }
        UpdateTextsDynamic();

        if (truckLabel != null && fleetManager != null && fleetManager.activeRoutes != null)
        {
            if (fleetManager.activeRoutes.Count > 0)
            {
                TransportRoute active = null;
                foreach (var r in fleetManager.activeRoutes)
                {
                    if (r.isEnRoute)
                    {
                        active = r;
                        break;
                    }
                }

                if (active != null)
                {
                    truckLabel.text = $"Transport: {active.resourceType} w drodze ({Mathf.RoundToInt(active.currentJourneyProgress * 100f)}%)";
                }
                else
                {
                    truckLabel.text = "Transport: Oczekuje na załadunek";
                }
            }
            else
            {
                truckLabel.text = "Transport: Brak aktywnych tras";
            }
        }
    }

    private void OnBuySiliconPlotBtnClicked(ClickEvent ev) => HandleBuySiliconPlot();
    private void OnBuyCoalPlotBtnClicked(ClickEvent ev) => HandleBuyCoalPlot();

    private void OnRouteMineToGlobalSiliconBtnClicked(ClickEvent ev) => AddSpecificRoute(siliconMine?.GetComponent<BuildingBase>(), DestinationType.GlobalInventory, null, ResourceType.Silicon);
    private void OnRouteMineToFactorySiliconBtnClicked(ClickEvent ev) => AddSpecificRoute(siliconMine?.GetComponent<BuildingBase>(), DestinationType.Factory, factoryBase, ResourceType.Silicon);
    private void OnRouteMineToGlobalCoalBtnClicked(ClickEvent ev) => AddSpecificRoute(coalMine?.GetComponent<BuildingBase>(), DestinationType.GlobalInventory, null, ResourceType.Coal);
    private void OnRouteGlobalToFactorySiliconBtnClicked(ClickEvent ev) => AddSpecificRoute(null, DestinationType.Factory, factoryBase, ResourceType.Silicon);
    private void OnRouteGlobalToFactoryCoalBtnClicked(ClickEvent ev) => AddSpecificRoute(null, DestinationType.Factory, factoryBase, ResourceType.Coal);
    private void OnRouteFactoryToGlobalChipBtnClicked(ClickEvent ev) => AddSpecificRoute(factoryBase, DestinationType.GlobalInventory, null, ResourceType.Microchip);

    private void OnBuyFactoryBtnClicked(ClickEvent ev) => HandleBuyFactory();
    private void OnHireFactoryBtnClicked(ClickEvent ev) { factoryHR?.HireWorker(); RefreshHUD(); }
    private void OnFireFactoryBtnClicked(ClickEvent ev) { factoryHR?.FireWorker(); RefreshHUD(); }
    private void OnRaiseFactoryWageBtnClicked(ClickEvent ev) { factoryHR?.AdjustWage(10); RefreshHUD(); }
    private void OnLowerFactoryWageBtnClicked(ClickEvent ev) { factoryHR?.AdjustWage(-10); RefreshHUD(); }

    private void OnPauseBtnClicked(ClickEvent ev) => timeManager?.SetPause(true);
    private void OnSpeed1BtnClicked(ClickEvent ev) => timeManager?.SetSpeed(1.0f);
    private void OnSpeed3BtnClicked(ClickEvent ev) => timeManager?.SetSpeed(5.0f);


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

    private void AddSpecificRoute(BuildingBase source, DestinationType destType, BuildingBase dest, ResourceType resType)
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

            if (fleetManager.activeRoutes != null)
            {
                fleetManager.activeRoutes.Add(newRoute);
            }
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
        if (factoryBase != null)
        {
            if (factoryHRLabel != null && factoryHR != null) factoryHRLabel.text = $"Inżynierowie: {factoryHR.currentWorkers} / {factoryHR.maxWorkers} | Efektywność: {Mathf.RoundToInt(factoryHR.laborEfficiency * 100f)}%";
            if (factoryWageLabel != null && factoryHR != null) factoryWageLabel.text = $"Oferowana Pensja: {factoryHR.currentSalary:F2} USD/h";

            if (factoryBase.isBuilt)
            {
                if (factoryInventoryPanel != null) factoryInventoryPanel.style.display = DisplayStyle.Flex;
                if (factoryInventory != null)
                {
                    if (factoryLocalSiliconLabel != null) factoryLocalSiliconLabel.text = $"Krzem: {factoryInventory.GetStock(ResourceType.Silicon)} / {factoryInventory.GetCapacity(ResourceType.Silicon)} t";
                    if (factoryLocalCoalLabel != null) factoryLocalCoalLabel.text = $"Węgiel: {factoryInventory.GetStock(ResourceType.Coal)} / {factoryInventory.GetCapacity(ResourceType.Coal)} t";
                    if (factoryLocalChipLabel != null) factoryLocalChipLabel.text = $"Procesory: {factoryInventory.GetStock(ResourceType.Microchip)} / {factoryInventory.GetCapacity(ResourceType.Microchip)} szt.";
                }
            }
            else
            {
                if (factoryInventoryPanel != null) factoryInventoryPanel.style.display = DisplayStyle.None;
            }
        }
        if (activeRoutesLabel != null && fleetManager != null && fleetManager.activeRoutes != null)
        {
            activeRoutesLabel.text = $"Zlecone trasy (Aktywne ciężarówki): {fleetManager.activeRoutes.Count}";
        }
    }

    private void UpdateTextsDynamic()
    {
        if (siliconMine != null)
        {
            if (!siliconMine.hasPlotPurchased) { if (siliconDepositLabel != null) siliconDepositLabel.text = "Złoże Krzemu: Brak dostępu"; if (buySiliconPlotBtn != null) buySiliconPlotBtn.style.display = DisplayStyle.Flex; }
            else { if (siliconDepositLabel != null) siliconDepositLabel.text = siliconMine.remainingDeposit <= 0 ? "Złoże Krzemu: WYCZERPANE" : $"Złoże Krzemu: {siliconMine.remainingDeposit} t"; if (buySiliconPlotBtn != null) buySiliconPlotBtn.style.display = DisplayStyle.None; }
        }
        if (coalMine != null)
        {
            if (!coalMine.hasPlotPurchased) { if (coalDepositLabel != null) coalDepositLabel.text = "Złoże Węgla: Brak dostępu"; if (buyCoalPlotBtn != null) buyCoalPlotBtn.style.display = DisplayStyle.Flex; }
            else { if (coalDepositLabel != null) coalDepositLabel.text = coalMine.remainingDeposit <= 0 ? "Złoże Węgla: WYCZERPANE" : $"Złoże Węgla: {coalMine.remainingDeposit} t"; if (buyCoalPlotBtn != null) buyCoalPlotBtn.style.display = DisplayStyle.None; }
        }
        if (factoryBase != null)
        {
            if (!factoryBase.isBuilt) { if (factoryStatusLabel != null) factoryStatusLabel.text = "Fabryka: Nie wybudowano"; if (buyFactoryBtn != null) buyFactoryBtn.style.display = DisplayStyle.Flex; }
            else
            {
                if (buyFactoryBtn != null) buyFactoryBtn.style.display = DisplayStyle.None;
                if (factoryHR != null && factoryHR.currentWorkers <= 0) { if (factoryStatusLabel != null) factoryStatusLabel.text = "Fabryka: Brak pracowników! Przestój."; }
                else if (factoryManufacturer != null && factoryManufacturer.recipe == null) { if (factoryStatusLabel != null) factoryStatusLabel.text = "Fabryka: BŁĄD! Brak receptury (Dodaj w Edytorze)."; }
                else if (factoryManufacturer != null && factoryManufacturer.isProducing) { if (factoryStatusLabel != null) factoryStatusLabel.text = $"Fabryka: Produkcja ({factoryManufacturer.currentProductionTimer}/{factoryManufacturer.recipe.productionTimeHours}h)"; }
                else { if (factoryStatusLabel != null) factoryStatusLabel.text = "Fabryka: Oczekiwanie na surowce"; }
            }
        }
    }

    private void HandleBuySiliconPlot()
    {
        if (siliconMine == null || corporationManager == null || siliconMine.hasPlotPurchased) return;
        if (corporationManager.cash >= siliconPlotCost)
        {
            corporationManager.cash -= siliconPlotCost;

            siliconMine.hasPlotPurchased = true;
            siliconMine.remainingDeposit = siliconDepositAmount;

            var baseBuilding = siliconMine.GetComponent<BuildingBase>();
            if (baseBuilding != null) baseBuilding.isBuilt = true;

            RefreshHUD();
        }
    }

    private void HandleBuyCoalPlot()
    {
        if (coalMine == null || corporationManager == null || coalMine.hasPlotPurchased) return;
        if (corporationManager.cash >= coalPlotCost)
        {
            corporationManager.cash -= coalPlotCost;

            coalMine.hasPlotPurchased = true;
            coalMine.remainingDeposit = coalDepositAmount;

            var baseBuilding = coalMine.GetComponent<BuildingBase>();
            if (baseBuilding != null) baseBuilding.isBuilt = true;

            RefreshHUD();
        }
    }

    private void HandleBuyFactory()
    {
        if (factoryBase == null || corporationManager == null || factoryBase.isBuilt) return;
        if (corporationManager.cash >= factoryCost)
        {
            corporationManager.cash -= factoryCost;
            factoryBase.isBuilt = true;
            Debug.Log("<b><color=#9c27b0>[FABRYKA]</color></b> Fabryka procesorów gotowa do pracy! Czeka na inżynierów.");
            RefreshHUD();
        }
    }
}
