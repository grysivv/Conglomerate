// FleetManager.cs
using UnityEngine;

public class FleetManager : MonoBehaviour
{
    [Header("Powiązania systemowe")]
    public TimeManager timeManager;
    public CorporationManager corporationManager;
    public GlobalInventoryManager globalInventory;
    public MarketManager marketManager;

    [Header("Ustawienia Logistyki")]
    public float transportDurationHours = 3.0f;
    public int truckCapacity = 20;
    public double fuelCostPerDelivery = 50.00;

    [HideInInspector] public bool isEnRoute = false;
    [HideInInspector] public float currentJourneyProgress = 0f;

    private int currentLoad = 0;
    private ResourceType currentLoadedType;

    void OnEnable() { TimeManager.OnHourlyTick += CheckForDelivery; }
    void OnDisable() { TimeManager.OnHourlyTick -= CheckForDelivery; }

    void Update()
    {
        if (!isEnRoute || timeManager == null) return;

        float timeScale = timeManager.GetCurrentSpeed();
        if (timeScale <= 0) return;

        float realSecondsRequired = transportDurationHours * (1.0f / timeScale);
        currentJourneyProgress += Time.deltaTime / realSecondsRequired;

        if (currentJourneyProgress >= 1f)
        {
            if (marketManager != null)
            {
                marketManager.SellResourceFromDelivery(currentLoadedType, currentLoad);
            }

            currentLoad = 0;
            currentJourneyProgress = 0f;
            isEnRoute = false;
        }
    }

    private void CheckForDelivery()
    {
        if (isEnRoute || globalInventory == null || corporationManager == null) return;

        // Tablica priorytetów wysyłki: Najpierw drogie Chipy, potem Krzem, na końcu Tani Węgiel
        ResourceType[] deliveryPriority = new ResourceType[] { ResourceType.Microchip, ResourceType.Silicon, ResourceType.Coal };

        foreach (ResourceType type in deliveryPriority)
        {
            int availableStock = globalInventory.GetStock(type);

            // Bezpiecznik: Dla drogich procesorów wysyłamy ciężarówkę nawet po 1 sztukę, nie czekamy na 5!
            int minBatch = (type == ResourceType.Microchip) ? 1 : 5;

            if (availableStock >= minBatch)
            {
                int amountToLoad = Mathf.Min(truckCapacity, availableStock);

                if (globalInventory.RemoveResource(type, amountToLoad))
                {
                    currentLoadedType = type;
                    currentLoad = amountToLoad;
                    corporationManager.cash -= fuelCostPerDelivery;
                    isEnRoute = true;
                    currentJourneyProgress = 0f;
                    Debug.Log($"<b><color=#fbc02d>[LOGISTYKA]</color></b> Ciężarówka ruszyła! Dostawa priorytetowa: {currentLoad}x {currentLoadedType}.");
                    break;
                }
            }
        }
    }
}