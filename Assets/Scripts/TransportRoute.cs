using System;

public enum DestinationType
{
    Market,
    Factory,
    GlobalInventory // Added to match previous logic where it could go to global
}

[Serializable]
public class TransportRoute
{
    public ProductionBuilding sourceBuilding;
    public DestinationType destinationType;
    public ProductionBuilding destinationBuilding;
    public ResourceType resourceType;
    public int batchSize = 20;
    public float transportDurationHours = 3.0f;
    public double fuelCostPerDelivery = 50.00;

    // Runtime variables
    [NonSerialized] public bool isEnRoute = false;
    [NonSerialized] public float currentJourneyProgress = 0f;
    [NonSerialized] public int currentLoad = 0;
}
