using UnityEngine;

public class SiliconMine : MonoBehaviour
{
    [Header("References")]
    public TimeManager timeManager;
    public CorporationManager corporationManager;

    [Header("State")]
    public bool isOperating = true;
    public int siliconStorage = 0;

    [Header("Configuration")]
    public double baseHourlyEnergyConsumption = 40.00;
    public double hourlyLaborCost = 200.00;
    public int baseHourlyProduction = 5;

    private void OnEnable()
    {
        if (timeManager != null)
        {
            timeManager.OnHourlyTick += HandleHourlyTick;
        }
        else
        {
            Debug.LogWarning("TimeManager reference is missing in SiliconMine!");
        }
    }

    private void OnDisable()
    {
        if (timeManager != null)
        {
            timeManager.OnHourlyTick -= HandleHourlyTick;
        }
    }

    private void HandleHourlyTick()
    {
        if (!isOperating) return;

        if (timeManager == null || corporationManager == null)
        {
            Debug.LogError("Missing references in SiliconMine. Cannot process hourly tick.");
            return;
        }

        // Pobranie z TimeManager aktualnego mnożnika cen energii
        double energyMultiplier = timeManager.GetEnergyCostMultiplier();

        // Obliczenie całkowitego kosztu godziny pracy
        double totalHourlyCost = (baseHourlyEnergyConsumption * energyMultiplier) + hourlyLaborCost;

        // Odjęcie kosztu ze stanu konta
        corporationManager.cash -= totalHourlyCost;

        // Dodanie wydobycia do magazynu
        siliconStorage += baseHourlyProduction;

        // Wyrzucenie do konsoli Unity precyzyjnego loga
        Debug.Log($"[SiliconMine] Status: Working | Cost for this hour: {totalHourlyCost:F2} | Silicon Storage: {siliconStorage} tons");
    }
}