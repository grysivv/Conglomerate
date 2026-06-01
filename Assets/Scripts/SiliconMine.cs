// SiliconMine.cs
using UnityEngine;

public class SiliconMine : MonoBehaviour
{
    [Header("Menedżerowie")]
    public TimeManager timeManager;
    public CorporationManager corporationManager;
    public GlobalInventoryManager globalInventory;

    [Header("Typ Surowca")]
    public ResourceType resourceType = ResourceType.Silicon; // Możesz zmienić w Inspektorze na Coal!

    [Header("Stan Działki i Złoża")]
    public bool hasPlotPurchased = false;
    public int remainingDeposit = 0;

    [Header("Stan Operacyjny")]
    public bool isOperating = true;
    public int totalResourceProduced = 0;

    [Header("Koszty i produkcja")]
    public double baseHourlyEnergyConsumption = 40.00;
    public double hourlyLaborCost = 200.00;
    public int baseHourlyProduction = 5;

    void OnEnable() { TimeManager.OnHourlyTick += HandleHourlyTick; }
    void OnDisable() { TimeManager.OnHourlyTick -= HandleHourlyTick; }

    private void HandleHourlyTick()
    {
        if (!hasPlotPurchased || remainingDeposit <= 0 || !isOperating) return;
        if (timeManager == null || corporationManager == null || globalInventory == null) return;

        // BEZPIECZNIK: Sprawdź czy magazyn docelowy nie jest pełny
        if (globalInventory.GetFreeSpace(resourceType) <= 0)
        {
            Debug.Log($"<b><color=#ef5350>[KOPALNIA]</color></b> Produkcja wstrzymana! Magazyn dla {resourceType} jest PEŁNY.");
            return;
        }

        float energyMultiplier = timeManager.GetEnergyCostMultiplier();
        double currentHourEnergyCost = baseHourlyEnergyConsumption * energyMultiplier;
        double totalHourlyCost = currentHourEnergyCost + hourlyLaborCost;

        corporationManager.cash -= totalHourlyCost;

        // Uwzględniamy zarówno stan złoża, jak i wolne miejsce w magazynie
        int maxPossibleProd = Mathf.Min(baseHourlyProduction, remainingDeposit);
        int actualProduction = Mathf.Min(maxPossibleProd, globalInventory.GetFreeSpace(resourceType));

        if (actualProduction <= 0) return;

        remainingDeposit -= actualProduction;
        globalInventory.AddResource(resourceType, actualProduction);
        totalResourceProduced += actualProduction;

        Debug.Log($"<b><color=#00acc1>[KOPALNIA]</color></b> Wydobyto: +{actualProduction}t {resourceType}. Pozostałe złoże: {remainingDeposit}t.");
    }

    public void PurchasePlot(double cost, int initialDeposit)
    {
        if (hasPlotPurchased) return;

        hasPlotPurchased = true;
        remainingDeposit = initialDeposit;
    }
}