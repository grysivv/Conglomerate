// SiliconMine.cs
using UnityEngine;

public class SiliconMine : MonoBehaviour
{
    [Header("Menedżerowie")]
    public TimeManager timeManager;
    public CorporationManager corporationManager;

    [Header("Stan kopalni")]
    public bool isOperating = true;
    public int siliconStorage = 0;

    [Header("Koszty i produkcja")]
    public double baseHourlyEnergyConsumption = 40.00;
    public double hourlyLaborCost = 200.00;
    public int baseHourlyProduction = 5;

    void OnEnable()
    {
        TimeManager.OnHourlyTick += HandleHourlyTick;
    }

    void OnDisable()
    {
        TimeManager.OnHourlyTick -= HandleHourlyTick;
    }

    private void HandleHourlyTick()
    {
        if (!isOperating) return;

        if (timeManager == null || corporationManager == null)
        {
            Debug.LogWarning("[Kopalnia Krzemu] Brak referencji do TimeManager lub CorporationManager.");
            return;
        }

        // Pobranie mnożnika kosztów energii z TimeManagera
        float energyMultiplier = timeManager.GetEnergyCostMultiplier();

        // Obliczenie całkowitego kosztu godziny pracy
        double currentHourEnergyCost = baseHourlyEnergyConsumption * energyMultiplier;
        double totalHourlyCost = currentHourEnergyCost + hourlyLaborCost;

        // Odjęcie kosztów
        corporationManager.cash -= totalHourlyCost;

        // Dodanie krzemu do magazynu
        siliconStorage += baseHourlyProduction;

        // Logowanie do konsoli
        Debug.Log($"<b>[Kopalnia Krzemu]</b> Produkcja w toku. Wyprodukowano: {baseHourlyProduction}t. " +
                  $"Aktualny stan magazynu: {siliconStorage}t. " +
                  $"Koszt godziny: {totalHourlyCost:F2} USD (Energia: {currentHourEnergyCost:F2} USD, Praca: {hourlyLaborCost:F2} USD).");
    }
}
