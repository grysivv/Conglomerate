// GlobalInventoryManager.cs
using UnityEngine;

public class GlobalInventoryManager : MonoBehaviour
{
    [Header("Stan Magazynu")]
    public int siliconInStock = 0;

    public void AddSilicon(int amount)
    {
        if (amount > 0)
        {
            siliconInStock += amount;
        }
    }

    public bool RemoveSilicon(int amount)
    {
        if (amount > 0 && siliconInStock >= amount)
        {
            siliconInStock -= amount;
            return true;
        }
        return false;
    }
}