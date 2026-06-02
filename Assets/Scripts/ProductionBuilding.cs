using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ResourceRequirement
{
    public ResourceType resource;
    public int amount;
}

[System.Serializable]
public class LocalStorageLimit
{
    public ResourceType type;
    public int maxCapacity;
}

public class ProductionBuilding : MonoBehaviour
{
    [Header("Menedżerowie")]
    public TimeManager timeManager;
    public CorporationManager corporationManager;
    public GlobalInventoryManager globalInventory;

    [Header("Stan Infrastruktury")]
    public bool isBuilt = false;
    public bool isOperating = true;

    [Header("Ustawienia Złoża (Tylko Kopalnie)")]
    public bool isExtractor = false;
    public ResourceType extractionType;
    public int remainingDeposit = 0;

    [Header("Panel HR")]
    public bool usesHR = false;
    public int currentWorkers = 0;
    public int maxWorkers = 5;
    public double currentSalary = 300.00;
    public double expectedSalary = 300.00;
    public float laborEfficiency = 1f;

    [Header("Ustawienia Produkcji")]
    public ProductionRecipe recipe;
    public double baseHourlyMaintenanceCost = 200.00;

    [Header("Magazyn Lokalny (Local Inventory)")]
    public List<LocalStorageLimit> storageLimits = new List<LocalStorageLimit>();
    private Dictionary<ResourceType, int> localInventory = new Dictionary<ResourceType, int>();
    private Dictionary<ResourceType, int> localCapacities = new Dictionary<ResourceType, int>();

    [Header("Aktualny Proces")]
    public bool isProducing = false;
    public int currentProductionTimer = 0;
    public int totalResourceProduced = 0;

    protected virtual void Awake()
    {
        foreach (var limit in storageLimits)
        {
            localInventory[limit.type] = 0;
            localCapacities[limit.type] = limit.maxCapacity;
        }
    }

    protected virtual void OnEnable() { TimeManager.OnHourlyTick += HandleHourlyTick; }
    protected virtual void OnDisable() { TimeManager.OnHourlyTick -= HandleHourlyTick; }

    public int GetLocalStock(ResourceType type)
    {
        return localInventory.ContainsKey(type) ? localInventory[type] : 0;
    }

    public int GetLocalCapacity(ResourceType type)
    {
        return localCapacities.ContainsKey(type) ? localCapacities[type] : 0;
    }

    public int GetLocalFreeSpace(ResourceType type)
    {
        if (!localInventory.ContainsKey(type)) return 0;
        return localCapacities[type] - localInventory[type];
    }

    public void AddLocalResource(ResourceType type, int amount)
    {
        if (!localInventory.ContainsKey(type)) return;
        localInventory[type] = Mathf.Min(localInventory[type] + amount, localCapacities[type]);
    }

    public bool RemoveLocalResource(ResourceType type, int amount)
    {
        if (!localInventory.ContainsKey(type) || localInventory[type] < amount) return false;
        localInventory[type] -= amount;
        return true;
    }

    protected virtual void HandleHourlyTick()
    {
        if (!isBuilt || !isOperating) return;
        if (timeManager == null || corporationManager == null) return;

        if (usesHR)
        {
            UpdateHRMechanics();
        }

        double currentHourCost = baseHourlyMaintenanceCost * timeManager.GetEnergyCostMultiplier();
        if (usesHR)
        {
            currentHourCost += currentWorkers * currentSalary;
        }

        corporationManager.cash -= currentHourCost;

        if (usesHR && currentWorkers <= 0)
        {
            isProducing = false;
            currentProductionTimer = 0;
            return;
        }

        if (isExtractor)
        {
            ProcessExtraction();
        }
        else
        {
            ProcessManufacturing();
        }
    }

    private void UpdateHRMechanics()
    {
        if (currentWorkers <= 0) { laborEfficiency = 0f; return; }

        float wageRatio = (float)(currentSalary / expectedSalary);
        laborEfficiency = Mathf.Clamp(wageRatio, 0.5f, 1.5f);

        if (wageRatio < 0.9f && Random.value > 0.4f && currentWorkers > 0)
        {
            currentWorkers--;
            Debug.Log("<b><color=#ef5350>[HR]</color></b> Pracownik odszedł z fabryki z powodu zbyt niskiej płacy!");
        }
    }

    private void ProcessExtraction()
    {
        if (remainingDeposit <= 0) return;

        int outAmt = recipe != null ? recipe.outputAmount : 1; // Fallback to 1 if recipe missing

        int freeSpace = GetLocalFreeSpace(extractionType);
        bool useGlobal = false;
        if (!localInventory.ContainsKey(extractionType))
        {
            if (globalInventory != null)
            {
                freeSpace = globalInventory.GetFreeSpace(extractionType);
                useGlobal = true;
            }
        }

        if (freeSpace <= 0)
        {
            Debug.Log($"<b><color=#ef5350>[KOPALNIA]</color></b> Produkcja wstrzymana! Magazyn dla {extractionType} jest PEŁNY.");
            return;
        }

        int actualProduction = Mathf.Min(outAmt, remainingDeposit);
        actualProduction = Mathf.Min(actualProduction, freeSpace);

        if (actualProduction <= 0) return;

        remainingDeposit -= actualProduction;

        if (useGlobal)
        {
            globalInventory.AddResource(extractionType, actualProduction);
        }
        else
        {
            AddLocalResource(extractionType, actualProduction);
        }

        totalResourceProduced += actualProduction;

        Debug.Log($"<b><color=#00acc1>[KOPALNIA]</color></b> Wydobyto: +{actualProduction}t {extractionType}. Pozostałe złoże: {remainingDeposit}t.");
    }

    private void ProcessManufacturing()
    {
        if (recipe == null) return;

        if (isProducing)
        {
            currentProductionTimer++;

            float eff = usesHR ? laborEfficiency : 1f;
            int requiredTime = Mathf.RoundToInt(recipe.productionTimeHours / eff);
            requiredTime = Mathf.Max(1, requiredTime);

            if (currentProductionTimer >= requiredTime)
            {
                int freeSpace = GetLocalFreeSpace(recipe.outputType);
                bool useGlobal = false;
                if (!localInventory.ContainsKey(recipe.outputType))
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
                        AddLocalResource(recipe.outputType, recipe.outputAmount);
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
        if (usesHR && currentWorkers <= 0) return;

        int freeSpace = GetLocalFreeSpace(recipe.outputType);
        bool useGlobalOut = !localInventory.ContainsKey(recipe.outputType);
        if (useGlobalOut && globalInventory != null) freeSpace = globalInventory.GetFreeSpace(recipe.outputType);

        if (freeSpace < recipe.outputAmount) return;

        foreach (var req in recipe.inputs)
        {
            bool useGlobalIn = !localInventory.ContainsKey(req.resource);
            if (useGlobalIn && globalInventory != null)
            {
                if (globalInventory.GetStock(req.resource) < req.amount) return;
            }
            else
            {
                if (GetLocalStock(req.resource) < req.amount) return;
            }
        }

        foreach (var req in recipe.inputs)
        {
            bool useGlobalIn = !localInventory.ContainsKey(req.resource);
            if (useGlobalIn && globalInventory != null)
            {
                globalInventory.RemoveResource(req.resource, req.amount);
            }
            else
            {
                RemoveLocalResource(req.resource, req.amount);
            }
        }

        isProducing = true;
        currentProductionTimer = 0;
    }

    public void HireWorker()
    {
        if (usesHR && currentWorkers < maxWorkers) currentWorkers++;
    }

    public void FireWorker()
    {
        if (usesHR && currentWorkers > 0) currentWorkers--;
    }

    public void AdjustWage(double amount)
    {
        if (usesHR)
        {
            currentSalary = System.Math.Max(50.00, currentSalary + amount);
        }
    }
}
