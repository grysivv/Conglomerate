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
    }

    [Header("Powiązania systemowe")]
    public TimeManager timeManager;
    public CorporationManager corporationManager;

    [Header("Cennik Bazowy Surowców")]
    public List<ResourceBasePrice> basePrices = new List<ResourceBasePrice>()
    {
        new ResourceBasePrice { type = ResourceType.Silicon, basePricePerTon = 150.00 },
        new ResourceBasePrice { type = ResourceType.Coal, basePricePerTon = 45.00 },
        new ResourceBasePrice { type = ResourceType.Microchip, basePricePerTon = 1200.00 } // Drogi produkt końcowy
    };

    private Dictionary<ResourceType, double> currentPrices = new Dictionary<ResourceType, double>();

    private void Start()
    {
        CalculatePrices();
    }

    private void OnEnable() { TimeManager.OnHourlyTick += HandleHourlyMarket; }
    private void OnDisable() { TimeManager.OnHourlyTick -= HandleHourlyMarket; }

    private void HandleHourlyMarket() { CalculatePrices(); }

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

        Debug.Log($"<b><color=#2e7d32>[RYNEK]</color></b> Sprzedano {amount}t/szt {type} za <b>{totalEarnings:F2} USD</b>");
    }

    private void CalculatePrices()
    {
        if (timeManager == null) return;
        float demandMultiplier = timeManager.GetMarketDemandMultiplier();

        foreach (var priceData in basePrices)
        {
            currentPrices[priceData.type] = priceData.basePricePerTon * demandMultiplier;
        }
    }
}