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

    public static event Action<float> OnEfficiencyChanged;

    private BuildingBase buildingBase;

    protected virtual void Awake()
    {
        buildingBase = GetComponent<BuildingBase>();
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
            if (laborEfficiency != 0f)
            {
                laborEfficiency = 0f;
                OnEfficiencyChanged?.Invoke(laborEfficiency);
            }
            return;
        }

        float wageRatio = (float)(currentSalary / expectedSalary);
        float newEfficiency = Mathf.Clamp(wageRatio, 0.5f, 1.5f);

        if (laborEfficiency != newEfficiency)
        {
            laborEfficiency = newEfficiency;
            OnEfficiencyChanged?.Invoke(laborEfficiency);
        }

        if (wageRatio < 0.9f && UnityEngine.Random.value > 0.4f && currentWorkers > 0)
        {
            currentWorkers--;
            Debug.Log("<b><color=#ef5350>[HR]</color></b> Pracownik odszedł z fabryki z powodu zbyt niskiej płacy!");
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
}
