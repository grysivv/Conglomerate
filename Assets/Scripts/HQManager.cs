using System;
using System.Collections.Generic;
using UnityEngine;

public enum UpgradeType
{
    LogisticsCapacity,
    LogisticsSpeed,
    StorageCapacity
}

public class HQManager : MonoBehaviour
{
    public static HQManager Instance { get; private set; }

    [Header("Referencje")]
    public CorporationManager corporationManager;

    [Header("Poziomy ulepszeń")]
    private Dictionary<UpgradeType, int> upgradeLevels = new Dictionary<UpgradeType, int>();

    [Header("Koszty ulepszeń")]
    public double baseUpgradeCost = 50000.0;
    public double costMultiplierPerLevel = 1.5;

    public event Action OnUpgradesChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Inicjalizacja poziomów ulepszeń na 0
        foreach (UpgradeType type in Enum.GetValues(typeof(UpgradeType)))
        {
            upgradeLevels[type] = 0;
        }
    }

    public int GetUpgradeLevel(UpgradeType type)
    {
        if (upgradeLevels.TryGetValue(type, out int level))
        {
            return level;
        }
        return 0;
    }

    public double GetUpgradeCost(UpgradeType type)
    {
        int currentLevel = GetUpgradeLevel(type);
        return baseUpgradeCost * Math.Pow(costMultiplierPerLevel, currentLevel);
    }

    public bool TryPurchaseUpgrade(UpgradeType type)
    {
        if (corporationManager == null)
        {
            Debug.LogError("HQManager: CorporationManager nie jest podpięty.");
            return false;
        }

        double cost = GetUpgradeCost(type);

        if (corporationManager.cash >= cost)
        {
            corporationManager.cash -= cost;
            upgradeLevels[type]++;
            Debug.Log($"<b><color=#4caf50>[HQ]</color></b> Ulepszono {type} na poziom {upgradeLevels[type]}. Koszt: {cost} USD.");

            OnUpgradesChanged?.Invoke();
            return true;
        }
        else
        {
            Debug.Log($"<b><color=#f44336>[HQ]</color></b> Niewystarczające fundusze na ulepszenie {type}. Wymagane: {cost} USD, Posiadane: {corporationManager.cash} USD.");
            return false;
        }
    }

    // --- Modyfikatory ulepszeń ---

    public int GetTruckCapacityBonus()
    {
        // Ulepszenie 1: Pojemność ciężarówek (zwiększa bazową ładowność ciężarówki we FleetManager z 20t na 30t, 40t itd.)
        // Dodajemy +10 za każdy poziom ulepszenia.
        return GetUpgradeLevel(UpgradeType.LogisticsCapacity) * 10;
    }

    public float GetFleetSpeedMultiplier()
    {
        // Ulepszenie 2: Szybkość floty (skraca czas dostawy o 10%, 20%, 30%)
        // Każdy poziom skraca czas o 10% (mnożnik czasu maleje do maksymalnie powiedzmy 0.1)
        int level = GetUpgradeLevel(UpgradeType.LogisticsSpeed);
        float deduction = level * 0.10f;
        // Zabezpieczenie przed redukcją czasu poniżej 10% oryginału
        float multiplier = Mathf.Max(0.1f, 1.0f - deduction);
        return multiplier;
    }

    public int GetGlobalStorageBonus()
    {
        // Ulepszenie: Zwiększenie globalnej pojemności silosów (+100t, +200t do limitów w GlobalInventoryManager)
        return GetUpgradeLevel(UpgradeType.StorageCapacity) * 100;
    }
}
