// UIManager.cs
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem; // Wymagane dla nowego Input Systemu

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

    [Header("Koszty Działek i Zasoby")]
    public double siliconPlotCost = 100000.00;
    public int siliconDepositAmount = 500;
    public double coalPlotCost = 50000.00;
    public int coalDepositAmount = 1500;
    public double factoryCost = 250000.00;

    // --- KONTENERY EKRANÓW (ZAKŁADKI) ---
    private VisualElement screenDashboard;
    private VisualElement screenMarket;
    private VisualElement screenFactory;

    // --- PRZYCISKI NAWIGACYJNE ---
    private Button navDashboardBtn;
    private Button navMarketBtn;
    private Button navFactoryBtn;
    private Button globalBackBtn;

    // --- ELEMENTY GLOBALNE HUD ---
    private Label timeLabel;
    private Label cashLabel;
    private Label truckLabel;

    // --- ELEMENTY: PULPIT (DASHBOARD) ---
    private Label siliconDepositLabel;
    private Label coalDepositLabel;
    private Button buySiliconPlotBtn;
    private Button buyCoalPlotBtn;

    // --- ELEMENTY: RYNEK & LOGISTYKA ---
    private Label siliconLabel;
    private Label coalLabel;
    private Label siliconMarketLabel;
    private Label coalMarketLabel;

    // --- ELEMENTY: FABRYKA NR 1 & HR ---
    private Label microchipLabel;
    private Label microchipMarketLabel;
    private Label factoryStatusLabel;
    private Button buyFactoryBtn;
    private Label factoryHRLabel;
    private Label factoryWageLabel;
    private Button hireFactoryBtn;
    private Button fireFactoryBtn;
    private Button raiseFactoryWageBtn;
    private Button lowerFactoryWageBtn;

    // --- PRZYCISKI CZASU ---
    private Button pauseBtn;
    private Button speed1Btn;
    private Button speed3Btn;

    private enum ScreenType { Dashboard, Market, Factory }
    private ScreenType currentScreen = ScreenType.Dashboard;

    void OnEnable()
    {
        UIDocument uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null) return;
        VisualElement root = uiDocument.rootVisualElement;

        // 1. POWIĄZANIE KONTENERÓW
        screenDashboard = root.Q<VisualElement>("ScreenDashboard");
        screenMarket = root.Q<VisualElement>("ScreenMarket");
        screenFactory = root.Q<VisualElement>("ScreenFactory");

        // 2. POWIĄZANIE NAVI
        navDashboardBtn = root.Q<Button>("NavDashboardBtn");
        navMarketBtn = root.Q<Button>("NavMarketBtn");
        navFactoryBtn = root.Q<Button>("NavFactoryBtn");
        globalBackBtn = root.Q<Button>("GlobalBackBtn");

        if (navDashboardBtn != null) navDashboardBtn.clicked += HandleNavDashboardClick;
        if (navMarketBtn != null) navMarketBtn.clicked += HandleNavMarketClick;
        if (navFactoryBtn != null) navFactoryBtn.clicked += HandleNavFactoryClick;
        if (globalBackBtn != null) globalBackBtn.clicked += HandleNavDashboardClick;

        // 3. POWIĄZANIE RESZTY ELEMENTÓW HUD
        timeLabel = root.Q<Label>("TimeLabel");
        cashLabel = root.Q<Label>("CashLabel");
        truckLabel = root.Q<Label>("TruckLabel");

        siliconDepositLabel = root.Q<Label>("SiliconDepositLabel");
        coalDepositLabel = root.Q<Label>("CoalDepositLabel");
        buySiliconPlotBtn = root.Q<Button>("BuySiliconPlotBtn");
        if (buySiliconPlotBtn != null) buySiliconPlotBtn.clicked += HandleBuySiliconPlot;
        buyCoalPlotBtn = root.Q<Button>("BuyCoalPlotBtn");
        if (buyCoalPlotBtn != null) buyCoalPlotBtn.clicked += HandleBuyCoalPlot;

        siliconLabel = root.Q<Label>("SiliconLabel");
        coalLabel = root.Q<Label>("CoalLabel");
        siliconMarketLabel = root.Q<Label>("SiliconMarketLabel");
        coalMarketLabel = root.Q<Label>("CoalMarketLabel");

        microchipLabel = root.Q<Label>("MicrochipLabel");
        microchipMarketLabel = root.Q<Label>("MicrochipMarketLabel");
        factoryStatusLabel = root.Q<Label>("FactoryStatusLabel");
        buyFactoryBtn = root.Q<Button>("BuyFactoryBtn");
        if (buyFactoryBtn != null) buyFactoryBtn.clicked += HandleBuyFactory;

        factoryHRLabel = root.Q<Label>("FactoryHRLabel");
        factoryWageLabel = root.Q<Label>("FactoryWageLabel");
        hireFactoryBtn = root.Q<Button>("HireFactoryBtn");
        fireFactoryBtn = root.Q<Button>("FireFactoryBtn");
        raiseFactoryWageBtn = root.Q<Button>("RaiseFactoryWageBtn");
        lowerFactoryWageBtn = root.Q<Button>("LowerFactoryWageBtn");

        if (hireFactoryBtn != null) hireFactoryBtn.clicked += HandleHireFactoryWorker;
        if (fireFactoryBtn != null) fireFactoryBtn.clicked += HandleFireFactoryWorker;
        if (raiseFactoryWageBtn != null) raiseFactoryWageBtn.clicked += HandleRaiseFactoryWage;
        if (lowerFactoryWageBtn != null) lowerFactoryWageBtn.clicked += HandleLowerFactoryWage;

        pauseBtn = root.Q<Button>("PauseBtn");
        speed1Btn = root.Q<Button>("Speed1Btn");
        speed3Btn = root.Q<Button>("Speed3Btn");

        if (pauseBtn != null) pauseBtn.clicked += HandlePauseClick;
        if (speed1Btn != null) speed1Btn.clicked += HandleSpeed1Click;
        if (speed3Btn != null) speed3Btn.clicked += HandleSpeed3Click;

        TimeManager.OnHourlyTick += RefreshHUD;

        SwitchScreen(ScreenType.Dashboard);
    }

    void OnDisable()
    {
        TimeManager.OnHourlyTick -= RefreshHUD;

        if (navDashboardBtn != null) navDashboardBtn.clicked -= HandleNavDashboardClick;
        if (navMarketBtn != null) navMarketBtn.clicked -= HandleNavMarketClick;
        if (navFactoryBtn != null) navFactoryBtn.clicked -= HandleNavFactoryClick;
        if (globalBackBtn != null) globalBackBtn.clicked -= HandleNavDashboardClick;

        if (buySiliconPlotBtn != null) buySiliconPlotBtn.clicked -= HandleBuySiliconPlot;
        if (buyCoalPlotBtn != null) buyCoalPlotBtn.clicked -= HandleBuyCoalPlot;
        if (buyFactoryBtn != null) buyFactoryBtn.clicked -= HandleBuyFactory;

        if (hireFactoryBtn != null) hireFactoryBtn.clicked -= HandleHireFactoryWorker;
        if (fireFactoryBtn != null) fireFactoryBtn.clicked -= HandleFireFactoryWorker;
        if (raiseFactoryWageBtn != null) raiseFactoryWageBtn.clicked -= HandleRaiseFactoryWage;
        if (lowerFactoryWageBtn != null) lowerFactoryWageBtn.clicked -= HandleLowerFactoryWage;

        if (pauseBtn != null) pauseBtn.clicked -= HandlePauseClick;
        if (speed1Btn != null) speed1Btn.clicked -= HandleSpeed1Click;
        if (speed3Btn != null) speed3Btn.clicked -= HandleSpeed3Click;
    }

    // --- METODY OBSŁUGI ZDARZEŃ UI ---
    private void HandleNavDashboardClick() => SwitchScreen(ScreenType.Dashboard);
    private void HandleNavMarketClick() => SwitchScreen(ScreenType.Market);
    private void HandleNavFactoryClick() => SwitchScreen(ScreenType.Factory);

    private void HandleHireFactoryWorker() { if (microchipFactory) microchipFactory.HireWorker(); RefreshHUD(); }
    private void HandleFireFactoryWorker() { if (microchipFactory) microchipFactory.FireWorker(); RefreshHUD(); }
    private void HandleRaiseFactoryWage() { if (microchipFactory) microchipFactory.AdjustWage(10); RefreshHUD(); }
    private void HandleLowerFactoryWage() { if (microchipFactory) microchipFactory.AdjustWage(-10); RefreshHUD(); }

    private void HandlePauseClick() => timeManager.SetPause(true);
    private void HandleSpeed1Click() => timeManager.SetSpeed(1.0f);
    private void HandleSpeed3Click() => timeManager.SetSpeed(5.0f);

    void Start() { RefreshHUD(); }

    void Update()
    {
        // NOWA OBSŁUGA ESCAPE: Bezpieczna dla nowego Input Systemu
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (currentScreen != ScreenType.Dashboard)
            {
                Debug.Log("<b><color=#00acc1>[NAWIGACJA]</color></b> Wciśnięto ESC. Powrót do Pulpitu.");
                SwitchScreen(ScreenType.Dashboard);
            }
        }

        if (truckLabel != null && fleetManager != null)
        {
            if (fleetManager.isEnRoute)
            {
                int progressPercent = Mathf.RoundToInt(fleetManager.currentJourneyProgress * 100f);
                truckLabel.text = $"Transport: W trasie ({progressPercent}%)";
            }
            else
            {
                truckLabel.text = "Transport: Oczekiwanie w bazie";
            }
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

        switch (targetScreen)
        {
            case ScreenType.Dashboard:
                screenDashboard.style.display = DisplayStyle.Flex;
                if (globalBackBtn != null) globalBackBtn.style.display = DisplayStyle.None;
                break;
            case ScreenType.Market:
                screenMarket.style.display = DisplayStyle.Flex;
                if (globalBackBtn != null) globalBackBtn.style.display = DisplayStyle.Flex;
                break;
            case ScreenType.Factory:
                screenFactory.style.display = DisplayStyle.Flex;
                if (globalBackBtn != null) globalBackBtn.style.display = DisplayStyle.Flex;
                break;
        }
    }

    private void RefreshHUD()
    {
        if (timeManager == null || corporationManager == null || globalInventory == null || marketManager == null) return;

        if (timeLabel != null)
            timeLabel.text = $"Dzień: {timeManager.currentDay} | Zegar: {timeManager.currentHour:00}:00 ({timeManager.currentPhase})";

        if (cashLabel != null)
            cashLabel.text = $"Fundusze: {corporationManager.cash:N2} USD";

        if (siliconLabel != null)
            siliconLabel.text = $"Silos Krzemu: {globalInventory.GetStock(ResourceType.Silicon)} / {globalInventory.GetCapacity(ResourceType.Silicon)} t";

        if (coalLabel != null)
            coalLabel.text = $"Hałda Węgla: {globalInventory.GetStock(ResourceType.Coal)} / {globalInventory.GetCapacity(ResourceType.Coal)} t";

        if (microchipLabel != null)
            microchipLabel.text = $"Magazyn Procesorów: {globalInventory.GetStock(ResourceType.Microchip)} / {globalInventory.GetCapacity(ResourceType.Microchip)} szt.";

        if (siliconMarketLabel != null)
            siliconMarketLabel.text = $"Cena Krzemu: {marketManager.GetCurrentPrice(ResourceType.Silicon):F2} USD";

        if (coalMarketLabel != null)
            coalMarketLabel.text = $"Cena Węgla: {marketManager.GetCurrentPrice(ResourceType.Coal):F2} USD";

        if (microchipMarketLabel != null)
            microchipMarketLabel.text = $"Cena Procesorów: {marketManager.GetCurrentPrice(ResourceType.Microchip):F2} USD";

        if (microchipFactory != null)
        {
            if (factoryHRLabel != null)
                factoryHRLabel.text = $"Inżynierowie: {microchipFactory.currentWorkers} / {microchipFactory.maxWorkers} | Efektywność: {Mathf.RoundToInt(microchipFactory.laborEfficiency * 100f)}%";

            if (factoryWageLabel != null)
                factoryWageLabel.text = $"Oferowana Pensja: {microchipFactory.currentSalary:F2} USD/h";
        }
    }

    private void UpdateTextsDynamic()
    {
        if (siliconMine != null && siliconDepositLabel != null)
        {
            if (!siliconMine.hasPlotPurchased)
            {
                siliconDepositLabel.text = "Złoże Krzemu: Brak dostępu";
                if (buySiliconPlotBtn != null) buySiliconPlotBtn.style.display = DisplayStyle.Flex;
            }
            else
            {
                siliconDepositLabel.text = siliconMine.remainingDeposit <= 0 ? "Złoże Krzemu: WYCZERPANE" : $"Złoże Krzemu: {siliconMine.remainingDeposit} t";
                if (buySiliconPlotBtn != null) buySiliconPlotBtn.style.display = DisplayStyle.None;
            }
        }

        if (coalMine != null && coalDepositLabel != null)
        {
            if (!coalMine.hasPlotPurchased)
            {
                coalDepositLabel.text = "Złoże Węgla: Brak dostępu";
                if (buyCoalPlotBtn != null) buyCoalPlotBtn.style.display = DisplayStyle.Flex;
            }
            else
            {
                coalDepositLabel.text = coalMine.remainingDeposit <= 0 ? "Złoże Węgla: WYCZERPANE" : $"Złoże Węgla: {coalMine.remainingDeposit} t";
                if (buyCoalPlotBtn != null) buyCoalPlotBtn.style.display = DisplayStyle.None;
            }
        }

        if (microchipFactory != null && factoryStatusLabel != null)
        {
            if (!microchipFactory.isBuilt)
            {
                factoryStatusLabel.text = "Fabryka: Nie wybudowano";
                if (buyFactoryBtn != null) buyFactoryBtn.style.display = DisplayStyle.Flex;
            }
            else
            {
                if (buyFactoryBtn != null) buyFactoryBtn.style.display = DisplayStyle.None;

                if (microchipFactory.currentWorkers <= 0)
                {
                    factoryStatusLabel.text = "Fabryka: Brak pracowników! Przestój.";
                }
                else if (microchipFactory.isProducing)
                {
                    factoryStatusLabel.text = $"Fabryka: Produkcja ({microchipFactory.currentProductionTimer}/{microchipFactory.productionTimeHours}h)";
                }
                else
                {
                    factoryStatusLabel.text = "Fabryka: Oczekiwanie na surowce";
                }
            }
        }
    }

    private void HandleBuySiliconPlot()
    {
        if (siliconMine == null || corporationManager == null || siliconMine.hasPlotPurchased) return;
        if (corporationManager.cash >= siliconPlotCost) { corporationManager.cash -= siliconPlotCost; siliconMine.PurchasePlot(siliconPlotCost, siliconDepositAmount); RefreshHUD(); }
    }

    private void HandleBuyCoalPlot()
    {
        if (coalMine == null || corporationManager == null || coalMine.hasPlotPurchased) return;
        if (corporationManager.cash >= coalPlotCost) { corporationManager.cash -= coalPlotCost; coalMine.PurchasePlot(coalPlotCost, coalDepositAmount); RefreshHUD(); }
    }

    private void HandleBuyFactory()
    {
        if (microchipFactory == null || corporationManager == null || microchipFactory.isBuilt) return;
        if (corporationManager.cash >= factoryCost) { corporationManager.cash -= factoryCost; microchipFactory.BuildFactory(); RefreshHUD(); }
    }
}