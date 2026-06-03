using System;

public enum DestinationType
{
    Factory,
    GlobalInventory
}

[Serializable]
public class TransportRoute
{
    public BuildingBase sourceBuilding;
    public DestinationType destinationType;
    public BuildingBase destinationBuilding;
    public ResourceType resourceType;
    public int batchSize = 20;
    public float transportDurationHours = 3.0f;
    public double fuelCostPerDelivery = 50.00;

    // Runtime variables
    [NonSerialized] public bool isEnRoute = false;
    [NonSerialized] public float currentJourneyProgress = 0f;
    [NonSerialized] public int currentLoad = 0;
}
