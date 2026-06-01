using UnityEngine;
using System.Reflection;

public class FleetManager : MonoBehaviour
{
    [Header("Menedżerowie")]
    public GlobalInventoryManager globalInventoryManager;
    public CorporationManager corporationManager;
    public MarketManager marketManager;
    public TimeManager timeManager;

    [Header("Ustawienia Floty")]
    public float transportDurationHours = 3.0f;
    public int truckCapacity = 20;
    public double fuelCostPerDelivery = 50.00;

    [Header("Punkty Trasy")]
    public Transform startPoint;
    public Transform endPoint;

    [Header("Stan Ciężarówki")]
    public bool isEnRoute = false;
    public int currentLoad = 0;
    public float currentJourneyTimer = 0f;

    private SpriteRenderer spriteRenderer;
    private FieldInfo speedMultiplierField;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        spriteRenderer.color = Color.yellow;
        speedMultiplierField = typeof(TimeManager).GetField("currentSpeedMultiplier", BindingFlags.NonPublic | BindingFlags.Instance);
    }

    void Start()
    {
        if (startPoint != null)
        {
            transform.position = startPoint.position;
        }
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
        if (globalInventoryManager == null || corporationManager == null || marketManager == null) return;

        if (!isEnRoute)
        {
            // Oczekujemy minimum 5 ton, by wyruszyć w trasę
            if (globalInventoryManager.siliconInStock >= 5)
            {
                int amountToLoad = Mathf.Min(globalInventoryManager.siliconInStock, truckCapacity);

                if (globalInventoryManager.RemoveSilicon(amountToLoad))
                {
                    corporationManager.cash -= fuelCostPerDelivery;
                    currentLoad = amountToLoad;
                    isEnRoute = true;
                    currentJourneyTimer = 0f;

                    Debug.Log($"<b>[Logistyka]</b> Ciężarówka wyrusza z {currentLoad}t krzemu. Koszt paliwa: {fuelCostPerDelivery:F2} USD.");
                }
            }
        }
    }

    void Update()
    {
        if (timeManager == null || timeManager.isPaused || startPoint == null || endPoint == null) return;

        if (isEnRoute)
        {
            // Pobranie currentSpeedMultiplier przez refleksję, by uwzględnić przyspieszenie gry
            float speedMultiplier = 1.0f;
            if (speedMultiplierField != null)
            {
                speedMultiplier = (float)speedMultiplierField.GetValue(timeManager);
            }

            // Prędkość poruszania zsynchronizowana z czasem wirtualnym
            float realSecondsNeeded = transportDurationHours * timeManager.baseSecondsPerHour;

            // Zwiększanie czasu podróży z uwzględnieniem przyspieszenia
            currentJourneyTimer += Time.deltaTime * speedMultiplier;

            // Obliczenie postępu podróży
            float journeyProgress = Mathf.Clamp01(currentJourneyTimer / realSecondsNeeded);

            // Przesunięcie wizualne (lerp)
            transform.position = Vector3.Lerp(startPoint.position, endPoint.position, journeyProgress);

            // Zakończenie podróży
            if (journeyProgress >= 1.0f)
            {
                if (marketManager != null)
                {
                    marketManager.SellSiliconFromDelivery(currentLoad);
                }

                currentLoad = 0;
                isEnRoute = false;
                currentJourneyTimer = 0f;
                transform.position = startPoint.position; // natychmiastowy powrót na start

                Debug.Log("<b>[Logistyka]</b> Ciężarówka dotarła na rynek, rozładowała towar i powróciła do magazynu.");
            }
        }
    }
}
