// MicrochipFactory.cs
using UnityEngine;
using System.Collections.Generic;

public class MicrochipFactory : ProductionBuilding
{
    protected override void Awake()
    {
        // KLUCZOWA POPRAWKA: Wymuszamy aktywację systemu HR dla tej konkretnej fabryki na starcie,
        // żeby przyciski zatrudniania z UI mogły dodawać pracowników.
        usesHR = true;

        base.Awake();
    }

    public void BuildFactory()
    {
        if (isBuilt) return;
        isBuilt = true;
        Debug.Log("<b><color=#9c27b0>[FABRYKA]</color></b> Fabryka procesorów gotowa do pracy! Czeka na inżynierów.");
    }
}