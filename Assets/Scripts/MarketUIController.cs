using UnityEngine;
using UnityEngine.UIElements;

public class MarketUIController
{
    private VisualElement root;
    private UIManager uiManager;

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

    public MarketUIController(VisualElement root, UIManager uiManager)
    {
        this.root = root;
        this.uiManager = uiManager;
    }

    public void Initialize()
    {
        siliconLabel = root.Q<Label>("SiliconLabel");
        sellSiliconBtn = root.Q<Button>("SellSiliconBtn");
        if (sellSiliconBtn != null) sellSiliconBtn.RegisterCallback<ClickEvent>(OnSellSiliconBtnClicked);

        coalLabel = root.Q<Label>("CoalLabel");
        sellCoalBtn = root.Q<Button>("SellCoalBtn");
        if (sellCoalBtn != null) sellCoalBtn.RegisterCallback<ClickEvent>(OnSellCoalBtnClicked);

        microchipLabel = root.Q<Label>("MicrochipLabel");
        sellChipBtn = root.Q<Button>("SellChipBtn");
        if (sellChipBtn != null) sellChipBtn.RegisterCallback<ClickEvent>(OnSellChipBtnClicked);

        siliconMarketLabel = root.Q<Label>("SiliconMarketLabel");
        coalMarketLabel = root.Q<Label>("CoalMarketLabel");
        microchipMarketLabel = root.Q<Label>("MicrochipMarketLabel");

        routeMineToGlobalSiliconBtn = root.Q<Button>("RouteMineToGlobalSiliconBtn");
        if (routeMineToGlobalSiliconBtn != null) routeMineToGlobalSiliconBtn.RegisterCallback<ClickEvent>(OnRouteMineToGlobalSiliconBtnClicked);

        routeMineToFactorySiliconBtn = root.Q<Button>("RouteMineToFactorySiliconBtn");
        if (routeMineToFactorySiliconBtn != null) routeMineToFactorySiliconBtn.RegisterCallback<ClickEvent>(OnRouteMineToFactorySiliconBtnClicked);

        routeMineToGlobalCoalBtn = root.Q<Button>("RouteMineToGlobalCoalBtn");
        if (routeMineToGlobalCoalBtn != null) routeMineToGlobalCoalBtn.RegisterCallback<ClickEvent>(OnRouteMineToGlobalCoalBtnClicked);

        routeGlobalToFactorySiliconBtn = root.Q<Button>("RouteGlobalToFactorySiliconBtn");
        if (routeGlobalToFactorySiliconBtn != null) routeGlobalToFactorySiliconBtn.RegisterCallback<ClickEvent>(OnRouteGlobalToFactorySiliconBtnClicked);

        routeGlobalToFactoryCoalBtn = root.Q<Button>("RouteGlobalToFactoryCoalBtn");
        if (routeGlobalToFactoryCoalBtn != null) routeGlobalToFactoryCoalBtn.RegisterCallback<ClickEvent>(OnRouteGlobalToFactoryCoalBtnClicked);

        routeFactoryToGlobalChipBtn = root.Q<Button>("RouteFactoryToGlobalChipBtn");
        if (routeFactoryToGlobalChipBtn != null) routeFactoryToGlobalChipBtn.RegisterCallback<ClickEvent>(OnRouteFactoryToGlobalChipBtnClicked);

        activeRoutesLabel = root.Q<Label>("ActiveRoutesLabel");
    }

    public void Dispose()
    {
        if (sellSiliconBtn != null) sellSiliconBtn.UnregisterCallback<ClickEvent>(OnSellSiliconBtnClicked);
        if (sellCoalBtn != null) sellCoalBtn.UnregisterCallback<ClickEvent>(OnSellCoalBtnClicked);
        if (sellChipBtn != null) sellChipBtn.UnregisterCallback<ClickEvent>(OnSellChipBtnClicked);

        if (routeMineToGlobalSiliconBtn != null) routeMineToGlobalSiliconBtn.UnregisterCallback<ClickEvent>(OnRouteMineToGlobalSiliconBtnClicked);
        if (routeMineToFactorySiliconBtn != null) routeMineToFactorySiliconBtn.UnregisterCallback<ClickEvent>(OnRouteMineToFactorySiliconBtnClicked);
        if (routeMineToGlobalCoalBtn != null) routeMineToGlobalCoalBtn.UnregisterCallback<ClickEvent>(OnRouteMineToGlobalCoalBtnClicked);
        if (routeGlobalToFactorySiliconBtn != null) routeGlobalToFactorySiliconBtn.UnregisterCallback<ClickEvent>(OnRouteGlobalToFactorySiliconBtnClicked);
        if (routeGlobalToFactoryCoalBtn != null) routeGlobalToFactoryCoalBtn.UnregisterCallback<ClickEvent>(OnRouteGlobalToFactoryCoalBtnClicked);
        if (routeFactoryToGlobalChipBtn != null) routeFactoryToGlobalChipBtn.UnregisterCallback<ClickEvent>(OnRouteFactoryToGlobalChipBtnClicked);
    }

    private void OnSellSiliconBtnClicked(ClickEvent ev) => uiManager.SellInstant(ResourceType.Silicon, 10);
    private void OnSellCoalBtnClicked(ClickEvent ev) => uiManager.SellInstant(ResourceType.Coal, 20);
    private void OnSellChipBtnClicked(ClickEvent ev) => uiManager.SellInstant(ResourceType.Microchip, 1);

    private void OnRouteMineToGlobalSiliconBtnClicked(ClickEvent ev) => uiManager.AddSpecificRoute(uiManager.siliconMine?.GetComponent<BuildingBase>(), DestinationType.GlobalInventory, null, ResourceType.Silicon);
    private void OnRouteMineToFactorySiliconBtnClicked(ClickEvent ev) => uiManager.AddSpecificRoute(uiManager.siliconMine?.GetComponent<BuildingBase>(), DestinationType.Factory, uiManager.factoryBase, ResourceType.Silicon);
    private void OnRouteMineToGlobalCoalBtnClicked(ClickEvent ev) => uiManager.AddSpecificRoute(uiManager.coalMine?.GetComponent<BuildingBase>(), DestinationType.GlobalInventory, null, ResourceType.Coal);
    private void OnRouteGlobalToFactorySiliconBtnClicked(ClickEvent ev) => uiManager.AddSpecificRoute(null, DestinationType.Factory, uiManager.factoryBase, ResourceType.Silicon);
    private void OnRouteGlobalToFactoryCoalBtnClicked(ClickEvent ev) => uiManager.AddSpecificRoute(null, DestinationType.Factory, uiManager.factoryBase, ResourceType.Coal);
    private void OnRouteFactoryToGlobalChipBtnClicked(ClickEvent ev) => uiManager.AddSpecificRoute(uiManager.factoryBase, DestinationType.GlobalInventory, null, ResourceType.Microchip);

    public void RefreshHUD()
    {
        if (uiManager.globalInventory != null)
        {
            if (siliconLabel != null) siliconLabel.text = $"Silos Krzemu: {uiManager.globalInventory.GetStock(ResourceType.Silicon)} / {uiManager.globalInventory.GetCapacity(ResourceType.Silicon)} t";
            if (coalLabel != null) coalLabel.text = $"Hałda Węgla: {uiManager.globalInventory.GetStock(ResourceType.Coal)} / {uiManager.globalInventory.GetCapacity(ResourceType.Coal)} t";
            if (microchipLabel != null) microchipLabel.text = $"Magazyn Procesorów: {uiManager.globalInventory.GetStock(ResourceType.Microchip)} / {uiManager.globalInventory.GetCapacity(ResourceType.Microchip)} szt.";
        }

        if (uiManager.marketManager != null)
        {
            if (siliconMarketLabel != null) siliconMarketLabel.text = $"Cena Krzemu: {uiManager.marketManager.GetCurrentPrice(ResourceType.Silicon):F2} USD";
            if (coalMarketLabel != null) coalMarketLabel.text = $"Cena Węgla: {uiManager.marketManager.GetCurrentPrice(ResourceType.Coal):F2} USD";
            if (microchipMarketLabel != null) microchipMarketLabel.text = $"Cena Procesorów: {uiManager.marketManager.GetCurrentPrice(ResourceType.Microchip):F2} USD";
        }

        if (activeRoutesLabel != null && uiManager.fleetManager != null && uiManager.fleetManager.activeRoutes != null)
        {
            activeRoutesLabel.text = $"Zlecone trasy (Aktywne ciężarówki): {uiManager.fleetManager.activeRoutes.Count}";
        }
    }
}
