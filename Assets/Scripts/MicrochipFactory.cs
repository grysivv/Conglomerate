// MicrochipFactory.cs
using UnityEngine;

public class MicrochipFactory : MonoBehaviour
{
    [Header("Menedżerowie")]
    public TimeManager timeManager;
    public CorporationManager corporationManager;
    public GlobalInventoryManager globalInventory;

    [Header("Stan Infrastruktury")]
    public bool isBuilt = false;
    public bool isOperating = true;

    [Header("Panel HR (Zarządzanie Kadrami)")]
    public int currentWorkers = 0;          // Aktualna liczba inżynierów
    public int maxWorkers = 5;              // Maksymalna pojemność fabryki
    public double currentSalary = 300.00;   // Oferowana stawka godzinowa za pracownika
    public double expectedSalary = 300.00;  // Oczekiwania rynku pracy
    public float laborEfficiency = 0f;     // Efektywność (0.0 do 1.5 czyli 0% - 150%)

    [Header("Ustawienia Produkcji")]
    public int siliconRequiredPerChip = 2;
    public int coalRequiredPerChip = 1;
    public int productionTimeHours = 4;
    public double baseHourlyMaintenanceCost = 200.00; // Koszt maszynowy

    [Header("Aktualny Proces")]
    public bool isProducing = false;
    public int currentProductionTimer = 0;

    void OnEnable() { TimeManager.OnHourlyTick += HandleHourlyTick; }
    void OnDisable() { TimeManager.OnHourlyTick -= HandleHourlyTick; }

    private void HandleHourlyTick()
    {
        if (!isBuilt || !isOperating) return;
        if (timeManager == null || corporationManager == null || globalInventory == null) return;

        // 1. Aktualizacja rynku pracy i efektywności ludzi co godzinę
        UpdateHRMechanics();

        // 2. Wyliczenie całkowitego kosztu godziny (Maszyny + Wypłaty inżynierów)
        double totalLaborCost = currentWorkers * currentSalary;
        double totalHourlyCost = baseHourlyMaintenanceCost + totalLaborCost;
        corporationManager.cash -= totalHourlyCost;

        // Jeśli nie ma ani jednego pracownika, produkcja zostaje zamrożona
        if (currentWorkers <= 0)
        {
            isProducing = false;
            currentProductionTimer = 0;
            return;
        }

        if (isProducing)
        {
            currentProductionTimer++;

            // Efektywność modyfikuje czas produkcji (wysoka pensja = szybsza praca)
            int requiredTime = Mathf.RoundToInt(productionTimeHours / laborEfficiency);
            requiredTime = Mathf.Max(1, requiredTime); // Czas nie może spaść poniżej 1 godziny

            if (currentProductionTimer >= requiredTime)
            {
                if (globalInventory.GetFreeSpace(ResourceType.Microchip) > 0)
                {
                    globalInventory.AddResource(ResourceType.Microchip, 1);
                    isProducing = false;
                    currentProductionTimer = 0;
                    Debug.Log("<b><color=#9c27b0>[FABRYKA]</color></b> Sukces! Wyprodukowano 1 procesor.");
                }
            }
        }
        else
        {
            TryStartProduction();
        }
    }

    private void UpdateHRMechanics()
    {
        if (currentWorkers <= 0) { laborEfficiency = 0f; return; }

        // Wyliczenie stosunku pensji do oczekiwań
        float wageRatio = (float)(currentSalary / expectedSalary);
        laborEfficiency = Mathf.Clamp(wageRatio, 0.5f, 1.5f); // Efektywność mieści się w granicach 50% - 150%

        // Losowa fluktuacja rynku (ludzie odchodzą jeśli płacisz poniżej oczekiwań)
        if (wageRatio < 0.9f && Random.value > 0.4f && currentWorkers > 0)
        {
            currentWorkers--;
            Debug.Log("<b><color=#ef5350>[HR]</color></b> Pracownik odszedł z fabryki z powodu zbyt niskiej płacy!");
        }
    }

    private void TryStartProduction()
    {
        if (currentWorkers <= 0 || globalInventory.GetFreeSpace(ResourceType.Microchip) <= 0) return;

        int availableSilicon = globalInventory.GetStock(ResourceType.Silicon);
        int availableCoal = globalInventory.GetStock(ResourceType.Coal);

        if (availableSilicon >= siliconRequiredPerChip && availableCoal >= coalRequiredPerChip)
        {
            globalInventory.RemoveResource(ResourceType.Silicon, siliconRequiredPerChip);
            globalInventory.RemoveResource(ResourceType.Coal, coalRequiredPerChip);

            isProducing = true;
            currentProductionTimer = 0;
        }
    }

    // --- PUBLICZNE METODY DLA PRZYCISKÓW W UI ---
    public void HireWorker()
    {
        if (currentWorkers < maxWorkers) currentWorkers++;
    }

    public void FireWorker()
    {
        if (currentWorkers > 0) currentWorkers--;
    }

    public void AdjustWage(double amount)
    {
        currentSalary = System.Math.Max(50.00, currentSalary + amount); // Minimalna stawka to 50 USD/h
    }

    public void BuildFactory()
    {
        if (isBuilt) return;
        isBuilt = true;
    }
}