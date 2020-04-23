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
        Y = 0;
    }

    public bool IsStreetAlongZRadius;
    public bool IsStreetAlongXRadius;
    public bool IsStreetAlongX;
    public bool IsStreetAlongZ;
    public bool WasHandledByTerrainGenerator;
    public bool WasHandledByBuildingGenerator;
    public bool HasAssignedObject;
    public int MaterialNumber;
    public float Y;
}