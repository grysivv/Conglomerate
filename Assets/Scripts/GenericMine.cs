// GenericMine.cs
using UnityEngine;

public class GenericMine : ProductionBuilding
{
    [Header("Ustawienia Działki Kopalni")]
    public bool hasPlotPurchased = false;

    protected override void Awake()
    {
        // Kopalnia jest ekstraktorem zasobów
        isExtractor = true;
        base.Awake();
    }

    protected override void HandleHourlyTick()
    {
        // Jeśli gracz nie kupił działki, kopalnia nic nie robi
        if (!hasPlotPurchased) return;

        base.HandleHourlyTick();
    }

    public void PurchasePlot(double cost, int initialDeposit)
    {
        if (hasPlotPurchased) return;

        if (corporationManager != null)
            corporationManager.cash -= cost;

        hasPlotPurchased = true;
        isBuilt = true;
        remainingDeposit = initialDeposit;
    }
}