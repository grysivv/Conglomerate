using UnityEngine;
using System.Collections.Generic;

public class MicrochipFactory : ProductionBuilding
{
    protected override void Awake()
    {
        base.Awake();
    }

    public void BuildFactory()
    {
        if (isBuilt) return;
        isBuilt = true;
    }
}
