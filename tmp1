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
        new ResourceBasePrice { type = ResourceType.Microchip, basePricePerTon = 1200.00 },
        new ResourceBasePrice { type = ResourceType.Fuel, basePricePerTon = 80.00 }
    };

    [Header("Ustawienia Rynku Dynamicznego")]
    public float saturationDecayRatePerHour = 0.05f; // Nasycenie spada o 5% co godzinę

    private Dictionary<ResourceType, double> currentPrices = new Dictionary<ResourceType, double>();

    // Nasycenie rynku: 1.0 oznacza brak nasycenia (normalna cena).
    // Wyższe nasycenie obniża cenę.
    // Mniejsze nasycenie podwyższa cenę.
    private Dictionary<ResourceType, float> marketSaturation = new Dictionary<ResourceType, float>();

    private void Start()
    {
        foreach (var priceData in basePrices)
        {
            marketSaturation[priceData.type] = 1.0f;
        }
        CalculatePrices();
    }

    private void OnEnable() { TimeManager.OnHourlyTick += HandleHourlyMarket; }
    private void OnDisable() { TimeManager.OnHourlyTick -= HandleHourlyMarket; }

    private void HandleHourlyMarket()
    {
        DecaySaturation();
        CalculatePrices();
    }

    private void DecaySaturation()
    {
        List<ResourceType> keys = new List<ResourceType>(marketSaturation.Keys);
        foreach(var key in keys)
        {
            // Powrót do 1.0 (równowagi)
            if (marketSaturation[key] > 1.0f)
            {
                marketSaturation[key] -= saturationDecayRatePerHour;
                if (marketSaturation[key] < 1.0f) marketSaturation[key] = 1.0f;
            }
            else if (marketSaturation[key] < 1.0f)
            {
                marketSaturation[key] += saturationDecayRatePerHour;
                if (marketSaturation[key] > 1.0f) marketSaturation[key] = 1.0f;
            }
        }
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

        // Sprzedaż zwiększa nasycenie rynku (zmniejsza popyt, obniża cenę)
        if (!marketSaturation.ContainsKey(type)) marketSaturation[type] = 1.0f;
        marketSaturation[type] += amount * 0.02f; // np. 10 sztuk zwiększa nasycenie o 0.2 (20%)

        Debug.Log($"<b><color=#2e7d32>[RYNEK]</color></b> Sprzedano {amount}t/szt {type} za <b>{totalEarnings:F2} USD</b>. Nowe nasycenie: {marketSaturation[type]:F2}");

        CalculatePrices(); // Aktualizacja cen po sprzedaży
    }

    public void BuyResourceFromMarket(ResourceType type, int amount, out double cost, bool isPlayer = true)
    {
        CalculatePrices();
        double price = GetCurrentPrice(type);
        cost = amount * price;

        if (isPlayer && corporationManager != null) corporationManager.cash -= cost;

        // Kupno zmniejsza nasycenie (zwiększa popyt, podnosi cenę)
        if (!marketSaturation.ContainsKey(type)) marketSaturation[type] = 1.0f;
        marketSaturation[type] -= amount * 0.02f;
        if (marketSaturation[type] < 0.1f) marketSaturation[type] = 0.1f; // max 10x multiplier approximately

        if (isPlayer)
        {
            Debug.Log($"<b><color=#2e7d32>[RYNEK]</color></b> Gracz kupił {amount}t/szt {type} za <b>{cost:F2} USD</b>. Nowe nasycenie: {marketSaturation[type]:F2}");
        }
        else
        {
            Debug.Log($"<b><color=#2e7d32>[RYNEK]</color></b> Inny podmiot kupił {amount}t/szt {type}. Nowe nasycenie: {marketSaturation[type]:F2}");
        }

        CalculatePrices();
    }

    // Dodana metoda dla NPC, żeby nie modyfikować kasy gracza
    public void NPCSellResource(object[] args)
    {
        ResourceType type = (ResourceType)args[0];
        int amount = (int)args[1];

        if (!marketSaturation.ContainsKey(type)) marketSaturation[type] = 1.0f;
        marketSaturation[type] += amount * 0.02f;
        CalculatePrices();
    }

    private void CalculatePrices()
    {
        if (timeManager == null) return;
        float demandMultiplier = timeManager.GetMarketDemandMultiplier();

        foreach (var priceData in basePrices)
        {
            if (!marketSaturation.ContainsKey(priceData.type)) marketSaturation[priceData.type] = 1.0f;
            float saturation = marketSaturation[priceData.type];

            // Cena to (Baza / Nasycenie) * Pora Dnia
            currentPrices[priceData.type] = (priceData.basePricePerTon / saturation) * demandMultiplier;
        }
    }
}