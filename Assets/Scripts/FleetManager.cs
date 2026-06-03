using UnityEngine;
using System.Collections.Generic;

public class FleetManager : MonoBehaviour
{
    [Header("Powiązania systemowe")]
    public TimeManager timeManager;
    public CorporationManager corporationManager;
    public GlobalInventoryManager globalInventory;
    public MarketManager marketManager;

    [Header("Trasy Transportowe")]
    public List<TransportRoute> activeRoutes = new List<TransportRoute>();

    // For backwards compatibility and tests, keeping these as defaults or optional fallbacks
    [Header("Ustawienia Domyślne")]
    public float defaultTransportDurationHours = 3.0f;
    public int defaultTruckCapacity = 20;
    public double defaultFuelCostPerDelivery = 50.00;

    void OnEnable() { TimeManager.OnHourlyTick += CheckForDelivery; }
    void OnDisable() { TimeManager.OnHourlyTick -= CheckForDelivery; }

    void Update()
    {
        if (timeManager == null) return;
        float timeScale = timeManager.GetCurrentSpeed();
        if (timeScale <= 0) return;

        if (activeRoutes == null) return;

        foreach (var route in activeRoutes)
        {
            if (route == null || !route.isEnRoute) continue;

            float realSecondsRequired = route.transportDurationHours * (1.0f / timeScale);
            route.currentJourneyProgress += Time.deltaTime / realSecondsRequired;

            if (route.currentJourneyProgress >= 1f)
            {
                DeliverCargo(route);
            }
        }
    }

    private void DeliverCargo(TransportRoute route)
    {
        if (route == null) return;

        if (route.destinationType == DestinationType.GlobalInventory && globalInventory != null)
        {
            globalInventory.AddResource(route.resourceType, route.currentLoad);
        }
        else if (route.destinationType == DestinationType.Factory && route.destinationBuilding != null)
        {
            route.destinationBuilding.AddLocalResource(route.resourceType, route.currentLoad);
        }

        Debug.Log($"<b><color=#fbc02d>[LOGISTYKA]</color></b> Dostawa ukończona! {route.currentLoad}x {route.resourceType} do {route.destinationType}.");

        route.currentLoad = 0;
        route.currentJourneyProgress = 0f;
        route.isEnRoute = false;
    }

    private void CheckForDelivery()
    {
        if (corporationManager == null) return;
        if (activeRoutes == null) return;

        foreach (var route in activeRoutes)
        {
            if (route == null || route.isEnRoute) continue;

            int availableStock = 0;

            if (route.sourceBuilding != null)
            {
                availableStock = route.sourceBuilding.GetLocalStock(route.resourceType);
            }
            else if (globalInventory != null)
            {
                availableStock = globalInventory.GetStock(route.resourceType);
            }

            int minBatch = (route.resourceType == ResourceType.Microchip) ? 1 : 5;

            if (availableStock >= minBatch)
            {
                // Check if destination has space
                if (route.destinationType == DestinationType.Factory)
                {
                    if (route.destinationBuilding == null || route.destinationBuilding.GetLocalFreeSpace(route.resourceType) <= 0) continue; // no space or missing building
                }
                else if (route.destinationType == DestinationType.GlobalInventory)
                {
                    if (globalInventory == null || globalInventory.GetFreeSpace(route.resourceType) <= 0) continue; // no space
                }

                int amountToLoad = Mathf.Min(route.batchSize, availableStock);

                bool loaded = false;
                if (route.sourceBuilding != null)
                {
                    loaded = route.sourceBuilding.RemoveLocalResource(route.resourceType, amountToLoad);
                }
                else if (globalInventory != null)
                {
                    loaded = globalInventory.RemoveResource(route.resourceType, amountToLoad);
                }

                if (loaded)
                {
                    route.currentLoad = amountToLoad;
                    corporationManager.cash -= route.fuelCostPerDelivery;
                    route.isEnRoute = true;
                    route.currentJourneyProgress = 0f;
                    Debug.Log($"<b><color=#fbc02d>[LOGISTYKA]</color></b> Ciężarówka ruszyła! Dostawa na trasie. Ładunek: {route.currentLoad}x {route.resourceType}.");
                }
            }
        }
    }
}
