// CorporationManager.cs
using UnityEngine;

public class CorporationManager : MonoBehaviour
{
    [Header("Fundusze Korporacji")]
    public double cash = 1000000.0; // Zgodnie z poleceniem: startowo 1 000 000.00 USD

    [Header("Koszty Stałe")]
    public double baseHQMaintenanceCost = 500.00;

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
        // Pobieranie stałych kosztów co godzinę
        cash -= baseHQMaintenanceCost;
    }
}