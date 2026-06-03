using UnityEngine;
using System;

public class HumanResourcesComponent : MonoBehaviour
{
    [Header("Panel HR")]
    public int currentWorkers = 0;
    public int maxWorkers = 5;
    public double currentSalary = 300.00;
    public double expectedSalary = 300.00;
    public float laborEfficiency = 1f;

    [Header("BHP i Nastroje")]
    [Range(0, 100)] public float morale = 100f;
    [Range(0, 100)] public float fatigue = 0f;

    [Header("Koszty Akcji")]
    public double safetyTrainingCost = 2500.00;
    public double fundBenefitsCost = 5000.00;
    public double accidentPenalty = 10000.00;

    // Events
    public static event Action<float> OnEfficiencyChanged;
    public event Action<float> OnMoraleChanged;
    public event Action<float> OnFatigueChanged;
    public event Action OnAccidentOccurred;

    // Internal state
    private BuildingBase buildingBase;
    private TimeManager timeManager;
    private CorporationManager corporationManager;

    private int safetyTrainingTimer = 0;
    private int accidentBlockTimer = 0;

    protected virtual void Awake()
    {
        buildingBase = GetComponent<BuildingBase>();
    }

    protected virtual void Start()
    {
        timeManager = FindObjectOfType<TimeManager>();
        corporationManager = FindObjectOfType<CorporationManager>();
    }

    protected virtual void OnEnable()
    {
        TimeManager.OnHourlyTick += HandleHourlyTick;
    }

    protected virtual void OnDisable()
    {
        TimeManager.OnHourlyTick -= HandleHourlyTick;
    }

    private void HandleHourlyTick()
    {
        if (buildingBase != null && (!buildingBase.isBuilt || !buildingBase.isOperating)) return;

        UpdateHRMechanics();
    }

    private void UpdateHRMechanics()
    {
        if (currentWorkers <= 0)
        {
            SetEfficiency(0f);
            return;
        }

        // Accident Block Logic
        if (accidentBlockTimer > 0)
        {
            accidentBlockTimer--;
            SetEfficiency(0f); // Production blocked
            return;
        }

        // Safety Training Logic
        if (safetyTrainingTimer > 0)
        {
            safetyTrainingTimer--;
        }

        // Fatigue and Morale Update
        float fatigueIncrease = 2f; // Base fatigue increase per hour
        float moraleDecrease = 0f;

        if (timeManager != null && timeManager.currentPhase == DayPhase.NOC)
        {
            fatigueIncrease *= 2f;
            moraleDecrease = 2f; // Morale drops during night shift
        }

        fatigue = Mathf.Clamp(fatigue + fatigueIncrease, 0f, 100f);
        morale = Mathf.Clamp(morale - moraleDecrease, 0f, 100f);

        OnFatigueChanged?.Invoke(fatigue);
        OnMoraleChanged?.Invoke(morale);

        // Accident Chance
        if (safetyTrainingTimer <= 0)
        {
            // Chance scales with fatigue. Max 5% at 100 fatigue.
            float accidentChance = (fatigue / 100f) * 0.05f;
            if (UnityEngine.Random.value < accidentChance)
            {
                TriggerAccident();
                return; // Stop further updates this tick
            }
        }

        // Efficiency Calculation
        float wageRatio = (float)(currentSalary / expectedSalary);
        float newEfficiency = Mathf.Clamp(wageRatio, 0.5f, 1.5f);

        // High fatigue and low morale drastically reduce efficiency
        if (fatigue > 80f && morale < 30f)
        {
            newEfficiency *= 0.3f; // 70% penalty
        }
        else if (fatigue > 80f)
        {
            newEfficiency *= 0.7f; // 30% penalty
        }
        else if (morale < 30f)
        {
            newEfficiency *= 0.8f; // 20% penalty
        }

        SetEfficiency(newEfficiency);

        // Worker leaving due to low wage
        if (wageRatio < 0.9f && UnityEngine.Random.value > 0.4f)
        {
            currentWorkers--;
            Debug.Log("<b><color=#ef5350>[HR]</color></b> Pracownik odszedł z fabryki z powodu zbyt niskiej płacy!");

            // If we lost the last worker, set efficiency to 0
            if (currentWorkers <= 0)
            {
                SetEfficiency(0f);
            }
        }
    }

    private void SetEfficiency(float newEfficiency)
    {
        if (laborEfficiency != newEfficiency)
        {
            laborEfficiency = newEfficiency;
            OnEfficiencyChanged?.Invoke(laborEfficiency);
        }
    }

    private void TriggerAccident()
    {
        Debug.Log("<b><color=#ef5350>[HR]</color></b> WYPADEK W PRACY! Produkcja wstrzymana, nałożono karę.");

        accidentBlockTimer = 12; // Block production for 12 hours
        morale = 0f;

        OnMoraleChanged?.Invoke(morale);
        OnAccidentOccurred?.Invoke();

        SetEfficiency(0f);

        if (corporationManager != null)
        {
            corporationManager.cash -= accidentPenalty;
        }
    }

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
        currentSalary = System.Math.Max(50.00, currentSalary + amount);
    }

    // Public Management Tools for UI
    public void TriggerSafetyTraining()
    {
        if (corporationManager != null && corporationManager.cash >= safetyTrainingCost)
        {
            corporationManager.cash -= safetyTrainingCost;
            safetyTrainingTimer = 24; // Protects for 24 hours
            Debug.Log("<b><color=#4caf50>[HR]</color></b> Zorganizowano szkolenie BHP. Bezpieczeństwo zapewnione na 24h.");
        }
        else
        {
            Debug.LogWarning("<b><color=#ef5350>[HR]</color></b> Brak środków na szkolenie BHP!");
        }
    }

    public void FundBenefits()
    {
        if (corporationManager != null && corporationManager.cash >= fundBenefitsCost)
        {
            corporationManager.cash -= fundBenefitsCost;
            morale = 100f;
            fatigue = Mathf.Clamp(fatigue - 50f, 0f, 100f); // Lowers fatigue by 50

            OnMoraleChanged?.Invoke(morale);
            OnFatigueChanged?.Invoke(fatigue);

            Debug.Log("<b><color=#4caf50>[HR]</color></b> Ufundowano pakiety pracownicze. Morale zregenerowane, zmęczenie zmniejszone.");
        }
        else
        {
            Debug.LogWarning("<b><color=#ef5350>[HR]</color></b> Brak środków na pakiety pracownicze!");
        }
    }
}