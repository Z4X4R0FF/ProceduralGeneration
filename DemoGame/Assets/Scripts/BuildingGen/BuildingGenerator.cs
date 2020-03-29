using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuildingGenerator : MonoBehaviour
{
    [Header("Building Settings")]
    public int minBuildingXAxisSize;
    public int maxBuildingXAxisSize;
    public int minBuildingZAxisSize;
    public int maxBuildingZAxisSize;
    public int maxBuildingResizeIterations;
    [Header("Yard Settings")]
    public int minYardZAxisSize;
    public int minYardXAxisSize;
    public int passWidth;
    [Header("Building Parts")]
    public List<Building> BuildingTypes = new List<Building>();
    private LevelMapPoint[, ] LevelMap;

    public IEnumerator GenerateBuildingBasement(LevelMapPoint[, ] levelMap)
    {
        LevelMap = levelMap;
        for (int z = 1; z < levelMap.GetLength(0) - 2; z++)
        {
            for (int x = 1; x < levelMap.GetLength(1) - 2; x++)
            {
                if (levelMap[z, x].MaterialNumber == 0 && !levelMap[z, x].WasHandledByBuildingGenerator)
                {
                    int quadWidth = FindQuadDimensions(z, x, false);
                    int quadLength = FindQuadDimensions(z, x, true);
                    //Debug.Log($"Qw:{quadWidth}; Ql:{quadLength}");
                    yield return StartCoroutine(HandleQuad(z, x, quadLength, quadWidth));
                    UpdateLevelMap(levelMap, z, x, quadWidth, quadLength);
                    yield return null;
                }
            }
        }
    }

    private int FindQuadDimensions(int z, int x, bool findLength)
    {
        int size = 1;
        if (findLength)
        {
            if (z != LevelMap.GetLength(0) - 3)
                while (LevelMap[z + 1, x].MaterialNumber == 0)
                {
                    size++;
                    z++;
                    if (z + 1 >= LevelMap.GetLength(0) - 2) break;
                }
        }
        else
        {
            if (x != LevelMap.GetLength(1) - 3)
                while (LevelMap[z, x + 1].MaterialNumber == 0)
                {
                    size++;
                    x++;
                    if (x + 1 >= LevelMap.GetLength(1) - 2) break;
                }
        }
        return size;
    }
    private IEnumerator HandleQuad(int startingZ, int startingX, int quadZAxisSize, int quadXAxisSize)
    {
        if (quadZAxisSize < minBuildingZAxisSize || quadXAxisSize < minBuildingXAxisSize)
        {
            //Debug.Log("SmallQuad");
            yield break;
        }
        else if ((minBuildingZAxisSize + passWidth) * 2 + minYardZAxisSize < quadZAxisSize &&
            (minBuildingXAxisSize + passWidth) * 2 + minYardXAxisSize < quadXAxisSize)
        {
            Debug.Log("BigYard");
            int buildingXAxisSize = minBuildingXAxisSize;
            int buildingZAxisSize = minBuildingZAxisSize;
            int yardZAxisSize = minYardZAxisSize;
            int yardXAxisSize = minYardXAxisSize;
            int totalXAxisSize = (buildingXAxisSize + passWidth) * 2 + yardXAxisSize;
            int totalZAxisSize = (buildingZAxisSize + passWidth) * 2 + yardZAxisSize;
            while (totalXAxisSize != quadXAxisSize)
            {
                if (totalXAxisSize + 2 > quadXAxisSize)
                {
                    yardXAxisSize++;
                }
                else
                {
                    bool whatToIncrement = Random.Range(0, 2) == 1 ? true : false;
                    if (whatToIncrement && buildingXAxisSize != maxBuildingXAxisSize) buildingXAxisSize++;
                    else yardXAxisSize++;
                }
                totalXAxisSize = yardXAxisSize + (buildingXAxisSize + passWidth) * 2;
            }
            while (totalZAxisSize != quadZAxisSize)
            {
                if (totalZAxisSize + 2 > quadZAxisSize)
                {
                    yardZAxisSize++;
                }
                else
                {
                    bool whatToIncrement = (Random.Range(0, 2) == 1) ? true : false;
                    if (whatToIncrement && buildingZAxisSize != maxBuildingZAxisSize) buildingZAxisSize++;
                    else yardZAxisSize++;
                }
                totalZAxisSize = yardZAxisSize + (buildingZAxisSize + passWidth) * 2;
            }

            SpawnBuildingBasement(startingZ, startingX, buildingZAxisSize, buildingXAxisSize);
            yield return null;
            SpawnBuildingBasement(startingZ + buildingZAxisSize + yardZAxisSize + passWidth * 2, startingX, buildingZAxisSize, buildingXAxisSize);
            yield return null;
            SpawnBuildingBasement(startingZ, startingX + buildingXAxisSize + yardXAxisSize + passWidth * 2, buildingZAxisSize, buildingXAxisSize);
            yield return null;
            SpawnBuildingBasement(startingZ + buildingZAxisSize + yardZAxisSize + passWidth * 2, startingX + buildingXAxisSize + yardXAxisSize + passWidth * 2, buildingZAxisSize, buildingXAxisSize);
            yield return null;
            for (int i = 0; i < quadXAxisSize; i++)
            {
                LevelMap[startingZ + buildingZAxisSize + passWidth - 2, startingX + i].MaterialNumber = 2;
                LevelMap[startingZ + buildingZAxisSize + yardZAxisSize + passWidth * 2 - 2, startingX + i].MaterialNumber = 2;
            }
            for (int i = 0; i < quadZAxisSize; i++)
            {
                LevelMap[startingZ + i, startingX + buildingXAxisSize + passWidth - 2].MaterialNumber = 2;
                LevelMap[startingZ + i, startingX + buildingXAxisSize + yardXAxisSize + passWidth * 2 - 2].MaterialNumber = 2;
            }
            var xAxisSizeRemaining = quadXAxisSize - (buildingXAxisSize + passWidth) * 2;
            var zAxisSizeRemaining = quadZAxisSize - (buildingZAxisSize + passWidth) * 2;
            if (xAxisSizeRemaining >= minBuildingXAxisSize)
            {
                if (xAxisSizeRemaining <= maxBuildingXAxisSize)
                {
                    SpawnBuildingBasement(startingZ, startingX + buildingXAxisSize + passWidth, buildingZAxisSize, xAxisSizeRemaining);
                    yield return null;
                    SpawnBuildingBasement(startingZ + buildingZAxisSize + passWidth * 2 + yardZAxisSize, startingX + buildingXAxisSize + passWidth, buildingZAxisSize, xAxisSizeRemaining);
                    yield return null;
                }
                else
                {
                    List<int> xAxisBuildingSizeList = new List<int>();
                    int iteration = 0;
                    var buildingSize = minBuildingXAxisSize;
                    while (true)
                    {
                        if (xAxisSizeRemaining <= maxBuildingXAxisSize && xAxisSizeRemaining >= minBuildingXAxisSize)
                        {
                            xAxisBuildingSizeList.Add(xAxisSizeRemaining);
                            xAxisSizeRemaining = 0;
                        }
                        else
                        {
                            bool whatToIncrement = (Random.Range(0, 4) == 1) ? true : false;
                            if (!whatToIncrement && buildingSize != maxBuildingXAxisSize)
                            {
                                buildingSize++;
                            }
                            else
                            {
                                if (xAxisSizeRemaining - passWidth - buildingSize < minBuildingXAxisSize)
                                {
                                    var sizeToComplete = xAxisSizeRemaining - buildingSize;
                                    if (sizeToComplete + buildingSize <= maxBuildingXAxisSize)
                                    {
                                        buildingSize += sizeToComplete;
                                        xAxisBuildingSizeList.Add(buildingSize);
                                        xAxisSizeRemaining = 0;
                                    }
                                    else
                                    {
                                        sizeToComplete -= passWidth;
                                        if (buildingSize - (minBuildingXAxisSize - sizeToComplete) >= minBuildingXAxisSize)
                                        {
                                            buildingSize -= (minBuildingXAxisSize - sizeToComplete);
                                            xAxisBuildingSizeList.Add(buildingSize);
                                            xAxisBuildingSizeList.Add(xAxisSizeRemaining - passWidth - buildingSize);
                                            xAxisSizeRemaining = 0;
                                        }
                                        else
                                        {
                                            var count = xAxisBuildingSizeList.Count();
                                            if (count != 0)
                                            {
                                                while (xAxisBuildingSizeList[count - 1] != minBuildingXAxisSize)
                                                {
                                                    xAxisBuildingSizeList[count - 1] -= 1;
                                                    xAxisSizeRemaining += 1;
                                                    if (xAxisSizeRemaining <= maxBuildingXAxisSize || xAxisSizeRemaining - passWidth >= minBuildingXAxisSize * 2)
                                                    {
                                                        buildingSize = minBuildingXAxisSize;
                                                        break;
                                                    }

                                                }
                                                if (xAxisSizeRemaining > maxBuildingXAxisSize || xAxisSizeRemaining - passWidth < minBuildingXAxisSize * 2)
                                                    while (xAxisBuildingSizeList[count - 1] != maxBuildingXAxisSize)
                                                    {
                                                        xAxisBuildingSizeList[count - 1] += 1;
                                                        xAxisSizeRemaining -= 1;
                                                        if (xAxisSizeRemaining <= maxBuildingXAxisSize || xAxisSizeRemaining - passWidth >= minBuildingXAxisSize * 2)
                                                        {
                                                            buildingSize = minBuildingXAxisSize;
                                                            break;
                                                        }
                                                    }
                                            }
                                            else
                                            {
                                                xAxisSizeRemaining = 0;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    xAxisBuildingSizeList.Add(buildingSize);
                                    xAxisSizeRemaining -= passWidth + buildingSize;
                                    buildingSize = minBuildingXAxisSize;
                                    iteration = 0;
                                }
                            }
                        }
                        Debug.Log("remaining " + xAxisSizeRemaining);
                        Debug.Log("Size " + buildingSize);
                        iteration++;
                        if (iteration == maxBuildingResizeIterations)
                        {
                            Debug.LogWarning("Iteration limit reached");
                            break;
                        }
                        if (xAxisSizeRemaining == 0) break;
                    }
                    var currentX = startingX + buildingXAxisSize + passWidth;
                    for (int i = 0; i < xAxisBuildingSizeList.Count; i++)
                    {
                        SpawnBuildingBasement(startingZ, currentX, buildingZAxisSize, xAxisBuildingSizeList[i]);
                        yield return null;
                        SpawnBuildingBasement(startingZ + quadZAxisSize - buildingZAxisSize, currentX, buildingZAxisSize, xAxisBuildingSizeList[i]);
                        yield return null;
                        currentX += xAxisBuildingSizeList[i] + passWidth;
                    }
                }
            }
            if (zAxisSizeRemaining >= minBuildingZAxisSize)
            {
                if (zAxisSizeRemaining <= maxBuildingZAxisSize)
                {
                    SpawnBuildingBasement(startingZ + buildingZAxisSize + passWidth, startingX, zAxisSizeRemaining, buildingXAxisSize);
                    yield return null;
                    SpawnBuildingBasement(startingZ + buildingZAxisSize + passWidth, startingX + buildingXAxisSize + passWidth * 2 + yardXAxisSize, zAxisSizeRemaining, buildingXAxisSize);
                    yield return null;
                }
                else
                {
                    List<int> zAxisBuildingSizeList = new List<int>();
                    int iteration = 0;
                    var buildingSize = minBuildingZAxisSize;
                    while (true)
                    {
                        if (zAxisSizeRemaining <= maxBuildingZAxisSize && zAxisSizeRemaining >= minBuildingZAxisSize)
                        {
                            zAxisBuildingSizeList.Add(zAxisSizeRemaining);
                            zAxisSizeRemaining = 0;
                        }
                        else
                        {
                            bool whatToIncrement = (Random.Range(0, 4) == 1) ? true : false;
                            if (!whatToIncrement && buildingSize != maxBuildingZAxisSize)
                            {
                                buildingSize++;
                            }
                            else
                            {
                                if (zAxisSizeRemaining - passWidth - buildingSize < minBuildingZAxisSize)
                                {
                                    var sizeToComplete = zAxisSizeRemaining - buildingSize;
                                    if (sizeToComplete + buildingSize <= maxBuildingZAxisSize)
                                    {
                                        buildingSize += sizeToComplete;
                                        zAxisBuildingSizeList.Add(buildingSize);
                                        zAxisSizeRemaining = 0;
                                    }
                                    else
                                    {
                                        sizeToComplete -= passWidth;
                                        if (buildingSize - (minBuildingZAxisSize - sizeToComplete) >= minBuildingZAxisSize)
                                        {
                                            buildingSize -= (minBuildingZAxisSize - sizeToComplete);
                                            zAxisBuildingSizeList.Add(buildingSize);
                                            zAxisBuildingSizeList.Add(zAxisSizeRemaining - passWidth - buildingSize);
                                            zAxisSizeRemaining = 0;
                                        }
                                        else
                                        {
                                            var count = zAxisBuildingSizeList.Count();
                                            if (count != 0)
                                            {
                                                while (zAxisBuildingSizeList[count - 1] != minBuildingZAxisSize)
                                                {
                                                    zAxisBuildingSizeList[count - 1] -= 1;
                                                    zAxisSizeRemaining += 1;
                                                    if (zAxisSizeRemaining <= maxBuildingZAxisSize || zAxisSizeRemaining - passWidth >= minBuildingZAxisSize * 2)
                                                    {
                                                        buildingSize = minBuildingZAxisSize;
                                                        break;
                                                    }

                                                }
                                                if (zAxisSizeRemaining > maxBuildingZAxisSize || zAxisSizeRemaining - passWidth < minBuildingZAxisSize * 2)
                                                    while (zAxisBuildingSizeList[count - 1] != maxBuildingZAxisSize)
                                                    {
                                                        zAxisBuildingSizeList[count - 1] += 1;
                                                        zAxisSizeRemaining -= 1;
                                                        if (zAxisSizeRemaining <= maxBuildingZAxisSize || zAxisSizeRemaining - passWidth >= minBuildingZAxisSize * 2)
                                                        {
                                                            buildingSize = minBuildingZAxisSize;
                                                            break;
                                                        }
                                                    }
                                            }
                                            else
                                            {
                                                zAxisSizeRemaining = 0;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    zAxisBuildingSizeList.Add(buildingSize);
                                    zAxisSizeRemaining -= passWidth + buildingSize;
                                    buildingSize = minBuildingZAxisSize;
                                    iteration = 0;
                                }
                            }
                        }
                        Debug.Log("remaining " + zAxisSizeRemaining);
                        Debug.Log("Size " + buildingSize);
                        iteration++;
                        if (iteration == maxBuildingResizeIterations)
                        {
                            Debug.LogWarning("Iteration limit reached");
                            break;
                        }
                        if (zAxisSizeRemaining == 0) break;
                    }
                    var currentZ = startingZ + buildingZAxisSize + passWidth;
                    for (int i = 0; i < zAxisBuildingSizeList.Count; i++)
                    {
                        SpawnBuildingBasement(currentZ, startingX, zAxisBuildingSizeList[i], buildingXAxisSize);
                        yield return null;
                        SpawnBuildingBasement(currentZ, startingX + quadXAxisSize - buildingXAxisSize, zAxisBuildingSizeList[i], buildingXAxisSize);
                        yield return null;
                        currentZ += zAxisBuildingSizeList[i] + passWidth;
                    }
                }
            }
        }
        else
        {
            if (quadZAxisSize > quadXAxisSize)
            {
                var zAxisBuildingSizeList = new List<int>();

                for (int x = startingX; x < startingX + quadXAxisSize;)
                {
                    int xAxisBuildingSize = UnityEngine.Random.Range(minBuildingXAxisSize, Mathf.Min(startingX + quadXAxisSize - x, maxBuildingXAxisSize));
                    for (int z = startingZ, currentBuilding = 0; z < startingZ + quadZAxisSize; currentBuilding++)
                    {
                        if (startingZ + quadZAxisSize - z < minBuildingZAxisSize) break;
                        int zAxisBuildingSize = 0;
                        if (x == startingX)
                            zAxisBuildingSize = UnityEngine.Random.Range(minBuildingZAxisSize, Mathf.Min(startingZ + quadZAxisSize - z, maxBuildingZAxisSize));
                        else
                            zAxisBuildingSize = zAxisBuildingSizeList[currentBuilding];
                        SpawnBuildingBasement(z, x, zAxisBuildingSize, xAxisBuildingSize);
                        yield return null;
                        if (x == startingX)
                            zAxisBuildingSizeList.Add(zAxisBuildingSize);
                        if (x == startingX)
                            if (z + zAxisBuildingSize + passWidth < startingZ + quadZAxisSize)
                            {
                                for (int i = 0; i < quadXAxisSize; i++)
                                {
                                    LevelMap[z + zAxisBuildingSize + passWidth - 2, x + i].MaterialNumber = 2;
                                }
                            }
                        z = z + zAxisBuildingSize + passWidth;
                    }
                    var xAxisSizeRemaining = startingX + quadXAxisSize - (x + xAxisBuildingSize);
                    if (xAxisSizeRemaining < passWidth) break;
                    for (int z = startingZ; z < startingZ + quadZAxisSize; z++)
                    {
                        LevelMap[z, x + xAxisBuildingSize + passWidth - 2].MaterialNumber = 2;
                    }
                    if (xAxisSizeRemaining < passWidth + minBuildingXAxisSize)
                    {
                        break;
                    }
                    x = x + xAxisBuildingSize + passWidth;
                }
            }
            else
            {
                var xAxisBuildingSizeList = new List<int>();

                for (int z = startingZ; z < startingZ + quadZAxisSize;)
                {
                    int zAxisBuildingSize = UnityEngine.Random.Range(minBuildingZAxisSize, Mathf.Min(startingZ + quadZAxisSize - z, maxBuildingZAxisSize));
                    for (int x = startingX, currentBuilding = 0; x < startingX + quadXAxisSize; currentBuilding++)
                    {
                        if (startingX + quadXAxisSize - x < minBuildingXAxisSize) break;
                        int xAxisBuildingSize = 0;
                        if (z == startingZ)
                            xAxisBuildingSize = UnityEngine.Random.Range(minBuildingXAxisSize, Mathf.Min(startingX + quadXAxisSize - x, maxBuildingXAxisSize));
                        else
                            xAxisBuildingSize = xAxisBuildingSizeList[currentBuilding];
                        yield return null;
                        SpawnBuildingBasement(z, x, zAxisBuildingSize, xAxisBuildingSize);
                        if (z == startingZ)
                            xAxisBuildingSizeList.Add(xAxisBuildingSize);
                        if (z == startingZ)
                            if (x + xAxisBuildingSize + passWidth < startingX + quadXAxisSize)
                            {
                                for (int i = 0; i < quadZAxisSize; i++)
                                {
                                    LevelMap[z + i, x + xAxisBuildingSize + passWidth - 2].MaterialNumber = 2;
                                }

                            }
                        x = x + xAxisBuildingSize + passWidth;
                    }
                    var zAxisSizeRemaining = startingZ + quadZAxisSize - (z + zAxisBuildingSize);
                    if (zAxisSizeRemaining < passWidth) break;
                    for (int x = startingX; x < startingX + quadXAxisSize; x++)
                    {
                        LevelMap[z + zAxisBuildingSize + passWidth - 2, x].MaterialNumber = 2;
                    }
                    if (zAxisSizeRemaining < passWidth + minBuildingXAxisSize)
                    {
                        break;
                    }
                    z = z + zAxisBuildingSize + passWidth;
                }
            }
        }
    }
    void SpawnBuildingBasement(int startingZ, int startingX, int zAxisSize, int xAxisSize)
    {
        var buildingParent = new GameObject($"Building{startingZ}{startingX}");
        buildingParent.isStatic = true;
        buildingParent.AddComponent<MeshFilter>();
        buildingParent.AddComponent<MeshRenderer>();

        var maxY = Mathf.Max(
            LevelMap[startingZ, startingX].y,
            LevelMap[startingZ, startingX + xAxisSize].y,
            LevelMap[startingZ + zAxisSize, startingX].y,
            LevelMap[startingZ + zAxisSize, startingX + xAxisSize].y);
        var minY = Mathf.Min(
            LevelMap[startingZ, startingX].y,
            LevelMap[startingZ, startingX + xAxisSize].y,
            LevelMap[startingZ + zAxisSize, startingX].y,
            LevelMap[startingZ + zAxisSize, startingX + xAxisSize].y);
        float y = (maxY + minY) / 2f;
        Building building;
        var buildingFaceZAxis = zAxisSize < xAxisSize ? true : false;
        if (buildingFaceZAxis)
        {
            if (xAxisSize % 2 == 0)
            {
                var evenBuildings = BuildingTypes.Where(r => r.IsEven == true).ToList();
                building = evenBuildings[Random.Range(0, evenBuildings.Count)];
            }
            else
            {
                var oddBuildings = BuildingTypes.Where(r => r.IsEven == false).ToList();
                building = oddBuildings[Random.Range(0, oddBuildings.Count)];
            }
        }
        else
        {
            if (zAxisSize % 2 == 0)
            {
                var evenBuildings = BuildingTypes.Where(r => r.IsEven == true).ToList();
                building = evenBuildings[Random.Range(0, evenBuildings.Count)];
            }
            else
            {
                var oddBuildings = BuildingTypes.Where(r => r.IsEven == false).ToList();
                building = oddBuildings[Random.Range(0, oddBuildings.Count)];
            }
        }

        for (int z = startingZ; z < startingZ + zAxisSize; z++)
        {
            for (int x = startingX; x < startingX + xAxisSize; x++)
            {
                GameObject part = null;
                if (z == startingZ)
                {
                    if (x == startingX)
                    {
                        part = Instantiate(building.CellingCornerPart, new Vector3(x + 0.5f, y, z + 0.5f), Quaternion.Euler(0f, 180f, 0f));
                    }
                    else if (x == startingX + xAxisSize - 1)
                    {
                        part = Instantiate(building.CellingCornerPart, new Vector3(x + 0.5f, y, z + 0.5f), Quaternion.Euler(0f, 90f, 0f));
                    }
                    else
                    {
                        part = Instantiate(building.CellingWallPart, new Vector3(x + 0.5f, y, z + 0.5f), Quaternion.Euler(0f, 90f, 0f));
                    }
                }
                else if (z == startingZ + zAxisSize - 1)
                {
                    if (x == startingX)
                    {
                        part = Instantiate(building.CellingCornerPart, new Vector3(x + 0.5f, y, z + 0.5f), Quaternion.Euler(0f, -90f, 0f));
                    }
                    else if (x == startingX + xAxisSize - 1)
                    {
                        part = Instantiate(building.CellingCornerPart, new Vector3(x + 0.5f, y, z + 0.5f), Quaternion.identity);
                    }
                    else
                    {
                        part = Instantiate(building.CellingWallPart, new Vector3(x + 0.5f, y, z + 0.5f), UnityEngine.Quaternion.Euler(0f, -90f, 0f));
                    }
                }
                else
                {
                    if (x == startingX)
                    {
                        part = Instantiate(building.CellingWallPart, new Vector3(x + 0.5f, y, z + 0.5f), Quaternion.Euler(0f, 180f, 0f));
                    }
                    else if (x == startingX + xAxisSize - 1)
                    {
                        part = Instantiate(building.CellingWallPart, new Vector3(x + 0.5f, y, z + 0.5f), Quaternion.Euler(0f, 0f, 0f));
                    }
                    else { }
                }
                if (part != null)
                    part.transform.SetParent(buildingParent.transform);
            }
        }
        SpawnBuildingStructure(startingZ, startingX, y + 1, zAxisSize, xAxisSize, building, buildingParent);
    }
    void UpdateLevelMap(LevelMapPoint[, ] levelMap, int startingZ, int startingX, int quadWidth, int quadLength)
    {
        for (int z = 0; z < quadLength; z++)
        {
            for (int x = 0; x < quadWidth; x++)
            {
                levelMap[startingZ + z, startingX + x].WasHandledByBuildingGenerator = true;
            }
        }
    }
    void SpawnBuildingStructure(int startingZ, int startingX, float startingY, int zAxisSize, int xAxisSize, Building building, GameObject buildingParent)
    {
        var floorCount = Random.Range(building.MinFloorCount, building.MaxFloorCount + 1);
        float y = startingY;
        for (int i = 0; i < floorCount; i++, y++)
        {
            for (int z = startingZ; z < startingZ + zAxisSize; z++)
            {
                for (int x = startingX; x < startingX + xAxisSize; x++)
                {
                    GameObject part = null;
                    if (z == startingZ)
                    {
                        if (x == startingX)
                        {
                            part = Instantiate(building.CornerPart, new Vector3(x + 0.5f, y, z + 0.5f), Quaternion.Euler(0f, 180f, 0f));
                        }
                        else if (x == startingX + xAxisSize - 1)
                        {
                            part = Instantiate(building.CornerPart, new Vector3(x + 0.5f, y, z + 0.5f), Quaternion.Euler(0f, 90f, 0f));
                        }
                        else
                        {
                            if (x % 2 == 0)
                                part = Instantiate(building.WallPart, new Vector3(x + 0.5f, y, z + 0.5f), Quaternion.Euler(0f, 90f, 0f));
                            else
                                part = Instantiate(building.WindowPart, new Vector3(x + 0.5f, y, z + 0.5f), Quaternion.Euler(0f, 90f, 0f));
                        }
                    }
                    else if (z == startingZ + zAxisSize - 1)
                    {
                        if (x == startingX)
                        {
                            part = Instantiate(building.CornerPart, new Vector3(x + 0.5f, y, z + 0.5f), Quaternion.Euler(0f, -90f, 0f));
                        }
                        else if (x == startingX + xAxisSize - 1)
                        {
                            part = Instantiate(building.CornerPart, new Vector3(x + 0.5f, y, z + 0.5f), Quaternion.identity);
                        }
                        else
                        {
                            if (x % 2 == 0)
                                part = Instantiate(building.WallPart, new Vector3(x + 0.5f, y, z + 0.5f), UnityEngine.Quaternion.Euler(0f, -90f, 0f));
                            else
                                part = Instantiate(building.WindowPart, new Vector3(x + 0.5f, y, z + 0.5f), UnityEngine.Quaternion.Euler(0f, -90f, 0f));
                        }
                    }
                    else
                    {
                        if (x == startingX)
                        {
                            if (z % 2 == 0)
                                part = Instantiate(building.WallPart, new Vector3(x + 0.5f, y, z + 0.5f), Quaternion.Euler(0f, 180f, 0f));
                            else
                                part = Instantiate(building.WindowPart, new Vector3(x + 0.5f, y, z + 0.5f), Quaternion.Euler(0f, 180f, 0f));
                        }
                        else if (x == startingX + xAxisSize - 1)
                        {
                            if (z % 2 == 0)
                                part = Instantiate(building.WallPart, new Vector3(x + 0.5f, y, z + 0.5f), Quaternion.Euler(0f, 0f, 0f));
                            else
                                part = Instantiate(building.WindowPart, new Vector3(x + 0.5f, y, z + 0.5f), Quaternion.Euler(0f, 0f, 0f));
                        }
                        else
                        {
                            part = Instantiate(building.EmptyPart, new Vector3(x + 0.5f, y, z + 0.5f), Quaternion.Euler(0f, 0f, 0f));
                        }
                    }
                    if (part != null)
                        part.transform.SetParent(buildingParent.transform);
                }
            }
        }
        CombineMesh(buildingParent, building);
    }

    void CombineMesh(GameObject buildingMainParent, Building building)
    {
        MeshFilter[] meshFilters = buildingMainParent.GetComponentsInChildren<MeshFilter>();
        List<MeshFilter>[] materialSortedMeshFilters = new List<MeshFilter>[4] { new List<MeshFilter>(), new List<MeshFilter>(), new List<MeshFilter>(), new List<MeshFilter>() };
        for (int i = 0; i < meshFilters.Length; i++)
        {
            if (meshFilters[i].gameObject.GetComponent<MeshRenderer>().material.name == building.Materials[0].name + " (Instance)")
            {
                materialSortedMeshFilters[0].Add(meshFilters[i]);
            }
            else if (meshFilters[i].gameObject.GetComponent<MeshRenderer>().material.name == building.Materials[1].name + " (Instance)")
            {
                materialSortedMeshFilters[1].Add(meshFilters[i]);
            }
            else if (meshFilters[i].gameObject.GetComponent<MeshRenderer>().material.name == building.Materials[2].name + " (Instance)")
            {
                materialSortedMeshFilters[2].Add(meshFilters[i]);
            }
            else
            {
                materialSortedMeshFilters[3].Add(meshFilters[i]);
            }
        }
        Debug.Log(meshFilters.Length);
        for (int i = 0; i < building.Materials.Count(); i++)
        {
            var buildingParent = new GameObject($"{buildingMainParent.name} sibling{i}");
            buildingParent.AddComponent<MeshRenderer>();
            buildingParent.AddComponent<MeshFilter>();
            List<CombineInstance> combineList = new List<CombineInstance>();
            //var filters = meshFilters.Where(r => r.gameObject.GetComponent<MeshRenderer>().material.name == building.Materials[i].name + " (Instance)").ToArray();
            var filters = materialSortedMeshFilters[i];
            Debug.Log(filters.Count);
            for (int j = 0; j < filters.Count; j++)
            {
                if (filters[j].sharedMesh != null)
                {
                    Debug.Log("Inside");
                    CombineInstance combine = new CombineInstance();
                    combine.mesh = filters[j].sharedMesh;
                    combine.transform = filters[j].transform.localToWorldMatrix;
                    filters[j].gameObject.SetActive(false);
                    combineList.Add(combine);
                }
            }
            buildingParent.transform.GetComponent<MeshFilter>().mesh = new Mesh();
            buildingParent.transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combineList.ToArray());
            buildingParent.transform.gameObject.SetActive(true);
            buildingParent.GetComponent<MeshRenderer>().material = building.Materials[i];
            buildingParent.isStatic = true;
        }
    }
}