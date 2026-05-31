// TimeManager.cs
using UnityEngine;
using System;

public enum DayPhase { NOC, PORANEK, SZCZYT_PORANNY, GODZINY_PRACY, SZCZYT_POPOŁUDNIOWY, WIECZÓR, WYGASZANIE }

public class TimeManager : MonoBehaviour
{
    [Header("Ustawienia Czasu")]
    public float baseSecondsPerHour = 1.0f; // 1 sekunda realna = 1 godzina gry przy 1x
    public bool isPaused = false;
    private float currentSpeedMultiplier = 1.0f;

    [Header("Aktualny Stan Zegara")]
    public int currentHour = 0;
    public int currentDay = 1;
    public DayPhase currentPhase;

    public static event Action OnHourlyTick;
    private float timer = 0f;

    void Start() { UpdateDayPhase(); }

    void Update()
    {
        if (isPaused) return;

        // Licznik tyka szybciej, im wyższy jest mnożnik prędkości
        timer += Time.deltaTime * currentSpeedMultiplier;
        while (timer >= baseSecondsPerHour)
        {
            timer -= baseSecondsPerHour;
            ExecuteHourlyTick();
        }
    }

    private void ExecuteHourlyTick()
    {
        currentHour++;
        if (currentHour >= 24) { currentHour = 0; currentDay++; }
        UpdateDayPhase();
        OnHourlyTick?.Invoke();
    }

    private void UpdateDayPhase()
    {
        if (currentHour >= 0 && currentHour < 5) currentPhase = DayPhase.NOC;
        else if (currentHour >= 5 && currentHour < 8) currentPhase = DayPhase.PORANEK;
        else if (currentHour >= 8 && currentHour < 10) currentPhase = DayPhase.SZCZYT_PORANNY;
        else if (currentHour >= 10 && currentHour < 16) currentPhase = DayPhase.GODZINY_PRACY;
        else if (currentHour >= 16 && currentHour < 19) currentPhase = DayPhase.SZCZYT_POPOŁUDNIOWY;
        else if (currentHour >= 19 && currentHour < 22) currentPhase = DayPhase.WIECZÓR;
        else currentPhase = DayPhase.WYGASZANIE;
    }

    // Funkcje sterujące czasem wywoływane przez przyciski z UI
    public void SetPause(bool pauseStatus)
    {
        isPaused = pauseStatus;
        Debug.Log($"<b>[ZEGAR]</b> Zmiana stanu pauzy: {isPaused}");
    }

    public void SetSpeed(float newSpeed)
    {
        isPaused = false;
        currentSpeedMultiplier = newSpeed;
        Debug.Log($"<b>[ZEGAR]</b> Zmiana prędkości gry na: {newSpeed}x");
    }

    public float GetEnergyCostMultiplier()
    {
        if (currentPhase == DayPhase.NOC) return 0.3f;
        if (currentPhase == DayPhase.SZCZYT_PORANNY || currentPhase == DayPhase.SZCZYT_POPOŁUDNIOWY) return 3.0f;
        return 1.0f;
    }
}