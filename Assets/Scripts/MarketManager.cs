using UnityEngine;

public class MarketManager : MonoBehaviour
{
    [Header("Menedżerowie")]
    public TimeManager timeManager;
    public CorporationManager corporationManager;

    [Header("Ustawienia Rynku")]
    public double baseSiliconPricePerTon = 150.00;

    public double currentSiliconPrice { get; private set; }

    void OnEnable()
    {
        TimeManager.OnHourlyTick += HandleHourlyTick;
    }

    void OnDisable()
    {
        TimeManager.OnHourlyTick -= HandleHourlyTick;
    }

    private void HandleHourlyTick()
    {
        if (timeManager == null || corporationManager == null) return;

        // Przeliczenie ceny
        float demandMultiplier = timeManager.GetMarketDemandMultiplier();
        currentSiliconPrice = baseSiliconPricePerTon * demandMultiplier;
    }

    public void SellSiliconFromDelivery(int amount)
    {
        if (amount > 0 && corporationManager != null)
        {
            double profit = amount * currentSiliconPrice;
            corporationManager.cash += profit;

            Debug.Log($"<b>[Rynek Zbytu]</b> Sprzedano z dostawy {amount}t krzemu za {profit:F2} USD (Cena za tonę: {currentSiliconPrice:F2} USD).");
        }
    }
}
