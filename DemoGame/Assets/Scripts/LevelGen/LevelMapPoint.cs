using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public struct LevelMapPoint
{
    public LevelMapPoint(
        bool isStreetAlongXRadius,
        bool isStreetAlongZRadius,
        bool isStreetAlongX,
        bool isStreetAlongZ)
    {
        IsStreetAlongZRadius = isStreetAlongXRadius;
        IsStreetAlongXRadius = isStreetAlongZRadius;
        IsStreetAlongX = isStreetAlongX;
        IsStreetAlongZ = isStreetAlongZ;
        WasHandledByTerrainGenerator = false;
        WasHandledByBuildingGenerator = false;
        HasAssignedObject = false;
        MaterialNumber = 0;
        y = 0;
    }
    public bool IsStreetAlongZRadius;
    public bool IsStreetAlongXRadius;
    public bool IsStreetAlongX;
    public bool IsStreetAlongZ;
    public bool WasHandledByTerrainGenerator;
    public bool WasHandledByBuildingGenerator;
    public bool HasAssignedObject;
    public int MaterialNumber;
    public float y;
}
