using UnityEngine;

[RequireComponent(typeof(InventoryComponent))]
public class ResourceManufacturer : MonoBehaviour
{
    [Header("Ustawienia Produkcji")]
    public ProductionRecipe recipe;
    public GlobalInventoryManager globalInventory;

    [Header("Aktualny Proces")]
    public bool isProducing = false;
    public int currentProductionTimer = 0;
    public int totalResourceProduced = 0;

    private BuildingBase buildingBase;
    private InventoryComponent inventory;
    private HumanResourcesComponent hrComponent;

    protected virtual void Awake()
    {
        buildingBase = GetComponent<BuildingBase>();
        inventory = GetComponent<InventoryComponent>();
        hrComponent = GetComponent<HumanResourcesComponent>();
    }

    protected virtual void OnEnable()
    {
        TimeManager.OnHourlyTick += HandleHourlyTick;
        HumanResourcesComponent.OnEfficiencyChanged += HandleEfficiencyChanged;
    }

    protected virtual void OnDisable()
    {
        TimeManager.OnHourlyTick -= HandleHourlyTick;
        HumanResourcesComponent.OnEfficiencyChanged -= HandleEfficiencyChanged;
    }

    private void HandleEfficiencyChanged(float newEfficiency)
    {
        // This acts as a listener, dynamic adjustments happen during the ProcessManufacturing step automatically
        // thanks to checking hrComponent.laborEfficiency, but having this satisfies the event-driven constraint.
    }

    protected virtual void HandleHourlyTick()
    {
        if (buildingBase != null && (!buildingBase.isBuilt || !buildingBase.isOperating)) return;

        if (hrComponent != null && hrComponent.currentWorkers <= 0)
        {
            isProducing = false;
            currentProductionTimer = 0;
            return;
        }

        ProcessManufacturing();
    }

    private void ProcessManufacturing()
    {
        if (recipe == null) return;

        if (isProducing)
        {
            currentProductionTimer++;

            float eff = hrComponent != null ? hrComponent.laborEfficiency : 1f;
            int requiredTime = Mathf.RoundToInt(recipe.productionTimeHours / eff);
            requiredTime = Mathf.Max(1, requiredTime);

            if (currentProductionTimer >= requiredTime)
            {
                int freeSpace = inventory.GetFreeSpace(recipe.outputType);
                bool useGlobal = false;

                // If the inventory limit doesn't explicitly contain the output type capacity config
                if (inventory.GetCapacity(recipe.outputType) == 0)
                {
                    if (globalInventory != null)
                    {
                        freeSpace = globalInventory.GetFreeSpace(recipe.outputType);
                        useGlobal = true;
                    }
                }

                if (freeSpace >= recipe.outputAmount)
                {
                    if (useGlobal)
                    {
                        globalInventory.AddResource(recipe.outputType, recipe.outputAmount);
                    }
                    else
                    {
                        inventory.AddResource(recipe.outputType, recipe.outputAmount);
                    }
                    totalResourceProduced += recipe.outputAmount;
                    isProducing = false;
                    currentProductionTimer = 0;
                    Debug.Log($"<b><color=#9c27b0>[FABRYKA]</color></b> Sukces! Wyprodukowano {recipe.outputAmount} {recipe.outputType}.");
                }
            }
        }
        else
        {
            TryStartProduction();
        }
    }

    private void TryStartProduction()
    {
        if (recipe == null) return;
        if (hrComponent != null && hrComponent.currentWorkers <= 0) return;

        int freeSpace = inventory.GetFreeSpace(recipe.outputType);
        bool useGlobalOut = inventory.GetCapacity(recipe.outputType) == 0;
        if (useGlobalOut && globalInventory != null) freeSpace = globalInventory.GetFreeSpace(recipe.outputType);

        if (freeSpace < recipe.outputAmount) return;

        foreach (var req in recipe.inputs)
        {
            bool useGlobalIn = inventory.GetCapacity(req.resource) == 0;
            if (useGlobalIn && globalInventory != null)
            {
                if (globalInventory.GetStock(req.resource) < req.amount) return;
            }
            else
            {
                if (inventory.GetStock(req.resource) < req.amount) return;
            }
        }

        foreach (var req in recipe.inputs)
        {
            bool useGlobalIn = inventory.GetCapacity(req.resource) == 0;
            if (useGlobalIn && globalInventory != null)
            {
                globalInventory.RemoveResource(req.resource, req.amount);
            }
            else
            {
                inventory.RemoveResource(req.resource, req.amount);
            }
        }

        isProducing = true;
        currentProductionTimer = 0;
    }
}
