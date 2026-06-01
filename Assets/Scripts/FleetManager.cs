// FleetManager.cs
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

    [Header("Stan Ciężarówki")]
    public bool isEnRoute = false;
    public int currentLoad = 0;
    public float currentJourneyTimer = 0f;

    private Vector3 startPos = new Vector3(-3, 0, 0);
    private Vector3 endPos = new Vector3(3, 0, 0);

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }
        // Wymagane w poleceniu: trójkąt/kwadrat, żółty kolor - gracz ustawi to sobie w edytorze,
        // my tylko wymuszamy domyślny kolor jakby co.
        spriteRenderer.color = Color.yellow;
    }

    void Start()
    {
        transform.position = startPos;
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
            // Oczekujemy minimum 5 ton, by wyruszyć w trasę (wymóg z polecenia)
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
        if (timeManager == null || timeManager.isPaused) return;

        if (isEnRoute)
        {
            // Pobranie prywatnego currentSpeedMultiplier przez refleksję, ponieważ TimeManager
            // nie został zmodyfikowany pod kątem publicznego dostępu do tej zmiennej.
            float speedMultiplier = 1.0f;
            FieldInfo field = typeof(TimeManager).GetField("currentSpeedMultiplier", BindingFlags.NonPublic | BindingFlags.Instance);
            if (field != null)
            {
                speedMultiplier = (float)field.GetValue(timeManager);
            }

            // Prędkość poruszania zsynchronizowana z czasem wirtualnym
            // baseSecondsPerHour = 1 realna sekunda (przy 1x) to 1 godzina gry.
            // transportDurationHours = 3.0f oznacza, że transport ma trwać 3 wirtualne godziny.
            // W realnym czasie przy prędkości 1x powinno to zająć (3.0 * baseSecondsPerHour) sekund.
            // Zatem w każdej klatce przyrost wynosi:
            float realSecondsNeeded = transportDurationHours * timeManager.baseSecondsPerHour;

            // Ponieważ timer odmierza się w realnych sekundach (właściwie to w zsynchronizowanych sekundach),
            // doliczamy czas, uwzględniając przyspieszenie gry.
            currentJourneyTimer += Time.deltaTime * speedMultiplier;

            // Obliczenie procentu podróży
            float journeyProgress = Mathf.Clamp01(currentJourneyTimer / realSecondsNeeded);

            // Przesunięcie wizualne
            transform.position = Vector3.Lerp(startPos, endPos, journeyProgress);

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
                transform.position = startPos;

                Debug.Log("<b>[Logistyka]</b> Ciężarówka dotarła na rynek, rozładowała towar i powróciła do magazynu.");
            }
        }
    }
}