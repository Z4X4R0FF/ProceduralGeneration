using System.Collections.Generic;
using UnityEngine;

public static class StreetGenerator
{
    public static LevelMapPoint[,] GenerateStreetMap(
        int levelZLength,
        int levelXLength,
        List<StreetsToGenerate> streetsAlongX,
        List<StreetsToGenerate> streetsAlongZ)
    {
        var levelMap = new LevelMapPoint[levelZLength, levelXLength];
        for (var i = 0; i < levelZLength; i++)
        for (var j = 0; j < levelXLength; j++)
            levelMap[i, j] = new LevelMapPoint(false, false, false, false);

        for (var i = 0; i < streetsAlongZ.Count; i++)
            GenerateStreet(streetsAlongZ[i], false, levelZLength, levelXLength, levelMap);
        for (var i = 0; i < streetsAlongX.Count; i++)
            GenerateStreet(streetsAlongX[i], true, levelZLength, levelXLength, levelMap);
        return levelMap;
    }

    private static void GenerateStreet(
        StreetsToGenerate street,
        bool isAlongX,
        int levelZLength,
        int levelXLength,
        LevelMapPoint[,] levelMap)
    {
        for (var i = 0; i < street.count; i++)
            if (isAlongX)
            {
                var acceptedValues = GetAcceptedMapVector(0, isAlongX, street.width, street.streetRadius, levelZLength,
                    levelXLength, levelMap);
                if (acceptedValues.Count == 0) break;
                var z = acceptedValues[Random.Range(0, acceptedValues.Count - 1)];

                for (var x = 0; x < levelXLength; x++)
                {
                    levelMap[z, x].IsStreetAlongXRadius = true;
                    levelMap[z, x].IsStreetAlongX = true;
                    if (levelMap[z, x].MaterialNumber == 0 || levelMap[z, x].MaterialNumber == 2)
                        levelMap[z, x].MaterialNumber = 2;
                    else
                        levelMap[z, x].MaterialNumber = 1;
                    for (var streetWidth = 1; streetWidth < street.width; streetWidth++)
                        if (streetWidth != street.width - 1)
                        {
                            levelMap[z + streetWidth, x].IsStreetAlongXRadius = true;
                            levelMap[z + streetWidth, x].IsStreetAlongX = true;
                            levelMap[z + streetWidth, x].MaterialNumber = 1;
                        }
                        else
                        {
                            levelMap[z + streetWidth, x].IsStreetAlongXRadius = true;
                            levelMap[z + streetWidth, x].IsStreetAlongX = true;
                            if (levelMap[z + streetWidth, x].MaterialNumber == 0 || levelMap[z, x].MaterialNumber == 2)
                                levelMap[z + streetWidth, x].MaterialNumber = 2;
                            else
                                levelMap[z + streetWidth, x].MaterialNumber = 1;
                            for (var streetRadiusSize = 1; streetRadiusSize <= street.streetRadius; streetRadiusSize++)
                            {
                                if (z - streetRadiusSize >= 0)
                                    levelMap[z - streetRadiusSize, x].IsStreetAlongXRadius = true;
                                if (z + streetWidth - 1 + streetRadiusSize < levelZLength)
                                    levelMap[z + streetWidth - 1 + streetRadiusSize, x].IsStreetAlongXRadius = true;
                            }
                        }
                }
            }
            else
            {
                var acceptedValues = GetAcceptedMapVector(0, isAlongX, street.width, street.streetRadius, levelZLength,
                    levelXLength, levelMap);
                if (acceptedValues.Count == 0) break;
                var x = acceptedValues[Random.Range(0, acceptedValues.Count - 1)];

                for (var z = 0; z < levelZLength; z++)
                {
                    levelMap[z, x].IsStreetAlongZRadius = true;
                    levelMap[z, x].IsStreetAlongZ = true;
                    if (levelMap[z, x].MaterialNumber == 0 || levelMap[z, x].MaterialNumber == 2)
                        levelMap[z, x].MaterialNumber = 2;
                    else
                        levelMap[z, x].MaterialNumber = 1;
                    for (var streetWidth = 1; streetWidth < street.width; streetWidth++)
                        if (streetWidth != street.width - 1)
                        {
                            levelMap[z, x + streetWidth].IsStreetAlongZRadius = true;
                            levelMap[z, x + streetWidth].IsStreetAlongZ = true;
                            levelMap[z, x + streetWidth].MaterialNumber = 1;
                        }
                        else
                        {
                            levelMap[z, x + streetWidth].IsStreetAlongZRadius = true;
                            levelMap[z, x + streetWidth].IsStreetAlongZ = true;
                            if (levelMap[z, x + streetWidth].MaterialNumber == 0 || levelMap[z, x].MaterialNumber == 2)
                                levelMap[z, x + streetWidth].MaterialNumber = 2;
                            else
                                levelMap[z, x + streetWidth].MaterialNumber = 1;
                            for (var streetRadiusSize = 1; streetRadiusSize <= street.streetRadius; streetRadiusSize++)
                            {
                                if (x - streetRadiusSize >= 0)
                                    levelMap[z, x - streetRadiusSize].IsStreetAlongZRadius = true;
                                if (x + streetWidth - 1 + streetRadiusSize < levelXLength)
                                    levelMap[z, x + streetWidth - 1 + streetRadiusSize].IsStreetAlongZRadius = true;
                            }
                        }
                }
            }
    }

    private static List<int> GetAcceptedMapVector(
        int oppositeEmptyValue,
        bool isAlongX,
        int streetWidth,
        int streetRadius,
        int levelZWidth,
        int levelXHeight,
        LevelMapPoint[,] levelMap)
    {
        var result = new List<int>();
        if (isAlongX)
        {
            for (var z = 1; z < levelZWidth - streetWidth - 1; z++)
                if (levelMap[z, oppositeEmptyValue].IsStreetAlongXRadius == false)
                {
                    var isValidForStreet = true;
                    for (var i = -streetRadius; i < streetWidth + streetRadius; i++)
                        if (z + i >= 0 && z + i <= levelZWidth - 1)
                            isValidForStreet = isValidForStreet && !levelMap[z + i, oppositeEmptyValue].IsStreetAlongX;
                    if (isValidForStreet)
                        result.Add(z);
                }
        }
        else
        {
            for (var x = 1; x < levelXHeight - streetWidth - 1; x++)
                if (levelMap[oppositeEmptyValue, x].IsStreetAlongZRadius == false)
                {
                    var isValidForStreet = true;
                    for (var i = -streetRadius; i < streetWidth + streetRadius; i++)
                        if (x + i >= 0 && x + i <= levelXHeight - 1)
                            isValidForStreet = isValidForStreet && !levelMap[oppositeEmptyValue, x + i].IsStreetAlongZ;
                    if (isValidForStreet)
                        result.Add(x);
                }
        }

        return result;
    }
}