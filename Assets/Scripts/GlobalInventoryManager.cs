using UnityEngine;

[RequireComponent(typeof(InventoryComponent))]
public class GlobalInventoryManager : MonoBehaviour
{
    private InventoryComponent inventory;

    private void Awake()
    {
        inventory = GetComponent<InventoryComponent>();
    }

    public int GetStock(ResourceType type)
    {
        return inventory != null ? inventory.GetStock(type) : 0;
    }

    public int GetCapacity(ResourceType type)
    {
        return inventory != null ? inventory.GetCapacity(type) : 0;
    }

    public int GetFreeSpace(ResourceType type)
    {
        return inventory != null ? inventory.GetFreeSpace(type) : 0;
    }

    public void AddResource(ResourceType type, int amount)
    {
        if (inventory != null) inventory.AddResource(type, amount);
    }

    public bool RemoveResource(ResourceType type, int amount)
    {
        return inventory != null && inventory.RemoveResource(type, amount);
    }
}
