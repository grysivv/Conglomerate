// MarketManager.cs
using UnityEngine;

public class MarketManager : MonoBehaviour
{
    [Header("Menedżerowie")]
    public TimeManager timeManager;
    public CorporationManager corporationManager;
    public SiliconMine siliconMine;

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
        if (timeManager == null || corporationManager == null || siliconMine == null) return;

        // Przeliczenie ceny
        float demandMultiplier = timeManager.GetMarketDemandMultiplier();
        currentSiliconPrice = baseSiliconPricePerTon * demandMultiplier;

        // Pętla finansowa: skupowanie krzemu z kopalni
        if (siliconMine.siliconStorage > 0)
        {
            int tonsSold = siliconMine.siliconStorage;
            double profit = tonsSold * currentSiliconPrice;

            siliconMine.siliconStorage = 0; // Zerujemy magazyn
            corporationManager.cash += profit; // Dodajemy gotówkę

            Debug.Log($"<b>[Rynek Zbytu]</b> Sprzedano {tonsSold}t krzemu za {profit:F2} USD (Cena za tonę: {currentSiliconPrice:F2} USD).");
        }
    }
}
