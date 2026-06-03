using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct ResourceRequirement
{
    public ResourceType resource;
    public int amount;
}

[CreateAssetMenu(fileName = "NewProductionRecipe", menuName = "Tycoon/Production Recipe")]
public class ProductionRecipe : ScriptableObject
{
    public string recipeName;

    [Header("Input Requirements")]
    public List<ResourceRequirement> inputs = new List<ResourceRequirement>();

    [Header("Output Settings")]
    public ResourceType outputType;
    public int outputAmount = 1;

    [Header("Production Time")]
    public int productionTimeHours = 4;
}
