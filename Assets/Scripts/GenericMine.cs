using UnityEngine;

[RequireComponent(typeof(BuildingBase), typeof(InventoryComponent), typeof(ResourceExtractor))]
public class GenericMine : MonoBehaviour
{
    // The previous implementation inherited from ProductionBuilding.
    // In the new Component-Driven Architecture, the actual extraction logic is handled by ResourceExtractor,
    // the inventory by InventoryComponent, and base state by BuildingBase.

    // We can keep this class as an empty shell or specialized marker if needed,
    // but the actual functionality has been moved to the modular components.

    // NOTE: Plot purchasing and deposit are now properties directly managed via ResourceExtractor/BuildingBase in UIManager.
    // If you need custom initialization, it goes here.
}
