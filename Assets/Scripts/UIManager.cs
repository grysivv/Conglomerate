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

    // Główny układ
    private Label timeLabel;
    private Label cashLabel;
    private Label truckLabel;

    private Button navDashboardBtn;
    private Button navMarketBtn;
    private Button navFactoryBtn;

    private Button globalBackBtn;
    private Button pauseBtn;
    private Button speed1Btn;
    private Button speed3Btn;

    // Modale
    private VisualElement modalOverlay;
    private Label modalTitle;
    private Label modalMessage;
    private Button modalCloseBtn;

    // Ekrany
    private VisualElement screenDashboardInstance;
    private VisualElement screenMarketInstance;
    private VisualElement screenFactoryInstance;

    private ScreenType currentScreen = ScreenType.Dashboard;

    // Dedykowane kontrolery
    private DashboardUIController dashboardUIController;
    private MarketUIController marketUIController;
    private FactoryUIController factoryUIController;

    private void OnEnable()
    {
        TimeManager.OnHourlyTick += RefreshHUD;

        if (uiDoc == null) return;
        var root = uiDoc.rootVisualElement;

        // Header
        timeLabel = root.Q<Label>("TimeLabel");
        cashLabel = root.Q<Label>("CashLabel");
        truckLabel = root.Q<Label>("TruckLabel");

        // Nawigacja
        navDashboardBtn = root.Q<Button>("NavDashboardBtn");
        navMarketBtn = root.Q<Button>("NavMarketBtn");
        navFactoryBtn = root.Q<Button>("NavFactoryBtn");

        if (navDashboardBtn != null) navDashboardBtn.RegisterCallback<ClickEvent>(ev => SwitchScreen(ScreenType.Dashboard));
        if (navMarketBtn != null) navMarketBtn.RegisterCallback<ClickEvent>(ev => SwitchScreen(ScreenType.Market));
        if (navFactoryBtn != null) navFactoryBtn.RegisterCallback<ClickEvent>(ev => SwitchScreen(ScreenType.Factory));

        // Paski i Ekrany z instancji
        screenDashboardInstance = root.Q<VisualElement>("ScreenDashboard");
        screenMarketInstance = root.Q<VisualElement>("ScreenMarket");
        screenFactoryInstance = root.Q<VisualElement>("ScreenFactory");

        // Stopka
        globalBackBtn = root.Q<Button>("GlobalBackBtn");
        if (globalBackBtn != null) globalBackBtn.RegisterCallback<ClickEvent>(ev => SwitchScreen(ScreenType.Dashboard));

        pauseBtn = root.Q<Button>("PauseBtn");
        if (pauseBtn != null) pauseBtn.RegisterCallback<ClickEvent>(OnPauseBtnClicked);

        speed1Btn = root.Q<Button>("Speed1Btn");
        if (speed1Btn != null) speed1Btn.RegisterCallback<ClickEvent>(OnSpeed1BtnClicked);

        speed3Btn = root.Q<Button>("Speed3Btn");
        if (speed3Btn != null) speed3Btn.RegisterCallback<ClickEvent>(OnSpeed3BtnClicked);

        // Inicjalizacja Modala
        modalOverlay = root.Q<VisualElement>("ModalOverlay");
        modalTitle = root.Q<Label>("ModalTitle");
        modalMessage = root.Q<Label>("ModalMessage");
        modalCloseBtn = root.Q<Button>("ModalCloseBtn");

        if (modalCloseBtn != null) modalCloseBtn.RegisterCallback<ClickEvent>(ev => CloseModal());

        // Inicjalizacja kontrolerów ekranów
        dashboardUIController = new DashboardUIController(root, this);
        dashboardUIController.Initialize();

        marketUIController = new MarketUIController(root, this);
        marketUIController.Initialize();

        factoryUIController = new FactoryUIController(root, this);
        factoryUIController.Initialize();

        SwitchScreen(ScreenType.Dashboard);
        RefreshHUD();
        UpdateTextsDynamic();
    }

    private void OnDisable()
    {
        TimeManager.OnHourlyTick -= RefreshHUD;

        dashboardUIController?.Dispose();
        marketUIController?.Dispose();
        factoryUIController?.Dispose();
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

    private void OnPauseBtnClicked(ClickEvent ev) => timeManager?.SetPause(true);
    private void OnSpeed1BtnClicked(ClickEvent ev) => timeManager?.SetSpeed(1.0f);
    private void OnSpeed3BtnClicked(ClickEvent ev) => timeManager?.SetSpeed(5.0f);

    private void SwitchScreen(ScreenType targetScreen)
    {
        if (screenDashboardInstance == null || screenMarketInstance == null || screenFactoryInstance == null) return;
        currentScreen = targetScreen;

        screenDashboardInstance.style.display = DisplayStyle.None;
        screenMarketInstance.style.display = DisplayStyle.None;
        screenFactoryInstance.style.display = DisplayStyle.None;

        if (navDashboardBtn != null) navDashboardBtn.RemoveFromClassList("active");
        if (navMarketBtn != null) navMarketBtn.RemoveFromClassList("active");
        if (navFactoryBtn != null) navFactoryBtn.RemoveFromClassList("active");

        if (targetScreen == ScreenType.Dashboard)
        {
            screenDashboardInstance.style.display = DisplayStyle.Flex;
            if (globalBackBtn != null) globalBackBtn.style.display = DisplayStyle.None;
            if (navDashboardBtn != null) navDashboardBtn.AddToClassList("active");
        }
        else if (targetScreen == ScreenType.Market)
        {
            screenMarketInstance.style.display = DisplayStyle.Flex;
            if (globalBackBtn != null) globalBackBtn.style.display = DisplayStyle.Flex;
            if (navMarketBtn != null) navMarketBtn.AddToClassList("active");
        }
        else if (targetScreen == ScreenType.Factory)
        {
            screenFactoryInstance.style.display = DisplayStyle.Flex;
            if (globalBackBtn != null) globalBackBtn.style.display = DisplayStyle.Flex;
            if (navFactoryBtn != null) navFactoryBtn.AddToClassList("active");
        }
    }

    public void RefreshHUD()
    {
        if (timeLabel != null && timeManager != null)
            timeLabel.text = $"Dzień: {timeManager.currentDay} | Zegar: {timeManager.currentHour:00}:00 ({timeManager.currentPhase})";

        if (cashLabel != null && corporationManager != null)
            cashLabel.text = $"Fundusze: {corporationManager.cash:N2} USD";

        marketUIController?.RefreshHUD();
        factoryUIController?.RefreshHUD();
    }

    private void UpdateTextsDynamic()
    {
        dashboardUIController?.UpdateTextsDynamic();
        factoryUIController?.UpdateTextsDynamic();
    }

    public void AddSpecificRoute(BuildingBase source, DestinationType destType, BuildingBase dest, ResourceType resType)
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

    public void SellInstant(ResourceType type, int amount)
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

    public void HandleBuySiliconPlot()
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

    public void HandleBuyCoalPlot()
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

    public void HandleBuyFactory()
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

    public void ShowModal(string title, string message)
    {
        if (modalOverlay != null)
        {
            if (modalTitle != null) modalTitle.text = title;
            if (modalMessage != null) modalMessage.text = message;
            modalOverlay.style.display = DisplayStyle.Flex;
        }
    }

    public void CloseModal()
    {
        if (modalOverlay != null)
        {
            modalOverlay.style.display = DisplayStyle.None;
        }
    }
}
