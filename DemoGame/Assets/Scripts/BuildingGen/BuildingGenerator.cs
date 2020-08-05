using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class BuildingGenerator : MonoBehaviour
{
    private LevelMapPoint[,] _levelMap;

    [Header("Building Parts")] [SerializeField]
    private List<Building> buildingTypes = new List<Building>();

    [Header("Building Settings")] public int maxBuildingResizeIterations;
    public int maxBuildingXAxisSize;
    public int maxBuildingZAxisSize;
    public int minBuildingXAxisSize;
    public int minBuildingZAxisSize;
    public int minYardXAxisSize;
    [Header("Yard Settings")] public int minYardZAxisSize;
    public int passWidth;

    private static List<string> DefaultMaterialNames = new List<string>
    {
        "DefaultBasementPrimaryMaterial (Instance)",
        "DefaultBasementSecondaryMaterial (Instance)",
        "DefaultStructurePrimaryMaterial (Instance)",
        "DefaultStructureSecondaryMaterial (Instance)",
        "DefaultRoofMaterial (Instance)",
        "DefaultWindowMaterial (Instance)",
    };

    public IEnumerator MarkUpLevelArea(LevelMapPoint[,] levelMap)
    {
        _levelMap = levelMap;
        for (var z = 1; z < levelMap.GetLength(0) - 2; z++)
        for (var x = 1; x < levelMap.GetLength(1) - 2; x++)
        {
            if (levelMap[z, x].MaterialNumber != 0 || levelMap[z, x].WasHandledByBuildingGenerator) continue;
            var quadWidth = FindQuadDimensions(z, x, false);
            var quadLength = FindQuadDimensions(z, x, true);
            yield return StartCoroutine(HandleQuad(z, x, quadLength, quadWidth));
            UpdateLevelMap(levelMap, z, x, quadWidth, quadLength);
            yield return null;
        }
    }

    private int FindQuadDimensions(int z, int x, bool findLength)
    {
        var size = 1;
        if (findLength)
        {
            if (z == _levelMap.GetLength(0) - 3) return size;
            while (_levelMap[z + 1, x].MaterialNumber == 0)
            {
                size++;
                z++;
                if (z + 1 >= _levelMap.GetLength(0) - 2) break;
            }
        }
        else
        {
            if (x == _levelMap.GetLength(1) - 3) return size;
            while (_levelMap[z, x + 1].MaterialNumber == 0)
            {
                size++;
                x++;
                if (x + 1 >= _levelMap.GetLength(1) - 2) break;
            }
        }

        return size;
    }

    private IEnumerator HandleQuad(int startingZ, int startingX, int quadZAxisSize, int quadXAxisSize)
    {
        if (quadZAxisSize < minBuildingZAxisSize || quadXAxisSize < minBuildingXAxisSize)
        {
        }
        else if ((minBuildingZAxisSize + passWidth) * 2 + minYardZAxisSize < quadZAxisSize &&
                 (minBuildingXAxisSize + passWidth) * 2 + minYardXAxisSize < quadXAxisSize)
        {
            var buildingXAxisSize = minBuildingXAxisSize;
            var buildingZAxisSize = minBuildingZAxisSize;
            var yardZAxisSize = minYardZAxisSize;
            var yardXAxisSize = minYardXAxisSize;
            var totalXAxisSize = (buildingXAxisSize + passWidth) * 2 + yardXAxisSize;
            var totalZAxisSize = (buildingZAxisSize + passWidth) * 2 + yardZAxisSize;
            while (totalXAxisSize != quadXAxisSize)
            {
                if (totalXAxisSize + 2 > quadXAxisSize)
                {
                    yardXAxisSize++;
                }
                else
                {
                    var whatToIncrement = Random.Range(0, 2) == 1;
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
                    var whatToIncrement = Random.Range(0, 2) == 1;
                    if (whatToIncrement && buildingZAxisSize != maxBuildingZAxisSize) buildingZAxisSize++;
                    else yardZAxisSize++;
                }

                totalZAxisSize = yardZAxisSize + (buildingZAxisSize + passWidth) * 2;
            }

            yield return StartCoroutine(SpawnBuildingBasement(startingZ, startingX, buildingZAxisSize,
                buildingXAxisSize));
            yield return StartCoroutine(SpawnBuildingBasement(
                startingZ + buildingZAxisSize + yardZAxisSize + passWidth * 2, startingX, buildingZAxisSize,
                buildingXAxisSize));
            yield return StartCoroutine(SpawnBuildingBasement(startingZ,
                startingX + buildingXAxisSize + yardXAxisSize + passWidth * 2, buildingZAxisSize, buildingXAxisSize));
            yield return StartCoroutine(SpawnBuildingBasement(
                startingZ + buildingZAxisSize + yardZAxisSize + passWidth * 2,
                startingX + buildingXAxisSize + yardXAxisSize + passWidth * 2, buildingZAxisSize, buildingXAxisSize));
            for (var i = 0; i < quadXAxisSize; i++)
            {
                _levelMap[startingZ + buildingZAxisSize + passWidth - 2, startingX + i].MaterialNumber = 2;
                _levelMap[startingZ + buildingZAxisSize + yardZAxisSize + passWidth * 2 - 2, startingX + i]
                    .MaterialNumber = 2;
            }

            for (var i = 0; i < quadZAxisSize; i++)
            {
                _levelMap[startingZ + i, startingX + buildingXAxisSize + passWidth - 2].MaterialNumber = 2;
                _levelMap[startingZ + i, startingX + buildingXAxisSize + yardXAxisSize + passWidth * 2 - 2]
                    .MaterialNumber = 2;
            }

            var xAxisSizeRemaining = quadXAxisSize - (buildingXAxisSize + passWidth) * 2;
            var zAxisSizeRemaining = quadZAxisSize - (buildingZAxisSize + passWidth) * 2;
            if (xAxisSizeRemaining >= minBuildingXAxisSize)
            {
                if (xAxisSizeRemaining <= maxBuildingXAxisSize)
                {
                    yield return StartCoroutine(SpawnBuildingBasement(startingZ,
                        startingX + buildingXAxisSize + passWidth, buildingZAxisSize, xAxisSizeRemaining));
                    yield return StartCoroutine(SpawnBuildingBasement(
                        startingZ + buildingZAxisSize + passWidth * 2 + yardZAxisSize,
                        startingX + buildingXAxisSize + passWidth, buildingZAxisSize, xAxisSizeRemaining));
                }
                else
                {
                    var xAxisBuildingSizeList = new List<int>();
                    var iteration = 0;
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
                            var whatToIncrement = Random.Range(0, 4) == 1;
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
                                        if (buildingSize - (minBuildingXAxisSize - sizeToComplete) >=
                                            minBuildingXAxisSize)
                                        {
                                            buildingSize -= minBuildingXAxisSize - sizeToComplete;
                                            xAxisBuildingSizeList.Add(buildingSize);
                                            xAxisBuildingSizeList.Add(xAxisSizeRemaining - passWidth - buildingSize);
                                            xAxisSizeRemaining = 0;
                                        }
                                        else
                                        {
                                            var count = xAxisBuildingSizeList.Count;
                                            if (count != 0)
                                            {
                                                while (xAxisBuildingSizeList[count - 1] != minBuildingXAxisSize)
                                                {
                                                    xAxisBuildingSizeList[count - 1] -= 1;
                                                    xAxisSizeRemaining += 1;
                                                    if (xAxisSizeRemaining <= maxBuildingXAxisSize ||
                                                        xAxisSizeRemaining - passWidth >= minBuildingXAxisSize * 2)
                                                    {
                                                        buildingSize = minBuildingXAxisSize;
                                                        break;
                                                    }
                                                }

                                                if (xAxisSizeRemaining > maxBuildingXAxisSize ||
                                                    xAxisSizeRemaining - passWidth < minBuildingXAxisSize * 2)
                                                    while (xAxisBuildingSizeList[count - 1] != maxBuildingXAxisSize)
                                                    {
                                                        xAxisBuildingSizeList[count - 1] += 1;
                                                        xAxisSizeRemaining -= 1;
                                                        if (xAxisSizeRemaining <= maxBuildingXAxisSize ||
                                                            xAxisSizeRemaining - passWidth >= minBuildingXAxisSize * 2)
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

                        iteration++;
                        if (iteration == maxBuildingResizeIterations)
                        {
                            Debug.LogWarning("Iteration limit reached");
                            break;
                        }

                        if (xAxisSizeRemaining == 0) break;
                    }

                    var currentX = startingX + buildingXAxisSize + passWidth;
                    foreach (var t in xAxisBuildingSizeList)
                    {
                        yield return StartCoroutine(SpawnBuildingBasement(startingZ, currentX, buildingZAxisSize, t));
                        yield return StartCoroutine(SpawnBuildingBasement(startingZ + quadZAxisSize - buildingZAxisSize,
                            currentX, buildingZAxisSize, t));
                        currentX += t + passWidth;
                    }
                }
            }

            if (zAxisSizeRemaining >= minBuildingZAxisSize)
            {
                if (zAxisSizeRemaining <= maxBuildingZAxisSize)
                {
                    yield return StartCoroutine(SpawnBuildingBasement(startingZ + buildingZAxisSize + passWidth,
                        startingX, zAxisSizeRemaining, buildingXAxisSize));
                    yield return StartCoroutine(SpawnBuildingBasement(startingZ + buildingZAxisSize + passWidth,
                        startingX + buildingXAxisSize + passWidth * 2 + yardXAxisSize, zAxisSizeRemaining,
                        buildingXAxisSize));
                }
                else
                {
                    var zAxisBuildingSizeList = new List<int>();
                    var iteration = 0;
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
                            var whatToIncrement = Random.Range(0, 4) == 1;
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
                                        if (buildingSize - (minBuildingZAxisSize - sizeToComplete) >=
                                            minBuildingZAxisSize)
                                        {
                                            buildingSize -= minBuildingZAxisSize - sizeToComplete;
                                            zAxisBuildingSizeList.Add(buildingSize);
                                            zAxisBuildingSizeList.Add(zAxisSizeRemaining - passWidth - buildingSize);
                                            zAxisSizeRemaining = 0;
                                        }
                                        else
                                        {
                                            var count = zAxisBuildingSizeList.Count;
                                            if (count != 0)
                                            {
                                                while (zAxisBuildingSizeList[count - 1] != minBuildingZAxisSize)
                                                {
                                                    zAxisBuildingSizeList[count - 1] -= 1;
                                                    zAxisSizeRemaining += 1;
                                                    if (zAxisSizeRemaining <= maxBuildingZAxisSize ||
                                                        zAxisSizeRemaining - passWidth >= minBuildingZAxisSize * 2)
                                                    {
                                                        buildingSize = minBuildingZAxisSize;
                                                        break;
                                                    }
                                                }

                                                if (zAxisSizeRemaining > maxBuildingZAxisSize ||
                                                    zAxisSizeRemaining - passWidth < minBuildingZAxisSize * 2)
                                                    while (zAxisBuildingSizeList[count - 1] != maxBuildingZAxisSize)
                                                    {
                                                        zAxisBuildingSizeList[count - 1] += 1;
                                                        zAxisSizeRemaining -= 1;
                                                        if (zAxisSizeRemaining <= maxBuildingZAxisSize ||
                                                            zAxisSizeRemaining - passWidth >= minBuildingZAxisSize * 2)
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

                        iteration++;
                        if (iteration == maxBuildingResizeIterations)
                        {
                            Debug.LogWarning("Iteration limit reached");
                            break;
                        }

                        if (zAxisSizeRemaining == 0) break;
                    }

                    var currentZ = startingZ + buildingZAxisSize + passWidth;
                    foreach (var t in zAxisBuildingSizeList)
                    {
                        yield return StartCoroutine(SpawnBuildingBasement(currentZ, startingX, t, buildingXAxisSize));
                        yield return StartCoroutine(SpawnBuildingBasement(currentZ,
                            startingX + quadXAxisSize - buildingXAxisSize, t, buildingXAxisSize));
                        currentZ += t + passWidth;
                    }
                }
            }
        }
        else
        {
            if (quadZAxisSize > quadXAxisSize)
            {
                var zAxisBuildingSizeList = new List<int>();

                for (var x = startingX; x < startingX + quadXAxisSize;)
                {
                    var xAxisBuildingSize = Random.Range(minBuildingXAxisSize,
                        Mathf.Min(startingX + quadXAxisSize - x, maxBuildingXAxisSize));
                    for (int z = startingZ, currentBuilding = 0; z < startingZ + quadZAxisSize; currentBuilding++)
                    {
                        if (startingZ + quadZAxisSize - z < minBuildingZAxisSize) break;
                        int zAxisBuildingSize = x == startingX
                            ? Random.Range(minBuildingZAxisSize,
                                Mathf.Min(startingZ + quadZAxisSize - z, maxBuildingZAxisSize))
                            : zAxisBuildingSizeList[currentBuilding];
                        yield return StartCoroutine(SpawnBuildingBasement(z, x, zAxisBuildingSize, xAxisBuildingSize));
                        if (x == startingX)
                            zAxisBuildingSizeList.Add(zAxisBuildingSize);
                        if (x == startingX)
                            if (z + zAxisBuildingSize + passWidth < startingZ + quadZAxisSize)
                                for (var i = 0; i < quadXAxisSize; i++)
                                    _levelMap[z + zAxisBuildingSize + passWidth - 2, x + i].MaterialNumber = 2;
                        z = z + zAxisBuildingSize + passWidth;
                    }

                    var xAxisSizeRemaining = startingX + quadXAxisSize - (x + xAxisBuildingSize);
                    if (xAxisSizeRemaining < passWidth) break;
                    for (var z = startingZ; z < startingZ + quadZAxisSize; z++)
                        _levelMap[z, x + xAxisBuildingSize + passWidth - 2].MaterialNumber = 2;
                    if (xAxisSizeRemaining < passWidth + minBuildingXAxisSize) break;
                    x = x + xAxisBuildingSize + passWidth;
                }
            }
            else
            {
                var xAxisBuildingSizeList = new List<int>();

                for (var z = startingZ; z < startingZ + quadZAxisSize;)
                {
                    var zAxisBuildingSize = Random.Range(minBuildingZAxisSize,
                        Mathf.Min(startingZ + quadZAxisSize - z, maxBuildingZAxisSize));
                    for (int x = startingX, currentBuilding = 0; x < startingX + quadXAxisSize; currentBuilding++)
                    {
                        if (startingX + quadXAxisSize - x < minBuildingXAxisSize) break;
                        var xAxisBuildingSize = z == startingZ
                            ? Random.Range(minBuildingXAxisSize,
                                Mathf.Min(startingX + quadXAxisSize - x, maxBuildingXAxisSize))
                            : xAxisBuildingSizeList[currentBuilding];
                        yield return StartCoroutine(SpawnBuildingBasement(z, x, zAxisBuildingSize, xAxisBuildingSize));
                        if (z == startingZ)
                            xAxisBuildingSizeList.Add(xAxisBuildingSize);
                        if (z == startingZ)
                            if (x + xAxisBuildingSize + passWidth < startingX + quadXAxisSize)
                                for (var i = 0; i < quadZAxisSize; i++)
                                    _levelMap[z + i, x + xAxisBuildingSize + passWidth - 2].MaterialNumber = 2;
                        x = x + xAxisBuildingSize + passWidth;
                    }

                    var zAxisSizeRemaining = startingZ + quadZAxisSize - (z + zAxisBuildingSize);
                    if (zAxisSizeRemaining < passWidth) break;
                    for (var x = startingX; x < startingX + quadXAxisSize; x++)
                        _levelMap[z + zAxisBuildingSize + passWidth - 2, x].MaterialNumber = 2;
                    if (zAxisSizeRemaining < passWidth + minBuildingXAxisSize) break;
                    z = z + zAxisBuildingSize + passWidth;
                }
            }
        }
    }

    private IEnumerator SpawnBuildingBasement(int startingZ, int startingX, int zAxisSize, int xAxisSize)
    {
        var buildingParent = new GameObject($"Building{startingZ}{startingX}") {isStatic = true};
        buildingParent.AddComponent<MeshFilter>();
        buildingParent.AddComponent<MeshRenderer>();
        var buildingFaceZAxis = zAxisSize < xAxisSize;
        var buildingPorchPosition = SetPorchPosition(startingZ, startingX, zAxisSize, xAxisSize, buildingFaceZAxis);
        var minY = Mathf.Min(
            _levelMap[startingZ, startingX].Y,
            _levelMap[startingZ, startingX + xAxisSize].Y,
            _levelMap[startingZ + zAxisSize, startingX].Y,
            _levelMap[startingZ + zAxisSize, startingX + xAxisSize].Y);
        var y = _levelMap[buildingPorchPosition.Item2, buildingPorchPosition.Item1].Y - 0.1f; //maxY-1;
        Building building;

        if (buildingFaceZAxis)
        {
            if (xAxisSize % 2 == 0)
            {
                var evenBuildings = buildingTypes.Where(r => r.isEven && r.doSpawn).ToList();
                building = evenBuildings[Random.Range(0, evenBuildings.Count)];
            }
            else
            {
                var oddBuildings = buildingTypes.Where(r => !r.isEven && r.doSpawn).ToList();
                building = oddBuildings[Random.Range(0, oddBuildings.Count)];
            }
        }
        else
        {
            if (zAxisSize % 2 == 0)
            {
                var evenBuildings = buildingTypes.Where(r => r.isEven).ToList();
                building = evenBuildings[Random.Range(0, evenBuildings.Count)];
            }
            else
            {
                var oddBuildings = buildingTypes.Where(r => r.isEven == false).ToList();
                building = oddBuildings[Random.Range(0, oddBuildings.Count)];
            }
        }

        var cellingCount = y - minY + 1;
        var currentY = y;
        for (var i = 0; i < cellingCount; i++, currentY--)
        for (var z = startingZ; z < startingZ + zAxisSize; z++)
        for (var x = startingX; x < startingX + xAxisSize; x++)
        {
            if (i == 0)
            {
                if (buildingFaceZAxis)
                {
                    if (z == buildingPorchPosition.Item2)
                    {
                        if (x == buildingPorchPosition.Item1 - 1)
                        {
                            InstantiateBuildingPartAsChild(building.cellingCornerPart,
                                new Vector3(x + 0.5f, currentY, z + 0.5f),
                                Quaternion.Euler(0f, buildingPorchPosition.Item2 == startingZ ? 90f : 0f, 0f),
                                buildingParent);
                            continue;
                        }

                        if (x == buildingPorchPosition.Item1)
                        {
                            InstantiateBuildingPartAsChild(building.porchBase,
                                new Vector3(x + 0.5f, currentY, z + 0.5f),
                                Quaternion.Euler(0f, buildingPorchPosition.Item2 == startingZ ? 90f : 270f, 0f),
                                buildingParent);
                            continue;
                        }

                        if (x == buildingPorchPosition.Item1 + 1)
                        {
                            InstantiateBuildingPartAsChild(building.cellingCornerPart,
                                new Vector3(x + 0.5f, currentY, z + 0.5f),
                                Quaternion.Euler(0f, buildingPorchPosition.Item2 == startingZ ? 180f : 270f, 0f),
                                buildingParent);
                            continue;
                        }
                    }
                    else
                    {
                        if (x == buildingPorchPosition.Item1)
                        {
                            if (buildingPorchPosition.Item2 == startingZ)
                                if (z == startingZ + 1)
                                    continue;
                            if (buildingPorchPosition.Item2 == startingZ + zAxisSize)
                                if (z == startingZ + zAxisSize - 1)
                                    continue;
                        }
                    }
                }
                else
                {
                    if (x == buildingPorchPosition.Item1)
                    {
                        if (z == buildingPorchPosition.Item2 - 1)
                        {
                            InstantiateBuildingPartAsChild(building.cellingCornerPart,
                                new Vector3(x + 0.5f, currentY, z + 0.5f),
                                Quaternion.Euler(0f, buildingPorchPosition.Item1 == startingX ? 270f : 0f, 0f),
                                buildingParent);
                            continue;
                        }

                        if (z == buildingPorchPosition.Item2)
                        {
                            InstantiateBuildingPartAsChild(building.porchBase,
                                new Vector3(x + 0.5f, currentY, z + 0.5f),
                                Quaternion.Euler(0f, buildingPorchPosition.Item1 == startingX ? 180f : 0f, 0f),
                                buildingParent);
                            continue;
                        }

                        if (z == buildingPorchPosition.Item2 + 1)
                        {
                            InstantiateBuildingPartAsChild(building.cellingCornerPart,
                                new Vector3(x + 0.5f, currentY, z + 0.5f),
                                Quaternion.Euler(0f, buildingPorchPosition.Item1 == startingX ? 180f : 90f, 0f),
                                buildingParent);
                            continue;
                        }
                    }
                    else
                    {
                        if (z == buildingPorchPosition.Item2)
                        {
                            if (buildingPorchPosition.Item1 == startingX)
                                if (x == startingX + 1)
                                    continue;
                            if (buildingPorchPosition.Item1 == startingX + xAxisSize)
                                if (x == startingX + xAxisSize - 1)
                                    continue;
                        }
                    }
                }
            }

            if (z == startingZ)
            {
                if (x == startingX)
                    InstantiateBuildingPartAsChild(building.cellingCornerPart,
                        new Vector3(x + 0.5f, currentY, z + 0.5f),
                        Quaternion.Euler(0f, 180f, 0f), buildingParent);
                else if (x == startingX + xAxisSize - 1)
                    InstantiateBuildingPartAsChild(building.cellingCornerPart,
                        new Vector3(x + 0.5f, currentY, z + 0.5f),
                        Quaternion.Euler(0f, 90f, 0f), buildingParent);
                else
                    InstantiateBuildingPartAsChild(building.cellingWallPart, new Vector3(x + 0.5f, currentY, z + 0.5f),
                        Quaternion.Euler(0f, 90f, 0f), buildingParent);
            }
            else if (z == startingZ + zAxisSize - 1)
            {
                if (x == startingX)
                    InstantiateBuildingPartAsChild(building.cellingCornerPart,
                        new Vector3(x + 0.5f, currentY, z + 0.5f),
                        Quaternion.Euler(0f, -90f, 0f), buildingParent);
                else if (x == startingX + xAxisSize - 1)
                    InstantiateBuildingPartAsChild(building.cellingCornerPart,
                        new Vector3(x + 0.5f, currentY, z + 0.5f),
                        Quaternion.identity, buildingParent);
                else
                    InstantiateBuildingPartAsChild(building.cellingWallPart, new Vector3(x + 0.5f, currentY, z + 0.5f),
                        Quaternion.Euler(0f, -90f, 0f), buildingParent);
            }
            else
            {
                if (x == startingX)
                    InstantiateBuildingPartAsChild(building.cellingWallPart, new Vector3(x + 0.5f, currentY, z + 0.5f),
                        Quaternion.Euler(0f, 180f, 0f), buildingParent);
                else if (x == startingX + xAxisSize - 1)
                    InstantiateBuildingPartAsChild(building.cellingWallPart, new Vector3(x + 0.5f, currentY, z + 0.5f),
                        Quaternion.Euler(0f, 0f, 0f), buildingParent);
            }
        }

        yield return StartCoroutine(SpawnBuildingStructure(startingZ, startingX, y + 1, zAxisSize, xAxisSize, building,
            buildingParent, buildingFaceZAxis, buildingPorchPosition));
    }

    private static void UpdateLevelMap(LevelMapPoint[,] levelMap, int startingZ, int startingX, int quadWidth,
        int quadLength)
    {
        for (var z = 0; z < quadLength; z++)
        for (var x = 0; x < quadWidth; x++)
            levelMap[startingZ + z, startingX + x].WasHandledByBuildingGenerator = true;
    }

    private IEnumerator SpawnBuildingStructure(
        int startingZ,
        int startingX,
        float startingY,
        int zAxisSize,
        int xAxisSize,
        Building building,
        GameObject buildingParent,
        bool buildingFaceZAxis,
        Tuple<int, int> buildingPorchPosition)
    {
        var floorCount = Random.Range(building.minFloorCount, building.maxFloorCount + 1);
        var flatCounts = new int[floorCount];
        var flats = new List<Flat>[floorCount];
        var y = startingY;

        for (var floorNumber = 0; floorNumber < floorCount; floorNumber++)
        {
            var flatCount = Random.Range(building.minFlatCount, building.maxFlatCount + 1);
            flatCounts[floorNumber] = flatCount;
            flats[floorNumber] = new List<Flat>();
            for (var j = 0; j < flatCount; j++)
            {
                for (var i = 0; i < flatCounts[floorNumber]; i++)
                {
                    flats[floorNumber].Add(building.flats[Random.Range(0, building.flats.Count)]);
                }
            }
        }


        var flatMapList = FloorGenerator.LayoutFlats(startingZ, startingX, zAxisSize, xAxisSize,
            buildingFaceZAxis,
            floorCount, flatCounts,
            buildingPorchPosition);
        FloorGenerator.LayoutRooms(flatMapList, startingZ, startingX, zAxisSize, xAxisSize,
            buildingFaceZAxis,
            floorCount,
            buildingPorchPosition, flats);
        

        for (var floorNumber = 0; floorNumber < floorCount; floorNumber++, y++)
        {
            for (var z = startingZ; z < startingZ + zAxisSize; z++)
            for (var x = startingX; x < startingX + xAxisSize; x++)
            {
                if (buildingFaceZAxis)
                {
                    if (z == buildingPorchPosition.Item2)
                    {
                        if (x == buildingPorchPosition.Item1 - 1)
                        {
                            InstantiateBuildingPartAsChild(building.cornerPart, new Vector3(x + 0.5f, y, z + 0.5f),
                                Quaternion.Euler(0f, buildingPorchPosition.Item2 == startingZ ? 90f : 0f, 0f),
                                buildingParent);
                            continue;
                        }

                        if (x == buildingPorchPosition.Item1)
                        {
                            if (floorNumber != floorCount - 1)
                            {
                                InstantiateBuildingPartAsChild(building.porch,
                                    new Vector3(x + 0.5f, y + 0.5f, z + 0.5f),
                                    Quaternion.Euler(0f, buildingPorchPosition.Item2 == startingZ ? 90f : 270f, 0f),
                                    buildingParent);
                            }

                            continue;
                        }

                        if (x == buildingPorchPosition.Item1 + 1)
                        {
                            InstantiateBuildingPartAsChild(building.cornerPart, new Vector3(x + 0.5f, y, z + 0.5f),
                                Quaternion.Euler(0f, buildingPorchPosition.Item2 == startingZ ? 180f : 270f, 0f),
                                buildingParent);
                            continue;
                        }
                    }
                    else
                    {
                        if (buildingPorchPosition.Item2 == startingZ)
                        {
                            if (z == startingZ + 1)
                            {
                                if (x == buildingPorchPosition.Item1 - 1)
                                {
                                    InstantiateBuildingPartAsChild(
                                        flatCounts[floorNumber] != 1 ? building.doorFramePart : building.wallPart,
                                        new Vector3(x + 0.5f, y, z + 0.5f), Quaternion.Euler(0f, 0f, 0f),
                                        buildingParent);
                                    flatMapList[floorNumber].FloorMap[(z - startingZ) * xAxisSize + x - startingX]
                                        .WallXUp = WallTypes.DoorFrame;
                                    continue;
                                }

                                if (x == buildingPorchPosition.Item1) continue;

                                if (x == buildingPorchPosition.Item1 + 1)
                                {
                                    InstantiateBuildingPartAsChild(
                                        flatCounts[floorNumber] != 1 ? building.doorFramePart : building.wallPart,
                                        new Vector3(x + 0.5f, y, z + 0.5f), Quaternion.Euler(0f, 180f, 0f),
                                        buildingParent);
                                    flatMapList[floorNumber].FloorMap[(z - startingZ) * xAxisSize + x - startingX]
                                        .WallXDown = WallTypes.DoorFrame;
                                    continue;
                                }
                            }
                            else if (z == startingZ + 2)
                            {
                                if (x == buildingPorchPosition.Item1)
                                {
                                    InstantiateBuildingPartAsChild(
                                        flatCounts[floorNumber] == 2 ? building.wallPart : building.doorFramePart,
                                        new Vector3(x + 0.5f, y, z + 0.5f), Quaternion.Euler(0f, 90f, 0f),
                                        buildingParent);
                                    flatMapList[floorNumber].FloorMap[(z - startingZ) * xAxisSize + x - startingX]
                                        .WallZDown = flatCounts[floorNumber] == 2
                                        ? WallTypes.Wall
                                        : WallTypes.DoorFrame;
                                    continue;
                                }
                            }
                        }
                        else if (buildingPorchPosition.Item2 == startingZ + zAxisSize - 1)
                        {
                            if (z == startingZ + zAxisSize - 2)
                            {
                                if (x == buildingPorchPosition.Item1 - 1)
                                {
                                    InstantiateBuildingPartAsChild(
                                        flatCounts[floorNumber] != 1 ? building.doorFramePart : building.wallPart,
                                        new Vector3(x + 0.5f, y, z + 0.5f), Quaternion.Euler(0f, 0f, 0f),
                                        buildingParent);
                                    flatMapList[floorNumber].FloorMap[(z - startingZ) * xAxisSize + x - startingX]
                                        .WallXUp = flatCounts[floorNumber] != 1
                                        ? WallTypes.DoorFrame
                                        : WallTypes.Wall;
                                    continue;
                                }

                                if (x == buildingPorchPosition.Item1) continue;

                                if (x == buildingPorchPosition.Item1 + 1)
                                {
                                    InstantiateBuildingPartAsChild(
                                        flatCounts[floorNumber] != 1 ? building.doorFramePart : building.wallPart,
                                        new Vector3(x + 0.5f, y, z + 0.5f), Quaternion.Euler(0f, 180f, 0f),
                                        buildingParent);
                                    flatMapList[floorNumber].FloorMap[(z - startingZ) * xAxisSize + x - startingX]
                                        .WallXDown = flatCounts[floorNumber] != 1
                                        ? WallTypes.DoorFrame
                                        : WallTypes.Wall;
                                    continue;
                                }
                            }
                            else if (z == startingZ + zAxisSize - 3)
                            {
                                if (x == buildingPorchPosition.Item1)
                                {
                                    InstantiateBuildingPartAsChild(
                                        flatCounts[floorNumber] == 2 ? building.wallPart : building.doorFramePart,
                                        new Vector3(x + 0.5f, y, z + 0.5f), Quaternion.Euler(0f, 270f, 0f),
                                        buildingParent);
                                    flatMapList[floorNumber].FloorMap[(z - startingZ) * xAxisSize + x - startingX]
                                        .WallZUp = flatCounts[floorNumber] == 2
                                        ? WallTypes.Wall
                                        : WallTypes.DoorFrame;
                                    continue;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (x == buildingPorchPosition.Item1)
                    {
                        if (z == buildingPorchPosition.Item2 - 1)
                        {
                            InstantiateBuildingPartAsChild(building.cornerPart, new Vector3(x + 0.5f, y, z + 0.5f),
                                Quaternion.Euler(0f, buildingPorchPosition.Item1 == startingX ? 270f : 0f, 0f),
                                buildingParent);
                            continue;
                        }

                        if (z == buildingPorchPosition.Item2)
                        {
                            if (floorNumber != floorCount - 1)
                            {
                                InstantiateBuildingPartAsChild(building.porch,
                                    new Vector3(x + 0.5f, y + 0.5f, z + 0.5f),
                                    Quaternion.Euler(0f, buildingPorchPosition.Item1 == startingX ? 180f : 0f, 0f),
                                    buildingParent);
                            }

                            continue;
                        }

                        if (z == buildingPorchPosition.Item2 + 1)
                        {
                            InstantiateBuildingPartAsChild(building.cornerPart, new Vector3(x + 0.5f, y, z + 0.5f),
                                Quaternion.Euler(0f, buildingPorchPosition.Item1 == startingX ? 180f : 90f, 0f),
                                buildingParent);
                            continue;
                        }
                    }
                    else
                    {
                        if (buildingPorchPosition.Item1 == startingX)
                        {
                            if (x == startingX + 1)
                            {
                                if (z == buildingPorchPosition.Item2 - 1)
                                {
                                    InstantiateBuildingPartAsChild(
                                        flatCounts[floorNumber] != 1 ? building.doorFramePart : building.wallPart,
                                        new Vector3(x + 0.5f, y, z + 0.5f), Quaternion.Euler(0f, 270f, 0f),
                                        buildingParent);
                                    flatMapList[floorNumber].FloorMap[(z - startingZ) * xAxisSize + x - startingX]
                                        .WallZUp = flatCounts[floorNumber] != 1
                                        ? WallTypes.DoorFrame
                                        : WallTypes.Wall;
                                    continue;
                                }

                                if (z == buildingPorchPosition.Item2) continue;

                                if (z == buildingPorchPosition.Item2 + 1)
                                {
                                    InstantiateBuildingPartAsChild(
                                        flatCounts[floorNumber] != 1 ? building.doorFramePart : building.wallPart,
                                        new Vector3(x + 0.5f, y, z + 0.5f), Quaternion.Euler(0f, 90f, 0f),
                                        buildingParent);
                                    flatMapList[floorNumber].FloorMap[(z - startingZ) * xAxisSize + x - startingX]
                                        .WallZDown = flatCounts[floorNumber] != 1
                                        ? WallTypes.DoorFrame
                                        : WallTypes.Wall;
                                    continue;
                                }
                            }
                            else if (x == startingX + 2)
                            {
                                if (z == buildingPorchPosition.Item2)
                                {
                                    InstantiateBuildingPartAsChild(
                                        flatCounts[floorNumber] == 2 ? building.wallPart : building.doorFramePart,
                                        new Vector3(x + 0.5f, y, z + 0.5f), Quaternion.Euler(0f, 180f, 0f),
                                        buildingParent);
                                    flatMapList[floorNumber].FloorMap[(z - startingZ) * xAxisSize + x - startingX]
                                        .WallXDown = flatCounts[floorNumber] == 2
                                        ? WallTypes.Wall
                                        : WallTypes.DoorFrame;
                                    continue;
                                }
                            }
                        }
                        else if (buildingPorchPosition.Item1 == startingX + xAxisSize - 1)
                        {
                            if (x == startingX + xAxisSize - 2)
                            {
                                if (z == buildingPorchPosition.Item2 - 1)
                                {
                                    InstantiateBuildingPartAsChild(
                                        flatCounts[floorNumber] != 1 ? building.doorFramePart : building.wallPart,
                                        new Vector3(x + 0.5f, y, z + 0.5f), Quaternion.Euler(0f, 270f, 0f),
                                        buildingParent);
                                    flatMapList[floorNumber].FloorMap[(z - startingZ) * xAxisSize + x - startingX]
                                        .WallZUp = flatCounts[floorNumber] != 1
                                        ? WallTypes.DoorFrame
                                        : WallTypes.Wall;
                                    continue;
                                }

                                if (z == buildingPorchPosition.Item2)
                                {
                                    continue;
                                }

                                if (z == buildingPorchPosition.Item2 + 1)
                                {
                                    InstantiateBuildingPartAsChild(
                                        flatCounts[floorNumber] != 1 ? building.doorFramePart : building.wallPart,
                                        new Vector3(x + 0.5f, y, z + 0.5f), Quaternion.Euler(0f, 90f, 0f),
                                        buildingParent);
                                    flatMapList[floorNumber].FloorMap[(z - startingZ) * xAxisSize + x - startingX]
                                        .WallZDown = flatCounts[floorNumber] != 1
                                        ? WallTypes.DoorFrame
                                        : WallTypes.Wall;
                                    continue;
                                }
                            }
                            else if (x == startingX + xAxisSize - 3)
                            {
                                if (z == buildingPorchPosition.Item2)
                                {
                                    InstantiateBuildingPartAsChild(
                                        flatCounts[floorNumber] == 2 ? building.wallPart : building.doorFramePart,
                                        new Vector3(x + 0.5f, y, z + 0.5f), Quaternion.Euler(0f, 0f, 0f),
                                        buildingParent);
                                    flatMapList[floorNumber].FloorMap[(z - startingZ) * xAxisSize + x - startingX]
                                        .WallXUp = flatCounts[floorNumber] == 2
                                        ? WallTypes.Wall
                                        : WallTypes.DoorFrame;
                                    continue;
                                }
                            }
                        }
                    }
                }

                if (z == startingZ)
                {
                    if (x == startingX)
                    {
                        InstantiateBuildingPartAsChild(building.cornerPart, new Vector3(x + 0.5f, y, z + 0.5f),
                            Quaternion.Euler(0f, 180f, 0f), buildingParent);
                    }
                    else if (x == startingX + xAxisSize - 1)
                    {
                        InstantiateBuildingPartAsChild(building.cornerPart, new Vector3(x + 0.5f, y, z + 0.5f),
                            Quaternion.Euler(0f, 90f, 0f), buildingParent);
                    }
                    else
                    {
                        if (x - startingX < xAxisSize / 2)
                            InstantiateBuildingPartAsChild(
                                (x - startingX) % 2 == 1 ? building.windowPart : building.wallPart,
                                new Vector3(x + 0.5f, y, z + 0.5f), Quaternion.Euler(0f, 90f, 0f), buildingParent);
                        else
                            InstantiateBuildingPartAsChild(
                                (xAxisSize + startingX - x + 1) % 2 == 1 ? building.windowPart : building.wallPart,
                                new Vector3(x + 0.5f, y, z + 0.5f), Quaternion.Euler(0f, 90f, 0f), buildingParent);
                    }
                }
                else if (z == startingZ + zAxisSize - 1)
                {
                    if (x == startingX)
                    {
                        InstantiateBuildingPartAsChild(building.cornerPart, new Vector3(x + 0.5f, y, z + 0.5f),
                            Quaternion.Euler(0f, -90f, 0f), buildingParent);
                    }
                    else if (x == startingX + xAxisSize - 1)
                    {
                        InstantiateBuildingPartAsChild(building.cornerPart, new Vector3(x + 0.5f, y, z + 0.5f),
                            Quaternion.identity, buildingParent);
                    }
                    else
                    {
                        if (x - startingX < xAxisSize / 2)
                            InstantiateBuildingPartAsChild(
                                (x - startingX) % 2 == 1 ? building.windowPart : building.wallPart,
                                new Vector3(x + 0.5f, y, z + 0.5f), Quaternion.Euler(0f, -90f, 0f), buildingParent);
                        else
                            InstantiateBuildingPartAsChild(
                                (xAxisSize + startingX - x + 1) % 2 == 1 ? building.windowPart : building.wallPart,
                                new Vector3(x + 0.5f, y, z + 0.5f), Quaternion.Euler(0f, -90f, 0f), buildingParent);
                    }
                }
                else
                {
                    if (x == startingX)
                    {
                        if (z - startingZ < zAxisSize / 2)
                            InstantiateBuildingPartAsChild(
                                (z - startingZ) % 2 == 1 ? building.windowPart : building.wallPart,
                                new Vector3(x + 0.5f, y, z + 0.5f), Quaternion.Euler(0f, 180f, 0f), buildingParent);
                        else
                            InstantiateBuildingPartAsChild(
                                (zAxisSize + startingZ - z + 1) % 2 == 1 ? building.windowPart : building.wallPart,
                                new Vector3(x + 0.5f, y, z + 0.5f), Quaternion.Euler(0f, 180f, 0f), buildingParent);
                    }
                    else if (x == startingX + xAxisSize - 1)
                    {
                        if (z - startingZ < zAxisSize / 2)
                            InstantiateBuildingPartAsChild(
                                (z - startingZ) % 2 == 1 ? building.windowPart : building.wallPart,
                                new Vector3(x + 0.5f, y, z + 0.5f), Quaternion.Euler(0f, 0f, 0f), buildingParent);
                        else
                            InstantiateBuildingPartAsChild(
                                (zAxisSize + startingZ - z + 1) % 2 == 1 ? building.windowPart : building.wallPart,
                                new Vector3(x + 0.5f, y, z + 0.5f), Quaternion.Euler(0f, 0f, 0f), buildingParent);
                    }
                }
            }
        }
        SpawnInnerWalls(flatMapList, flats, building, buildingParent, zAxisSize, xAxisSize, startingX, startingY,
            startingZ);

        yield return StartCoroutine(SpawnBuildingRoof(startingZ, startingX, y, zAxisSize, xAxisSize, building,
            buildingParent, buildingFaceZAxis, buildingPorchPosition));
    }

    private static IEnumerator SpawnBuildingRoof(
        int startingZ,
        int startingX,
        float startingY,
        int zAxisSize,
        int xAxisSize,
        Building building,
        GameObject buildingParent,
        bool buildingFaceZAxis,
        Tuple<int, int> buildingPorchPosition)
    {
        var y = startingY;
        for (var z = startingZ; z < startingZ + zAxisSize; z++)
        for (var x = startingX; x < startingX + xAxisSize; x++)
        {
            var (buildingPorchPositionX, buildingPorchPositionZ) = buildingPorchPosition;
            if (buildingFaceZAxis)
            {
                if (z == buildingPorchPositionZ)
                {
                    if (x == buildingPorchPositionX)
                    {
                        InstantiateBuildingPartAsChild(building.roofPorchPart,
                            new Vector3(x + 0.5f, y + 0.5f, z + 0.5f),
                            Quaternion.Euler(0f, buildingPorchPositionZ == startingZ ? 90f : 270f, 0f), buildingParent);
                        continue;
                    }
                }
                else
                {
                    if (buildingPorchPositionZ == startingZ)
                    {
                        if (z == startingZ + 1)
                            if (x == buildingPorchPositionX)
                                continue;
                    }
                    else if (buildingPorchPositionZ == startingZ + zAxisSize - 1)
                    {
                        if (z == startingZ + zAxisSize - 2)
                            if (x == buildingPorchPositionX)
                                continue;
                    }
                }
            }
            else
            {
                if (x == buildingPorchPositionX)
                {
                    if (z == buildingPorchPositionZ)
                    {
                        InstantiateBuildingPartAsChild(building.roofPorchPart,
                            new Vector3(x + 0.5f, y + 0.5f, z + 0.5f),
                            Quaternion.Euler(0f, buildingPorchPositionX == startingX ? 180f : 0f, 0f), buildingParent);
                        continue;
                    }
                }
                else
                {
                    if (buildingPorchPositionX == startingX)
                    {
                        if (x == startingX + 1)
                            if (z == buildingPorchPositionZ)
                                continue;
                    }
                    else if (buildingPorchPositionX == startingX + xAxisSize - 1)
                    {
                        if (x == startingX + xAxisSize - 2)
                            if (z == buildingPorchPositionZ)
                                continue;
                    }
                }
            }

            if (z == startingZ)
            {
                if (x == startingX)
                    InstantiateBuildingPartAsChild(building.roofCornerPart, new Vector3(x + 0.5f, y, z + 0.5f),
                        Quaternion.Euler(0f, 90f, 0f), buildingParent);
                else if (x == startingX + xAxisSize - 1)
                    InstantiateBuildingPartAsChild(building.roofCornerPart, new Vector3(x + 0.5f, y, z + 0.5f),
                        Quaternion.Euler(0f, 0f, 0f), buildingParent);
                else
                    InstantiateBuildingPartAsChild(building.roofSidePart, new Vector3(x + 0.5f, y, z + 0.5f),
                        Quaternion.Euler(0f, 90f, 0f), buildingParent);
            }
            else if (z == startingZ + zAxisSize - 1)
            {
                if (x == startingX)
                    InstantiateBuildingPartAsChild(building.roofCornerPart, new Vector3(x + 0.5f, y, z + 0.5f),
                        Quaternion.Euler(0f, -180f, 0f), buildingParent);
                else if (x == startingX + xAxisSize - 1)
                    InstantiateBuildingPartAsChild(building.roofCornerPart, new Vector3(x + 0.5f, y, z + 0.5f),
                        Quaternion.Euler(0f, -90f, 0f), buildingParent);
                else
                    InstantiateBuildingPartAsChild(building.roofSidePart, new Vector3(x + 0.5f, y, z + 0.5f),
                        Quaternion.Euler(0f, -90f, 0f), buildingParent);
            }
            else
            {
                if (x == startingX)
                    InstantiateBuildingPartAsChild(building.roofSidePart, new Vector3(x + 0.5f, y, z + 0.5f),
                        Quaternion.Euler(0f, 180f, 0f), buildingParent);
                else if (x == startingX + xAxisSize - 1)
                    InstantiateBuildingPartAsChild(building.roofSidePart, new Vector3(x + 0.5f, y, z + 0.5f),
                        Quaternion.Euler(0f, 0f, 0f), buildingParent);
                else
                    InstantiateBuildingPartAsChild(building.roofTopPart, new Vector3(x + 0.5f, y, z + 0.5f),
                        Quaternion.Euler(0f, 0f, 0f), buildingParent);
            }
        }

        //yield return StartCoroutine(
        CombineBuildingStructureMesh(buildingParent, building);

        yield return null;
    }


    private static void SpawnInnerWalls(List<BuildingFloorInfo> flatMapList, List<Flat>[] flats,
        Building building,
        GameObject buildingParent,
        int zAxisSize, int xAxisSize,
        int startingX, float startingY, int startingZ)
    {
        var materialSortedBuildingParents = new Dictionary<Material, GameObject>();

        var y = startingY;

        for (var i = 0;
            i < building.FlatMaterialList.Count;
            i++)
        {
            var material = building.FlatMaterialList[i];
            if (!materialSortedBuildingParents.ContainsKey(material))
            {
                var materialParent = new GameObject($"Building({startingZ},{startingX}) {material}")
                    {isStatic = true};
                materialParent.AddComponent<MeshFilter>();
                materialParent.AddComponent<MeshRenderer>();
                materialSortedBuildingParents.Add(material, materialParent);
            }
        }

        for (var floorNumber = 0;
            floorNumber < flatMapList.Count;
            floorNumber++, y++)
        {
            for (var z = 0; z < zAxisSize; z++)
            for (var x = 0; x < xAxisSize; x++)
            {
                var flatMapPoint = flatMapList[floorNumber].FloorMap[z * xAxisSize + x];
                if (flatMapPoint.IsPorch) continue;

                switch (flatMapPoint.RoomType)
                {
                    case RoomTypes.LivingRoom:
                    {
                        var room = flats[floorNumber][flatMapPoint.FlatNumber - 1].livingRoom;
                        InstantiateBuildingPartAsChild(room.floor,
                            new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                            Quaternion.identity, materialSortedBuildingParents[room.floorMaterial]);
                        InstantiateBuildingPartAsChild(room.ceiling,
                            new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                            Quaternion.identity, materialSortedBuildingParents[room.ceilingMaterial]);
                        switch (flatMapPoint.WallXDown)
                        {
                            case WallTypes.Empty:
                                break;
                            case WallTypes.Wall:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.wall,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 180f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            case WallTypes.DoorFrame:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.doorFrame,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 180f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            case WallTypes.Window:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.windowFrame,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 180f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        switch (flatMapPoint.WallXUp)
                        {
                            case WallTypes.Empty:
                                break;
                            case WallTypes.Wall:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.wall,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 0f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            case WallTypes.DoorFrame:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.doorFrame,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 0f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            case WallTypes.Window:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.windowFrame,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 0f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        switch (flatMapPoint.WallZDown)
                        {
                            case WallTypes.Empty:
                                break;
                            case WallTypes.Wall:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.wall,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 90f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            case WallTypes.DoorFrame:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.doorFrame,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 90f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            case WallTypes.Window:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.windowFrame,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 90f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        switch (flatMapPoint.WallZUp)
                        {
                            case WallTypes.Empty:
                                break;
                            case WallTypes.Wall:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.wall,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 270f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            case WallTypes.DoorFrame:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.doorFrame,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 270f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            case WallTypes.Window:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.windowFrame,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 270f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        break;
                    }
                    case RoomTypes.BedRoom:
                    {
                        var room = flats[floorNumber][flatMapPoint.FlatNumber - 1].bedRoom;
                        InstantiateBuildingPartAsChild(room.floor,
                            new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                            Quaternion.identity, materialSortedBuildingParents[room.floorMaterial]);
                        InstantiateBuildingPartAsChild(room.ceiling,
                            new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                            Quaternion.identity, materialSortedBuildingParents[room.ceilingMaterial]);
                        switch (flatMapPoint.WallXDown)
                        {
                            case WallTypes.Empty:
                                break;
                            case WallTypes.Wall:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.wall,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 180f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            case WallTypes.DoorFrame:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.doorFrame,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 180f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            case WallTypes.Window:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.windowFrame,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 180f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        switch (flatMapPoint.WallXUp)
                        {
                            case WallTypes.Empty:
                                break;
                            case WallTypes.Wall:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.wall,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 0f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            case WallTypes.DoorFrame:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.doorFrame,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 0f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            case WallTypes.Window:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.windowFrame,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 0f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        switch (flatMapPoint.WallZDown)
                        {
                            case WallTypes.Empty:
                                break;
                            case WallTypes.Wall:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.wall,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 90f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            case WallTypes.DoorFrame:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.doorFrame,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 90f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            case WallTypes.Window:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.windowFrame,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 90f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        switch (flatMapPoint.WallZUp)
                        {
                            case WallTypes.Empty:
                                break;
                            case WallTypes.Wall:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.wall,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 270f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            case WallTypes.DoorFrame:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.doorFrame,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 270f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            case WallTypes.Window:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.windowFrame,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 270f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        break;
                    }
                    case RoomTypes.BathRoom:
                    {
                        var room = flats[floorNumber][flatMapPoint.FlatNumber - 1].bathRoom;
                        InstantiateBuildingPartAsChild(room.floor,
                            new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                            Quaternion.identity, materialSortedBuildingParents[room.floorMaterial]);
                        InstantiateBuildingPartAsChild(room.ceiling,
                            new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                            Quaternion.identity, materialSortedBuildingParents[room.ceilingMaterial]);
                        switch (flatMapPoint.WallXDown)
                        {
                            case WallTypes.Empty:
                                break;
                            case WallTypes.Wall:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.wall,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 180f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            case WallTypes.DoorFrame:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.doorFrame,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 180f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            case WallTypes.Window:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.windowFrame,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 180f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        switch (flatMapPoint.WallXUp)
                        {
                            case WallTypes.Empty:
                                break;
                            case WallTypes.Wall:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.wall,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 0f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            case WallTypes.DoorFrame:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.doorFrame,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 0f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            case WallTypes.Window:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.windowFrame,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 0f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        switch (flatMapPoint.WallZDown)
                        {
                            case WallTypes.Empty:
                                break;
                            case WallTypes.Wall:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.wall,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 90f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            case WallTypes.DoorFrame:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.doorFrame,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 90f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            case WallTypes.Window:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.windowFrame,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 90f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        switch (flatMapPoint.WallZUp)
                        {
                            case WallTypes.Empty:
                                break;
                            case WallTypes.Wall:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.wall,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 270f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            case WallTypes.DoorFrame:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.doorFrame,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 270f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            case WallTypes.Window:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.windowFrame,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 270f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        break;
                    }
                    case RoomTypes.StorageRoom:
                    {
                        var room = flats[floorNumber][flatMapPoint.FlatNumber - 1].storageRooms[
                            Random.Range(0, flats[floorNumber][flatMapPoint.FlatNumber - 1].storageRooms.Count)];
                        InstantiateBuildingPartAsChild(room.floor,
                            new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                            Quaternion.identity, materialSortedBuildingParents[room.floorMaterial]);
                        InstantiateBuildingPartAsChild(room.ceiling,
                            new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                            Quaternion.identity, materialSortedBuildingParents[room.ceilingMaterial]);
                        switch (flatMapPoint.WallXDown)
                        {
                            case WallTypes.Empty:
                                break;
                            case WallTypes.Wall:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.wall,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 180f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            case WallTypes.DoorFrame:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.doorFrame,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 180f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            case WallTypes.Window:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.windowFrame,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 180f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        switch (flatMapPoint.WallXUp)
                        {
                            case WallTypes.Empty:
                                break;
                            case WallTypes.Wall:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.wall,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 0f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            case WallTypes.DoorFrame:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.doorFrame,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 0f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            case WallTypes.Window:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.windowFrame,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 0f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        switch (flatMapPoint.WallZDown)
                        {
                            case WallTypes.Empty:
                                break;
                            case WallTypes.Wall:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.wall,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 90f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            case WallTypes.DoorFrame:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.doorFrame,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 90f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            case WallTypes.Window:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.windowFrame,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 90f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        switch (flatMapPoint.WallZUp)
                        {
                            case WallTypes.Empty:
                                break;
                            case WallTypes.Wall:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.wall,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 270f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            case WallTypes.DoorFrame:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.doorFrame,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 270f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            case WallTypes.Window:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.windowFrame,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 270f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        break;
                    }
                    case RoomTypes.Corridor:
                    {
                        var room = flats[floorNumber][flatMapPoint.FlatNumber - 1].corridor;
                        InstantiateBuildingPartAsChild(room.floor,
                            new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                            Quaternion.identity, materialSortedBuildingParents[room.floorMaterial]);
                        InstantiateBuildingPartAsChild(room.ceiling,
                            new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                            Quaternion.identity, materialSortedBuildingParents[room.ceilingMaterial]);
                        switch (flatMapPoint.WallXDown)
                        {
                            case WallTypes.Empty:
                                break;
                            case WallTypes.Wall:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.wall,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 180f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            case WallTypes.DoorFrame:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.doorFrame,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 180f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            case WallTypes.Window:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.windowFrame,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 180f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        switch (flatMapPoint.WallXUp)
                        {
                            case WallTypes.Empty:
                                break;
                            case WallTypes.Wall:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.wall,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 0f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            case WallTypes.DoorFrame:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.doorFrame,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 0f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            case WallTypes.Window:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.windowFrame,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 0f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        switch (flatMapPoint.WallZDown)
                        {
                            case WallTypes.Empty:
                                break;
                            case WallTypes.Wall:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.wall,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 90f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            case WallTypes.DoorFrame:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.doorFrame,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 90f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            case WallTypes.Window:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.windowFrame,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 90f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        switch (flatMapPoint.WallZUp)
                        {
                            case WallTypes.Empty:
                                break;
                            case WallTypes.Wall:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.wall,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 270f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            case WallTypes.DoorFrame:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.doorFrame,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 270f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            case WallTypes.Window:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.windowFrame,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 270f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        break;
                    }
                    case RoomTypes.Kitchen:
                    {
                        var room = flats[floorNumber][flatMapPoint.FlatNumber - 1].kitchen;
                        InstantiateBuildingPartAsChild(room.floor,
                            new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                            Quaternion.identity, materialSortedBuildingParents[room.floorMaterial]);
                        InstantiateBuildingPartAsChild(room.ceiling,
                            new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                            Quaternion.identity, materialSortedBuildingParents[room.ceilingMaterial]);
                        switch (flatMapPoint.WallXDown)
                        {
                            case WallTypes.Empty:
                                break;
                            case WallTypes.Wall:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.wall,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 180f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            case WallTypes.DoorFrame:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.doorFrame,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 180f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            case WallTypes.Window:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.windowFrame,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 180f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        switch (flatMapPoint.WallXUp)
                        {
                            case WallTypes.Empty:
                                break;
                            case WallTypes.Wall:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.wall,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 0f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            case WallTypes.DoorFrame:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.doorFrame,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 0f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            case WallTypes.Window:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.windowFrame,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 0f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        switch (flatMapPoint.WallZDown)
                        {
                            case WallTypes.Empty:
                                break;
                            case WallTypes.Wall:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.wall,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 90f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            case WallTypes.DoorFrame:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.doorFrame,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 90f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            case WallTypes.Window:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.windowFrame,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 90f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        switch (flatMapPoint.WallZUp)
                        {
                            case WallTypes.Empty:
                                break;
                            case WallTypes.Wall:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.wall,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 270f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            case WallTypes.DoorFrame:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.doorFrame,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 270f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            case WallTypes.Window:
                            {
                                InstantiateBuildingPartAsChild(
                                    room.windowFrame,
                                    new Vector3(startingX + x + 0.5f, y, startingZ + z + 0.5f),
                                    Quaternion.Euler(0f, 270f, 0f),
                                    materialSortedBuildingParents[room.wallMaterial]);
                                break;
                            }
                            default:
                                throw new ArgumentOutOfRangeException();
                        }

                        break;
                    }
                    case RoomTypes.EmptyRoom:
                    {
                        Debug.LogWarning("RoomType.Empty!");
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        foreach (var item in materialSortedBuildingParents)
        {
            CombineMesh(item.Value, item.Key);
        }
    }

    private static void CombineMesh(GameObject buildingParent, Material material)
    {
        var loaders = buildingParent.GetComponentsInChildren<ComponentLoader>().ToArray();
        var meshFilters = loaders.SelectMany(r => r.meshFilters).ToList();
        var meshRenderers = loaders.SelectMany(r => r.meshRenderers).ToArray();

        var buildingParentSibling = new GameObject($"{buildingParent.name} sibling{material.name}");
        buildingParentSibling.AddComponent<MeshRenderer>();
        buildingParentSibling.AddComponent<MeshFilter>();
        var combineList = new List<CombineInstance>();
        for (var index = 0; index < meshFilters.Count; index++)
        {
            var t = meshFilters[index];
            if (t.sharedMesh == null) continue;
            var combine = new CombineInstance {mesh = t.sharedMesh, transform = t.transform.localToWorldMatrix};
            t.gameObject.SetActive(false);
            combineList.Add(combine);
            //if (index % 75 == 0) yield return null;
        }

        var mesh = new Mesh {indexFormat = UnityEngine.Rendering.IndexFormat.UInt32};
        mesh.CombineMeshes(combineList.ToArray());
        mesh.RecalculateBounds();
        buildingParentSibling.transform.GetComponent<MeshFilter>().mesh = mesh;
        buildingParentSibling.transform.gameObject.SetActive(true);
        buildingParentSibling.GetComponent<MeshRenderer>().material = material;
        buildingParentSibling.isStatic = true;
    }

    private static void CombineBuildingStructureMesh(GameObject buildingMainParent, Building building)
    {
        var loaders = buildingMainParent.GetComponentsInChildren<ComponentLoader>().ToArray();
        var meshFilters = loaders.SelectMany(r => r.meshFilters).ToArray();
        var meshRenderers = loaders.SelectMany(r => r.meshRenderers).ToArray();

        var materialSortedMeshFilters = new Dictionary<string, List<MeshFilter>>();

        for (var i = 0;
            i < meshFilters.Length;
            i++)
        {
            var materialName = meshRenderers[i].material.name;
            if (materialSortedMeshFilters.ContainsKey(materialName))
                materialSortedMeshFilters[materialName].Add(meshFilters[i]);
            else
            {
                materialSortedMeshFilters.Add(materialName, new List<MeshFilter> {meshFilters[i]});
            }
        }

        for (var i = 0;
            i < DefaultMaterialNames.Count;
            i++)
        {
            var buildingParent = new GameObject($"{buildingMainParent.name} sibling{i}");
            buildingParent.AddComponent<MeshRenderer>();
            buildingParent.AddComponent<MeshFilter>();
            var combineList = new List<CombineInstance>();
            var filters = materialSortedMeshFilters[DefaultMaterialNames[i]];
            for (var index = 0; index < filters.Count; index++)
            {
                var t = filters[index];
                if (t.sharedMesh == null) continue;
                var combine = new CombineInstance
                    {mesh = t.sharedMesh, transform = t.transform.localToWorldMatrix};
                t.gameObject.SetActive(false);
                combineList.Add(combine);
                //if (index % 75 == 0) yield return null;
            }

            var mesh = new Mesh {indexFormat = UnityEngine.Rendering.IndexFormat.UInt32};
            mesh.CombineMeshes(combineList.ToArray());
            mesh.RecalculateBounds();
            buildingParent.transform.GetComponent<MeshFilter>().mesh = mesh;
            buildingParent.transform.gameObject.SetActive(true);
            buildingParent.GetComponent<MeshRenderer>().material = building.MaterialList[i];
            buildingParent.isStatic = true;
        }
    }

    private Tuple<int, int> SetPorchPosition(int startingZ, int startingX, int zAxisSize, int xAxisSize,
        bool buildingFaceZAxis)
    {
        int x, z;
        if (buildingFaceZAxis)
        {
            if (xAxisSize % 2 == 0)
                x = startingX + xAxisSize / 2 - 1;
            else
                x = startingX + xAxisSize / 2;
            if (startingZ < 10)
                z = startingZ + zAxisSize - 1;
            else if (startingZ > _levelMap.GetLength(0) - 10)
                z = startingZ;
            else
                z = _levelMap[startingZ, x].Y > _levelMap[startingZ + zAxisSize - 1, x].Y
                    ? startingZ
                    : startingZ + zAxisSize - 1;
        }

        else
        {
            if (zAxisSize % 2 == 0)
                z = startingZ + zAxisSize / 2 - 1;
            else
                z = startingZ + zAxisSize / 2;
            if (startingX < 10)
                x = startingX + xAxisSize - 1;
            else if (startingX > _levelMap.GetLength(1) - 10)
                x = startingX;
            else
                x = _levelMap[z, startingX].Y > _levelMap[z, startingX + xAxisSize - 1].Y
                    ? startingX
                    : startingX + xAxisSize - 1;
        }

        var position = new Tuple<int, int>(x, z);
        return position;
    }

    private static void InstantiateBuildingPartAsChild(GameObject gameObject, Vector3 position,
        Quaternion rotation,
        GameObject parent)
    {
        var part = Instantiate(gameObject, position,
            rotation);
        part.transform.SetParent(parent.transform);
    }
}