using UnityEngine;

public class SiliconMine : ProductionBuilding
{
    [Header("Specyficzne dla Silicon Mine")]
    public bool hasPlotPurchased = false;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void HandleHourlyTick()
    {
        if (!hasPlotPurchased) return;
        base.HandleHourlyTick();
    }

    public void PurchasePlot(double cost, int initialDeposit)
    {
        if (hasPlotPurchased) return;

        // In a real scenario we'd deduct cost here
        if (corporationManager != null) corporationManager.cash -= cost;

        hasPlotPurchased = true;
        remainingDeposit = initialDeposit;
    }
}
