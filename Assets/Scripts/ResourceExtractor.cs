// ResourceExtractor.cs
using UnityEngine;

public class ResourceExtractor : MonoBehaviour
{
    [Header("Ustawienia Złoża")]
    public ResourceType resourceType;
    public int remainingDeposit = 0;
    public bool hasPlotPurchased = false;

    [Header("Wydajność i Magazyn lokalny")]
    public int extractionRatePerHour = 10;
    public int localInventory = 0;
    public int maxLocalCapacity = 100;

    [Header("Referencje")]
    private GlobalInventoryManager globalInventory;

    void Start()
    {
        // Automatycznie szukamy menedżera magazynu na scenie, żeby kopalnia miała gdzie oddawać surowce
        globalInventory = Object.FindFirstObjectByType<GlobalInventoryManager>();
    }

    void OnEnable()
    {
        // ZAPISANIE SIĘ NA ZEGAR: Kopalnia zaczyna słuchać menedżera czasu
        TimeManager.OnHourlyTick += HandleHourlyExtraction;
    }

    void OnDisable()
    {
        // WYPISANIE SIĘ Z ZEGARA: Czyszczenie referencji przy wyłączeniu obiektu
        TimeManager.OnHourlyTick -= HandleHourlyExtraction;
    }

    private void HandleHourlyExtraction()
    {
        // Warunek 1: Jeśli działka nie jest kupiona, kopalnia nic nie robi
        if (!hasPlotPurchased) return;

        // Warunek 2: Jeśli złoże się wyczerpało, zatrzymaj wydobycie
        if (remainingDeposit <= 0)
        {
            remainingDeposit = 0;
            return;
        }

        // Warunek 3: Jeśli lokalny kosz kopalni jest pełen, czekamy na ciężarówkę
        if (localInventory >= maxLocalCapacity)
        {
            Debug.Log($"<color=#ff9800>[KOPALNIA]</color> Magazyn lokalny kopalni {resourceType} jest pełny! Wydobycie wstrzymane.");
            return;
        }

        // Obliczanie faktycznego wydobycia (zabezpieczenie, żeby nie wykopać więcej niż zostało w ziemi)
        int amountToExtract = Mathf.Min(extractionRatePerHour, remainingDeposit);

        // Zabezpieczenie przed przepełnieniem magazynu lokalnego
        if (localInventory + amountToExtract > maxLocalCapacity)
        {
            amountToExtract = maxLocalCapacity - localInventory;
        }

        // Aktualizacja wartości
        remainingDeposit -= amountToExtract;
        localInventory += amountToExtract;

        Debug.Log($"<color=#4caf50>[KOPALNIA]</color> Wydobyto {amountToExtract}t {resourceType}. W ziemi zostało: {remainingDeposit}t. W kopalni: {localInventory}t.");
    }
}