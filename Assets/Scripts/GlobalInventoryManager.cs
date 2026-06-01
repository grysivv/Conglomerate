// GlobalInventoryManager.cs
using UnityEngine;
using System.Collections.Generic;

public class GlobalInventoryManager : MonoBehaviour
{
    [System.Serializable]
    public class StorageLimit
    {
        public ResourceType type;
        public int maxCapacity;
    }

    [Header("Ustawienia Pojemności Magazynów")]
    public List<StorageLimit> storageLimits = new List<StorageLimit>()
    {
        new StorageLimit { type = ResourceType.Silicon, maxCapacity = 100 },
        new StorageLimit { type = ResourceType.Coal, maxCapacity = 500 },
        new StorageLimit { type = ResourceType.Microchip, maxCapacity = 50 } // Limit procesorów
    };

    private Dictionary<ResourceType, int> stocks = new Dictionary<ResourceType, int>();
    private Dictionary<ResourceType, int> capacities = new Dictionary<ResourceType, int>();

    private void Awake()
    {
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
        if (!stocks.ContainsKey(type)) return;
        stocks[type] = Mathf.Min(stocks[type] + amount, capacities[type]);
    }

    public bool RemoveResource(ResourceType type, int amount)
    {
        if (!stocks.ContainsKey(type) || stocks[type] < amount) return false;
        stocks[type] -= amount;
        return true;
    }
}