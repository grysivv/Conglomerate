using UnityEngine;

[RequireComponent(typeof(InventoryComponent))]
public class ResourceExtractor : MonoBehaviour
{
    [Header("Ustawienia Złoża")]
    public ResourceType extractionType;
    public int remainingDeposit = 0;
    public int baseExtractionAmount = 5;

    public bool hasPlotPurchased = false;

    private BuildingBase buildingBase;
    private InventoryComponent inventory;

    // Total resource produced tracker for consistency with old behavior if needed
    public int totalResourceProduced = 0;

    protected virtual void Awake()
    {
        buildingBase = GetComponent<BuildingBase>();
        inventory = GetComponent<InventoryComponent>();

        // Ensure the extraction type is tracked in inventory
        if (inventory.GetCapacity(extractionType) == 0)
        {
            inventory.SetCapacity(extractionType, 500); // domyślny bufor kopalni
        }
    }

    protected virtual void OnEnable()
    {
        TimeManager.OnHourlyTick += HandleHourlyTick;
    }

    protected virtual void OnDisable()
    {
        TimeManager.OnHourlyTick -= HandleHourlyTick;
    }

    protected virtual void HandleHourlyTick()
    {
        if (buildingBase != null && (!buildingBase.isBuilt || !buildingBase.isOperating)) return;
        if (!hasPlotPurchased) return;

        ProcessExtraction();
    }

    private void ProcessExtraction()
    {
        if (remainingDeposit <= 0) return;

        int outAmt = baseExtractionAmount;

        int freeSpace = inventory.GetFreeSpace(extractionType);

        if (freeSpace <= 0)
        {
            Debug.Log($"<b><color=#ef5350>[KOPALNIA]</color></b> Produkcja wstrzymana! Lokalny magazyn kopalni dla {extractionType} jest PEŁNY.");
            return;
        }

        int actualProduction = Mathf.Min(outAmt, remainingDeposit);
        actualProduction = Mathf.Min(actualProduction, freeSpace);

        if (actualProduction <= 0) return;

        remainingDeposit -= actualProduction;

        inventory.AddResource(extractionType, actualProduction);
        totalResourceProduced += actualProduction;

        Debug.Log($"<b><color=#00acc1>[KOPALNIA]</color></b> Wydobyto: +{actualProduction}t {extractionType}. Pozostałe złoże: {remainingDeposit}t.");
    }
}
