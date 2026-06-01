using UnityEngine;

public class SiliconMine : MonoBehaviour
{
    [Header("Menedżerowie")]
    public TimeManager timeManager;
    public CorporationManager corporationManager;
    public GlobalInventoryManager globalInventoryManager;

    [Header("Stan kopalni")]
    public bool isOperating = true;
    public int totalSiliconProduced = 0; // Local counter for statistical purposes

    [Header("Koszty i produkcja")]
    public double baseHourlyEnergyConsumption = 40.00;
    public double hourlyLaborCost = 200.00;
    public int baseHourlyProduction = 5;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        UpdateMapVisuals();
    }

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

        if (timeManager == null || corporationManager == null || globalInventoryManager == null)
        {
            Debug.LogWarning("[Kopalnia Krzemu] Brak referencji do TimeManager, CorporationManager lub GlobalInventoryManager.");
            return;
        }

        // Pobranie mnożnika kosztów energii z TimeManagera
        float energyMultiplier = timeManager.GetEnergyCostMultiplier();

        // Obliczenie całkowitego kosztu godziny pracy
        double currentHourEnergyCost = baseHourlyEnergyConsumption * energyMultiplier;
        double totalHourlyCost = currentHourEnergyCost + hourlyLaborCost;

        // Odjęcie kosztów
        corporationManager.cash -= totalHourlyCost;

        // Przekazanie krzemu natychmiast do magazynu głównego
        globalInventoryManager.AddSilicon(baseHourlyProduction);

        // Zaktualizowanie licznika statystycznego
        totalSiliconProduced += baseHourlyProduction;

        // Aktualizacja widoku kopalni
        UpdateMapVisuals();

        // Logowanie do konsoli
        Debug.Log($"<b>[Kopalnia Krzemu]</b> Produkcja w toku. Wyprodukowano: {baseHourlyProduction}t i przekazano do magazynu. " +
                  $"Sumaryczna produkcja (statystyka): {totalSiliconProduced}t. " +
                  $"Koszt godziny: {totalHourlyCost:F2} USD (Energia: {currentHourEnergyCost:F2} USD, Praca: {hourlyLaborCost:F2} USD).");
    }

    public void UpdateMapVisuals()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = isOperating ? Color.green : Color.red;
        }
    }
}
