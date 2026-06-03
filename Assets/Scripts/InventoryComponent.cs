using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class StorageLimit
{
    public ResourceType type;
    public int maxCapacity;
}

public class InventoryComponent : MonoBehaviour
{
    [Header("Ustawienia Pojemności Magazynów")]
    public List<StorageLimit> storageLimits = new List<StorageLimit>();

    private Dictionary<ResourceType, int> stocks = new Dictionary<ResourceType, int>();
    private Dictionary<ResourceType, int> capacities = new Dictionary<ResourceType, int>();

    protected virtual void Awake()
    {
        InitializeInventory();
    }

    public void InitializeInventory()
    {
        stocks.Clear();
        capacities.Clear();
        foreach (var limit in storageLimits)
        {
            stocks[limit.type] = 0;
            capacities[limit.type] = limit.maxCapacity;
        }
    }

    public int GetStock(ResourceType type)
    {
        return stocks.ContainsKey(type) ? stocks[type] : 0;
    }

    public int GetCapacity(ResourceType type)
    {
        return capacities.ContainsKey(type) ? capacities[type] : 0;
    }

    public int GetFreeSpace(ResourceType type)
    {
        if (!stocks.ContainsKey(type)) return 0;
        return capacities[type] - stocks[type];
    }

    public void AddResource(ResourceType type, int amount)
    {
        if (!stocks.ContainsKey(type))
        {
            stocks[type] = 0;
            capacities[type] = int.MaxValue; // Fallback if limits not defined
        }
        stocks[type] = Mathf.Min(stocks[type] + amount, capacities[type]);
    }

    public bool RemoveResource(ResourceType type, int amount)
    {
        if (!stocks.ContainsKey(type) || stocks[type] < amount) return false;
        stocks[type] -= amount;
        return true;
    }

    public void SetCapacity(ResourceType type, int amount)
    {
        capacities[type] = amount;
        if (!stocks.ContainsKey(type))
        {
            stocks[type] = 0;
        }
    }
}
