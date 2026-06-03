// MarketManager.cs
using UnityEngine;
using System.Collections.Generic;

public class MarketManager : MonoBehaviour
{
    [System.Serializable]
    public class ResourceBasePrice
    {
        public ResourceType type;
        public double basePricePerTon;
        public int marketCapacity;
    }

    [Header("Powiązania systemowe")]
    public TimeManager timeManager;
    public CorporationManager corporationManager;

    [Header("Cennik Bazowy Surowców")]
    public List<ResourceBasePrice> basePrices = new List<ResourceBasePrice>()
    {
        new ResourceBasePrice { type = ResourceType.Silicon, basePricePerTon = 150.00, marketCapacity = 1000 },
        new ResourceBasePrice { type = ResourceType.Coal, basePricePerTon = 45.00, marketCapacity = 2000 },
        new ResourceBasePrice { type = ResourceType.Microchip, basePricePerTon = 1200.00, marketCapacity = 500 },
        new ResourceBasePrice { type = ResourceType.Fuel, basePricePerTon = 80.00, marketCapacity = 1500 }
    };

    private Dictionary<ResourceType, double> currentPrices = new Dictionary<ResourceType, double>();

    private Dictionary<ResourceType, int> dailySupply = new Dictionary<ResourceType, int>();

    private void Start()
    {
        CalculatePrices();
    }

    private void OnEnable() { TimeManager.OnHourlyTick += HandleHourlyMarket; }
    private void OnDisable() { TimeManager.OnHourlyTick -= HandleHourlyMarket; }

    private void HandleHourlyMarket()
    {
        if (timeManager != null && timeManager.currentHour == 0)
        {
            foreach (var priceData in basePrices)
            {
                dailySupply[priceData.type] = 0;
            }
        }
        CalculatePrices();
    }

    public double GetCurrentPrice(ResourceType type)
    {
        return currentPrices.ContainsKey(type) ? currentPrices[type] : 0.0;
    }

    public void SellResourceFromDelivery(ResourceType type, int amount)
    {
        if (corporationManager == null) return;

        CalculatePrices();
        double price = GetCurrentPrice(type);
        double totalEarnings = amount * price;
        corporationManager.cash += totalEarnings;

        if (!dailySupply.ContainsKey(type)) dailySupply[type] = 0;
        dailySupply[type] += amount;

        Debug.Log($"<b><color=#2e7d32>[RYNEK]</color></b> Sprzedano {amount}t/szt {type} za <b>{totalEarnings:F2} USD</b>. Nowe nasycenie: {GetSaturationPercentage(type) * 100f:F1}%");

        CalculatePrices(); // Aktualizacja cen po sprzedaży
    }

    public void BuyResourceFromMarket(ResourceType type, int amount, out double cost, bool isPlayer = true)
    {
        CalculatePrices();
        double price = GetCurrentPrice(type);
        cost = amount * price;

        if (isPlayer && corporationManager != null) corporationManager.cash -= cost;

        if (!dailySupply.ContainsKey(type)) dailySupply[type] = 0;
        dailySupply[type] -= amount;
        if (dailySupply[type] < 0) dailySupply[type] = 0;

        if (isPlayer)
        {
            Debug.Log($"<b><color=#2e7d32>[RYNEK]</color></b> Gracz kupił {amount}t/szt {type} za <b>{cost:F2} USD</b>. Nowe nasycenie: {GetSaturationPercentage(type) * 100f:F1}%");
        }
        else
        {
            Debug.Log($"<b><color=#2e7d32>[RYNEK]</color></b> Inny podmiot kupił {amount}t/szt {type}. Nowe nasycenie: {GetSaturationPercentage(type) * 100f:F1}%");
        }

        CalculatePrices();
    }

    // Dodana metoda dla NPC, żeby nie modyfikować kasy gracza
    public void NPCSellResource(ResourceType type, int amount)
    {
        if (!dailySupply.ContainsKey(type)) dailySupply[type] = 0;
        dailySupply[type] += amount;
        CalculatePrices();
    }

    public float GetSaturationPercentage(ResourceType type)
    {
        int supply = dailySupply.ContainsKey(type) ? dailySupply[type] : 0;
        int capacity = 1;
        foreach (var priceData in basePrices)
        {
            if (priceData.type == type)
            {
                capacity = priceData.marketCapacity > 0 ? priceData.marketCapacity : 1;
                break;
            }
        }
        return (float)supply / capacity;
    }

    private void CalculatePrices()
    {
        if (timeManager == null) return;
        float demandMultiplier = timeManager.GetMarketDemandMultiplier();

        foreach (var priceData in basePrices)
        {
            float saturation = GetSaturationPercentage(priceData.type);

            currentPrices[priceData.type] = priceData.basePricePerTon * demandMultiplier * (1.0f - Mathf.Min(0.3f, saturation * 0.3f));
        }
    }
}