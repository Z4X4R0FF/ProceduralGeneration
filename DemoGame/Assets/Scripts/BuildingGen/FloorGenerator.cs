using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public static class FloorGenerator
{
    public static List<BuildingFloorInfo> LayoutFlats(int startingZ, int startingX, int zAxisSize,
        int xAxisSize,
        bool buildingFaceZAxis, int floorCount,
        int[] flatCounts, Tuple<int, int> buildingPorchPosition)
    {
        var floorInfoList = new List<BuildingFloorInfo>();
        for (var floorNumber = 0; floorNumber < floorCount; floorNumber++)
        {
            var floorInfo = new BuildingFloorInfo
            {
                FlatCount = flatCounts[floorNumber], AdditionalFlatRooms = new int[flatCounts[floorNumber]],
                FloorMap = new FlatMapPoint[zAxisSize * xAxisSize]
            };
            for (var j = 0; j < floorInfo.AdditionalFlatRooms.Length; j++)
            {
                floorInfo.AdditionalFlatRooms[j] = 0;
            }

            switch (flatCounts[floorNumber])
            {
                case 1:
                {
                    for (var z = 0; z < zAxisSize; z++)
                    for (var x = 0; x < xAxisSize; x++)
                    {
                        if (buildingFaceZAxis)
                        {
                            if (startingX + x == buildingPorchPosition.Item1)
                            {
                                if (startingZ + z == buildingPorchPosition.Item2 ||
                                    startingZ + z == buildingPorchPosition.Item2 + 1 ||
                                    startingZ + z == buildingPorchPosition.Item2 - 1)
                                {
                                    floorInfo.FloorMap[z * xAxisSize + x] = new FlatMapPoint {IsPorch = true};
                                }
                                else
                                {
                                    floorInfo.FloorMap[z * xAxisSize + x] = new FlatMapPoint {FlatNumber = 1};
                                }
                            }
                            else
                            {
                                floorInfo.FloorMap[z * xAxisSize + x] = new FlatMapPoint {FlatNumber = 1};
                            }
                        }
                        else
                        {
                            if (startingZ + z == buildingPorchPosition.Item2)
                            {
                                if (startingX + x == buildingPorchPosition.Item1 ||
                                    startingX + x == buildingPorchPosition.Item1 + 1 ||
                                    startingX + x == buildingPorchPosition.Item1 - 1)
                                {
                                    floorInfo.FloorMap[z * xAxisSize + x] = new FlatMapPoint {IsPorch = true};
                                }
                                else
                                {
                                    floorInfo.FloorMap[z * xAxisSize + x] = new FlatMapPoint {FlatNumber = 1};
                                }
                            }
                            else
                            {
                                floorInfo.FloorMap[z * xAxisSize + x] = new FlatMapPoint {FlatNumber = 1};
                            }
                        }
                    }

                    break;
                }
                case 2:
                {
                    for (var z = 0; z < zAxisSize; z++)
                    for (var x = 0; x < xAxisSize; x++)
                    {
                        if (buildingFaceZAxis)
                        {
                            if (x + startingX < buildingPorchPosition.Item1)
                                floorInfo.FloorMap[z * xAxisSize + x] = new FlatMapPoint {FlatNumber = 1};
                            else if (x + startingX > buildingPorchPosition.Item1)
                            {
                                floorInfo.FloorMap[z * xAxisSize + x] = new FlatMapPoint {FlatNumber = 2};
                            }
                            else
                            {
                                if (buildingPorchPosition.Item2 == startingZ)
                                {
                                    if (z < 2)
                                        floorInfo.FloorMap[z * xAxisSize + x] = new FlatMapPoint {IsPorch = true};
                                    else
                                    {
                                        var middle = zAxisSize / 2 + 2;
                                        floorInfo.FloorMap[z * xAxisSize + x] = new FlatMapPoint
                                            {FlatNumber = z <= middle ? 1 : 2};
                                    }
                                }
                                else
                                {
                                    if (z > zAxisSize - 3)
                                        floorInfo.FloorMap[z * xAxisSize + x] = new FlatMapPoint {IsPorch = true};
                                    else
                                    {
                                        var middle = zAxisSize / 2 - 2;
                                        floorInfo.FloorMap[z * xAxisSize + x] = new FlatMapPoint
                                            {FlatNumber = z <= middle ? 1 : 2};
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (z + startingZ < buildingPorchPosition.Item2)
                                floorInfo.FloorMap[z * xAxisSize + x] = new FlatMapPoint {FlatNumber = 1};
                            else if (z + startingZ > buildingPorchPosition.Item2)
                            {
                                floorInfo.FloorMap[z * xAxisSize + x] = new FlatMapPoint {FlatNumber = 2};
                            }
                            else
                            {
                                if (buildingPorchPosition.Item1 == startingX)
                                {
                                    if (x < 2)
                                        floorInfo.FloorMap[z * xAxisSize + x] = new FlatMapPoint {IsPorch = true};
                                    else
                                    {
                                        var middle = xAxisSize / 2 + 2;
                                        floorInfo.FloorMap[z * xAxisSize + x] = new FlatMapPoint
                                            {FlatNumber = x <= middle ? 1 : 2};
                                    }
                                }
                                else
                                {
                                    if (x > xAxisSize - 3)
                                        floorInfo.FloorMap[z * xAxisSize + x] = new FlatMapPoint {IsPorch = true};
                                    else
                                    {
                                        var middle = xAxisSize / 2 - 2;
                                        floorInfo.FloorMap[z * xAxisSize + x] = new FlatMapPoint
                                            {FlatNumber = x <= middle ? 1 : 2};
                                    }
                                }
                            }
                        }
                    }

                    break;
                }
                case 3:
                {
                    if (buildingFaceZAxis)
                    {
                        var middleZ = zAxisSize / 2;
                        var middleXLeft = (buildingPorchPosition.Item1 - startingX) / 2;
                        var middleXRight = xAxisSize - (startingX + xAxisSize - buildingPorchPosition.Item1) / 2;


                        for (var z = 0; z < zAxisSize; z++)
                        for (var x = 0; x < xAxisSize; x++)
                        {
                            if (buildingPorchPosition.Item2 == startingZ)
                                if (z < middleZ)
                                {
                                    if (startingX + x < buildingPorchPosition.Item1)
                                        floorInfo.FloorMap[z * xAxisSize + x] = new FlatMapPoint {FlatNumber = 1};
                                    else if (startingX + x > buildingPorchPosition.Item1)
                                    {
                                        floorInfo.FloorMap[z * xAxisSize + x] = new FlatMapPoint {FlatNumber = 3};
                                    }
                                    else
                                    {
                                        if (z > 1)
                                            floorInfo.FloorMap[z * xAxisSize + x] =
                                                new FlatMapPoint {FlatNumber = 2};
                                        else
                                        {
                                            floorInfo.FloorMap[z * xAxisSize + x] =
                                                new FlatMapPoint {IsPorch = true};
                                        }
                                    }
                                }
                                else
                                {
                                    if (x < middleXLeft)
                                    {
                                        floorInfo.FloorMap[z * xAxisSize + x] = new FlatMapPoint {FlatNumber = 1};
                                    }
                                    else if (x >= middleXRight)
                                    {
                                        floorInfo.FloorMap[z * xAxisSize + x] = new FlatMapPoint {FlatNumber = 3};
                                    }
                                    else
                                    {
                                        floorInfo.FloorMap[z * xAxisSize + x] = new FlatMapPoint {FlatNumber = 2};
                                    }
                                }
                            else
                            {
                                if (z > middleZ)
                                {
                                    if (startingX + x < buildingPorchPosition.Item1)
                                        floorInfo.FloorMap[z * xAxisSize + x] = new FlatMapPoint {FlatNumber = 3};
                                    else if (startingX + x > buildingPorchPosition.Item1)
                                    {
                                        floorInfo.FloorMap[z * xAxisSize + x] = new FlatMapPoint {FlatNumber = 1};
                                    }
                                    else
                                    {
                                        if (z < zAxisSize - 2)
                                            floorInfo.FloorMap[z * xAxisSize + x] =
                                                new FlatMapPoint {FlatNumber = 2};
                                        else
                                        {
                                            floorInfo.FloorMap[z * xAxisSize + x] =
                                                new FlatMapPoint {IsPorch = true};
                                        }
                                    }
                                }
                                else
                                {
                                    if (x < middleXLeft)
                                    {
                                        floorInfo.FloorMap[z * xAxisSize + x] = new FlatMapPoint {FlatNumber = 3};
                                    }
                                    else if (x >= middleXRight)
                                    {
                                        floorInfo.FloorMap[z * xAxisSize + x] = new FlatMapPoint {FlatNumber = 1};
                                    }
                                    else
                                    {
                                        floorInfo.FloorMap[z * xAxisSize + x] = new FlatMapPoint {FlatNumber = 2};
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        var middleX = xAxisSize / 2;
                        var middleZLeft = (buildingPorchPosition.Item2 - startingZ) / 2 + 1;
                        var middleZRight =
                            zAxisSize - (startingZ + zAxisSize - 1 - buildingPorchPosition.Item2) / 2;
                        for (var z = 0; z < zAxisSize; z++)
                        for (var x = 0; x < xAxisSize; x++)
                        {
                            if (buildingPorchPosition.Item1 == startingX)
                                if (x < middleX)
                                {
                                    if (startingZ + z < buildingPorchPosition.Item2)
                                        floorInfo.FloorMap[z * xAxisSize + x] = new FlatMapPoint {FlatNumber = 3};
                                    else if (startingZ + z > buildingPorchPosition.Item2)
                                    {
                                        floorInfo.FloorMap[z * xAxisSize + x] = new FlatMapPoint {FlatNumber = 1};
                                    }
                                    else
                                    {
                                        if (x > 1)
                                            floorInfo.FloorMap[z * xAxisSize + x] =
                                                new FlatMapPoint {FlatNumber = 2};
                                        else
                                        {
                                            floorInfo.FloorMap[z * xAxisSize + x] =
                                                new FlatMapPoint {IsPorch = true};
                                        }
                                    }
                                }
                                else
                                {
                                    if (z < middleZLeft)
                                    {
                                        floorInfo.FloorMap[z * xAxisSize + x] = new FlatMapPoint {FlatNumber = 3};
                                    }
                                    else if (z >= middleZRight)
                                    {
                                        floorInfo.FloorMap[z * xAxisSize + x] = new FlatMapPoint {FlatNumber = 1};
                                    }
                                    else
                                    {
                                        floorInfo.FloorMap[z * xAxisSize + x] = new FlatMapPoint {FlatNumber = 2};
                                    }
                                }
                            else
                            {
                                if (x > middleX)
                                {
                                    if (startingZ + z < buildingPorchPosition.Item2)
                                        floorInfo.FloorMap[z * xAxisSize + x] = new FlatMapPoint {FlatNumber = 1};
                                    else if (startingZ + z > buildingPorchPosition.Item2)
                                    {
                                        floorInfo.FloorMap[z * xAxisSize + x] = new FlatMapPoint {FlatNumber = 3};
                                    }
                                    else
                                    {
                                        if (x < xAxisSize - 2)
                                            floorInfo.FloorMap[z * xAxisSize + x] =
                                                new FlatMapPoint {FlatNumber = 2};
                                        else
                                        {
                                            floorInfo.FloorMap[z * xAxisSize + x] =
                                                new FlatMapPoint {IsPorch = true};
                                        }
                                    }
                                }
                                else
                                {
                                    if (z < middleZLeft)
                                    {
                                        floorInfo.FloorMap[z * xAxisSize + x] = new FlatMapPoint {FlatNumber = 1};
                                    }
                                    else if (z >= middleZRight)
                                    {
                                        floorInfo.FloorMap[z * xAxisSize + x] = new FlatMapPoint {FlatNumber = 3};
                                    }
                                    else
                                    {
                                        floorInfo.FloorMap[z * xAxisSize + x] = new FlatMapPoint {FlatNumber = 2};
                                    }
                                }
                            }
                        }
                    }
                }

                    break;
            }

            floorInfoList.Add(floorInfo);
        }

        return floorInfoList;
    }

    public static void LayoutRooms(List<BuildingFloorInfo> flatMapList, int startingZ, int startingX,
        int zAxisSize, int xAxisSize,
        bool buildingFaceZAxis, int floorCount,
        Tuple<int, int> buildingPorchPosition, List<Flat>[] flats)
    {
        for (var floorNumber = 0; floorNumber < floorCount; floorNumber++)
        {
            var startingPoints = new List<List<Vector3Int>>();
            switch (flatMapList[floorNumber].FlatCount)
            {
                case 1:
                {
                    startingPoints.Add(new List<Vector3Int>()
                    {
                        new Vector3Int(0, 0, 0),
                        new Vector3Int(xAxisSize - 1, 0, 0),
                        new Vector3Int(0, 0, zAxisSize - 1),
                        new Vector3Int(xAxisSize - 1, 0, zAxisSize - 1)
                    });
                    break;
                }
                case 2:
                {
                    if (buildingFaceZAxis)
                    {
                        startingPoints.Add(new List<Vector3Int>()
                        {
                            new Vector3Int(0, 0, 0),
                            new Vector3Int(0, 0, zAxisSize - 1),
                            FindNextEdgePoint(new Vector3Int(0, 0, 0), false, true, flatMapList[floorNumber].FloorMap,
                                xAxisSize),
                            FindNextEdgePoint(new Vector3Int(0, 0, zAxisSize - 1), false, true,
                                flatMapList[floorNumber].FloorMap, xAxisSize)
                        });
                        startingPoints.Add(new List<Vector3Int>()
                        {
                            new Vector3Int(xAxisSize - 1, 0, 0),
                            new Vector3Int(xAxisSize - 1, 0, zAxisSize - 1),
                            FindNextEdgePoint(new Vector3Int(xAxisSize - 1, 0, 0), false, false,
                                flatMapList[floorNumber].FloorMap, xAxisSize),
                            FindNextEdgePoint(new Vector3Int(xAxisSize - 1, 0, zAxisSize - 1), false, false,
                                flatMapList[floorNumber].FloorMap, xAxisSize)
                        });
                    }
                    else
                    {
                        startingPoints.Add(new List<Vector3Int>()
                        {
                            new Vector3Int(0, 0, 0),
                            new Vector3Int(xAxisSize - 1, 0, 0),
                            FindNextEdgePoint(new Vector3Int(0, 0, 0), true, true,
                                flatMapList[floorNumber].FloorMap, xAxisSize),
                            FindNextEdgePoint(new Vector3Int(xAxisSize - 1, 0, 0), true, true,
                                flatMapList[floorNumber].FloorMap, xAxisSize)
                        });
                        startingPoints.Add(new List<Vector3Int>()
                        {
                            new Vector3Int(0, 0, zAxisSize - 1),
                            new Vector3Int(xAxisSize - 1, 0, zAxisSize - 1),
                            FindNextEdgePoint(new Vector3Int(0, 0, zAxisSize - 1), true, false,
                                flatMapList[floorNumber].FloorMap, xAxisSize),
                            FindNextEdgePoint(new Vector3Int(xAxisSize - 1, 0, zAxisSize - 1), true, false,
                                flatMapList[floorNumber].FloorMap, xAxisSize)
                        });
                    }

                    break;
                }
                case 3:
                {
                    if (buildingFaceZAxis)
                    {
                        if (buildingPorchPosition.Item2 == startingZ)
                        {
                            startingPoints.Add(new List<Vector3Int>()
                            {
                                new Vector3Int(0, 0, 0),
                                new Vector3Int(0, 0, zAxisSize - 1),
                                FindNextEdgePoint(new Vector3Int(0, 0, 0), false, true,
                                    flatMapList[floorNumber].FloorMap, xAxisSize),
                                FindNextEdgePoint(FindNextEdgePoint(new Vector3Int(0, 0, 0), false, true,
                                        flatMapList[floorNumber].FloorMap, xAxisSize), true, true,
                                    flatMapList[floorNumber].FloorMap, xAxisSize)
                            });
                            var centralFlatStartingPoint = FindNextEdgePoint(new Vector3Int(0, 0, zAxisSize - 1), false,
                                true,
                                flatMapList[floorNumber].FloorMap, xAxisSize);
                            centralFlatStartingPoint.x += 1;
                            startingPoints.Add(new List<Vector3Int>()
                            {
                                centralFlatStartingPoint,
                                FindNextEdgePoint(centralFlatStartingPoint, true, false,
                                    flatMapList[floorNumber].FloorMap, xAxisSize),
                                FindNextEdgePoint(centralFlatStartingPoint, false, true,
                                    flatMapList[floorNumber].FloorMap, xAxisSize),
                                FindNextEdgePoint(
                                    FindNextEdgePoint(centralFlatStartingPoint, false, true,
                                        flatMapList[floorNumber].FloorMap, xAxisSize), true, false,
                                    flatMapList[floorNumber].FloorMap, xAxisSize),
                            });
                            startingPoints.Add(new List<Vector3Int>()
                            {
                                new Vector3Int(xAxisSize - 1, 0, 0),
                                new Vector3Int(xAxisSize - 1, 0, zAxisSize - 1),
                                FindNextEdgePoint(new Vector3Int(xAxisSize - 1, 0, 0), false, false,
                                    flatMapList[floorNumber].FloorMap, xAxisSize),
                                FindNextEdgePoint(FindNextEdgePoint(new Vector3Int(xAxisSize - 1, 0, 0), false, false,
                                        flatMapList[floorNumber].FloorMap, xAxisSize), true, true,
                                    flatMapList[floorNumber].FloorMap, xAxisSize)
                            });
                        }
                        else
                        {
                            startingPoints.Add(new List<Vector3Int>()
                            {
                                new Vector3Int(xAxisSize - 1, 0, 0),
                                new Vector3Int(xAxisSize - 1, 0, zAxisSize - 1),
                                FindNextEdgePoint(new Vector3Int(xAxisSize - 1, 0, zAxisSize - 1), false, false,
                                    flatMapList[floorNumber].FloorMap, xAxisSize),
                                FindNextEdgePoint(FindNextEdgePoint(new Vector3Int(xAxisSize - 1, 0, zAxisSize - 1),
                                        false,
                                        false,
                                        flatMapList[floorNumber].FloorMap, xAxisSize), true, false,
                                    flatMapList[floorNumber].FloorMap, xAxisSize)
                            });
                            var centralFlatStartingPoint = FindNextEdgePoint(new Vector3Int(0, 0, 0), false, true,
                                flatMapList[floorNumber].FloorMap, xAxisSize);
                            centralFlatStartingPoint.x += 1;
                            startingPoints.Add(new List<Vector3Int>()
                            {
                                centralFlatStartingPoint,
                                FindNextEdgePoint(centralFlatStartingPoint, true, true,
                                    flatMapList[floorNumber].FloorMap, xAxisSize),
                                FindNextEdgePoint(centralFlatStartingPoint, false, true,
                                    flatMapList[floorNumber].FloorMap, xAxisSize),
                                FindNextEdgePoint(
                                    FindNextEdgePoint(centralFlatStartingPoint, false, true,
                                        flatMapList[floorNumber].FloorMap, xAxisSize), true, true,
                                    flatMapList[floorNumber].FloorMap, xAxisSize),
                            });
                            startingPoints.Add(new List<Vector3Int>()
                            {
                                new Vector3Int(0, 0, 0),
                                new Vector3Int(0, 0, zAxisSize - 1),
                                FindNextEdgePoint(new Vector3Int(0, 0, zAxisSize - 1), false, true,
                                    flatMapList[floorNumber].FloorMap, xAxisSize),
                                FindNextEdgePoint(FindNextEdgePoint(new Vector3Int(0, 0, zAxisSize - 1), false, true,
                                        flatMapList[floorNumber].FloorMap, xAxisSize), true, false,
                                    flatMapList[floorNumber].FloorMap, xAxisSize)
                            });
                        }
                    }
                    else
                    {
                        if (buildingPorchPosition.Item1 == startingX)
                        {
                            startingPoints.Add(new List<Vector3Int>()
                            {
                                new Vector3Int(0, 0, zAxisSize - 1),
                                new Vector3Int(xAxisSize - 1, 0, zAxisSize - 1),
                                FindNextEdgePoint(new Vector3Int(0, 0, zAxisSize - 1), true, false,
                                    flatMapList[floorNumber].FloorMap, xAxisSize),
                                FindNextEdgePoint(FindNextEdgePoint(new Vector3Int(0, 0, zAxisSize - 1),
                                        true,
                                        false,
                                        flatMapList[floorNumber].FloorMap, xAxisSize), false, true,
                                    flatMapList[floorNumber].FloorMap, xAxisSize)
                            });
                            var centralFlatStartingPoint = FindNextEdgePoint(new Vector3Int(xAxisSize - 1, 0, 0), true,
                                true,
                                flatMapList[floorNumber].FloorMap, xAxisSize);
                            centralFlatStartingPoint.z += 1;
                            startingPoints.Add(new List<Vector3Int>()
                            {
                                centralFlatStartingPoint,
                                FindNextEdgePoint(centralFlatStartingPoint, true, true,
                                    flatMapList[floorNumber].FloorMap, xAxisSize),
                                FindNextEdgePoint(centralFlatStartingPoint, false, false,
                                    flatMapList[floorNumber].FloorMap, xAxisSize),
                                FindNextEdgePoint(
                                    FindNextEdgePoint(centralFlatStartingPoint, true, true,
                                        flatMapList[floorNumber].FloorMap, xAxisSize), false, false,
                                    flatMapList[floorNumber].FloorMap, xAxisSize),
                            });
                            startingPoints.Add(new List<Vector3Int>()
                            {
                                new Vector3Int(0, 0, 0),
                                new Vector3Int(xAxisSize - 1, 0, 0),
                                FindNextEdgePoint(new Vector3Int(0, 0, 0), true, true,
                                    flatMapList[floorNumber].FloorMap, xAxisSize),
                                FindNextEdgePoint(FindNextEdgePoint(new Vector3Int(0, 0, 0), true, true,
                                        flatMapList[floorNumber].FloorMap, xAxisSize), false, true,
                                    flatMapList[floorNumber].FloorMap, xAxisSize)
                            });
                        }
                        else
                        {
                            startingPoints.Add(new List<Vector3Int>()
                            {
                                new Vector3Int(0, 0, 0),
                                new Vector3Int(xAxisSize - 1, 0, 0),
                                FindNextEdgePoint(new Vector3Int(xAxisSize - 1, 0, 0), true, true,
                                    flatMapList[floorNumber].FloorMap, xAxisSize),
                                FindNextEdgePoint(FindNextEdgePoint(new Vector3Int(xAxisSize - 1, 0, 0), true, true,
                                        flatMapList[floorNumber].FloorMap, xAxisSize), false, false,
                                    flatMapList[floorNumber].FloorMap, xAxisSize)
                            });
                            var centralFlatStartingPoint = FindNextEdgePoint(new Vector3Int(0, 0, 0), true, true,
                                flatMapList[floorNumber].FloorMap, xAxisSize);
                            centralFlatStartingPoint.z += 1;
                            startingPoints.Add(new List<Vector3Int>()
                            {
                                centralFlatStartingPoint,
                                FindNextEdgePoint(centralFlatStartingPoint, true, true,
                                    flatMapList[floorNumber].FloorMap, xAxisSize),
                                FindNextEdgePoint(centralFlatStartingPoint, false, true,
                                    flatMapList[floorNumber].FloorMap, xAxisSize),
                                FindNextEdgePoint(
                                    FindNextEdgePoint(centralFlatStartingPoint, true, true,
                                        flatMapList[floorNumber].FloorMap, xAxisSize), false, true,
                                    flatMapList[floorNumber].FloorMap, xAxisSize),
                            });
                            startingPoints.Add(new List<Vector3Int>()
                            {
                                new Vector3Int(0, 0, zAxisSize - 1),
                                new Vector3Int(xAxisSize - 1, 0, zAxisSize - 1),
                                FindNextEdgePoint(new Vector3Int(xAxisSize - 1, 0, zAxisSize - 1), true, false,
                                    flatMapList[floorNumber].FloorMap, xAxisSize),
                                FindNextEdgePoint(FindNextEdgePoint(new Vector3Int(xAxisSize - 1, 0, zAxisSize - 1),
                                        true,
                                        false,
                                        flatMapList[floorNumber].FloorMap, xAxisSize), false, false,
                                    flatMapList[floorNumber].FloorMap, xAxisSize)
                            });
                        }
                    }

                    break;
                }
            }

            for (var flatNumber = 0; flatNumber < startingPoints.Count; flatNumber++)
            {
                GrowRoom(flatMapList[floorNumber], flatNumber + 1, startingPoints[flatNumber], zAxisSize, xAxisSize);
            }

            AssignEmptyFlatMapPoints(flatMapList[floorNumber], startingPoints, zAxisSize, xAxisSize);
            AssignRooms(flatMapList[floorNumber], flats[floorNumber], zAxisSize, xAxisSize);
            UpdateFlatMap(flatMapList[floorNumber], zAxisSize, xAxisSize);
            ConnectRooms(flatMapList[floorNumber], startingPoints, xAxisSize);
        }
    }

    private static void GrowRoom(BuildingFloorInfo floorInfo, int flatNumber, List<Vector3Int> startingPoints,
        int zAxisSize, int xAxisSize)
    {
        var growRoomDirectionList = new List<GrowRoomDirection>();
        for (var pointNumber = 0; pointNumber < startingPoints.Count; pointNumber++)
        {
            var point = startingPoints[pointNumber];
            floorInfo.FloorMap[point.z * xAxisSize + point.x].RoomNumber = pointNumber + 1;
            growRoomDirectionList.Add(new GrowRoomDirection
            {
                DirectionXUp = point.x != xAxisSize - 1, DirectionXDown = point.x != 0,
                DirectionZUp = point.z != zAxisSize - 1, DirectionZDown = point.z != 0
            });
        }

        int slideZ = 1, slideX = 0;
        while (true)
        {
            var roomGrew = false;

            for (var pointNumber = 0; pointNumber < startingPoints.Count; pointNumber++)
            {
                if (growRoomDirectionList[pointNumber].CanGrow)
                {
                    var point = startingPoints[pointNumber];
                    roomGrew = true;
                    if (slideZ != 0)
                    {
                        if (growRoomDirectionList[pointNumber].DirectionZUp)
                        {
                            if (point.z + slideZ <= zAxisSize - 1)
                            {
                                if (slideX != 0)
                                {
                                    if (growRoomDirectionList[pointNumber].DirectionXUp)
                                    {
                                        if (point.x + slideX <= xAxisSize - 1)
                                        {
                                            if (floorInfo.FloorMap[(point.z + slideZ) * xAxisSize + point.x + slideX]
                                                    .FlatNumber ==
                                                flatNumber &&
                                                floorInfo.FloorMap[(point.z + slideZ) * xAxisSize + point.x + slideX]
                                                    .RoomNumber ==
                                                0)
                                            {
                                                floorInfo.FloorMap[(point.z + slideZ) * xAxisSize + point.x + slideX]
                                                        .RoomNumber =
                                                    pointNumber + 1;
                                            }
                                            else
                                            {
                                                growRoomDirectionList[pointNumber].DirectionXUp = false;
                                            }
                                        }
                                        else
                                        {
                                            growRoomDirectionList[pointNumber].DirectionXUp = false;
                                        }
                                    }

                                    if (growRoomDirectionList[pointNumber].DirectionXDown)
                                    {
                                        if (point.x - slideX >= 0)
                                        {
                                            if (floorInfo.FloorMap[(point.z + slideZ) * xAxisSize + point.x - slideX]
                                                    .FlatNumber ==
                                                flatNumber &&
                                                floorInfo.FloorMap[(point.z + slideZ) * xAxisSize + point.x - slideX]
                                                    .RoomNumber ==
                                                0)
                                            {
                                                floorInfo.FloorMap[(point.z + slideZ) * xAxisSize + point.x - slideX]
                                                        .RoomNumber =
                                                    pointNumber + 1;
                                            }
                                            else
                                            {
                                                growRoomDirectionList[pointNumber].DirectionXDown = false;
                                            }
                                        }
                                        else
                                        {
                                            growRoomDirectionList[pointNumber].DirectionXDown = false;
                                        }
                                    }
                                }
                                else
                                {
                                    if (floorInfo.FloorMap[(point.z + slideZ) * xAxisSize + point.x].FlatNumber ==
                                        flatNumber &&
                                        floorInfo.FloorMap[(point.z + slideZ) * xAxisSize + point.x].RoomNumber == 0)
                                    {
                                        floorInfo.FloorMap[(point.z + slideZ) * xAxisSize + point.x].RoomNumber =
                                            pointNumber + 1;
                                    }
                                    else
                                    {
                                        growRoomDirectionList[pointNumber].DirectionZUp = false;
                                    }
                                }
                            }
                            else
                            {
                                growRoomDirectionList[pointNumber].DirectionZUp = false;
                            }
                        }


                        if (growRoomDirectionList[pointNumber].DirectionZDown)
                        {
                            if (point.z - slideZ >= 0)
                            {
                                if (slideX != 0)
                                {
                                    if (point.x + slideX <= xAxisSize - 1)
                                    {
                                        if (growRoomDirectionList[pointNumber].DirectionXUp)
                                        {
                                            if (floorInfo.FloorMap[(point.z - slideZ) * xAxisSize + point.x + slideX]
                                                    .FlatNumber ==
                                                flatNumber &&
                                                floorInfo.FloorMap[(point.z - slideZ) * xAxisSize + point.x + slideX]
                                                    .RoomNumber ==
                                                0)
                                            {
                                                floorInfo.FloorMap[(point.z - slideZ) * xAxisSize + point.x + slideX]
                                                        .RoomNumber =
                                                    pointNumber + 1;
                                            }
                                            else
                                            {
                                                growRoomDirectionList[pointNumber].DirectionXUp = false;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        growRoomDirectionList[pointNumber].DirectionXUp = false;
                                    }

                                    if (growRoomDirectionList[pointNumber].DirectionXDown)
                                    {
                                        if (point.x - slideX >= 0)
                                        {
                                            if (floorInfo.FloorMap[(point.z - slideZ) * xAxisSize + point.x - slideX]
                                                    .FlatNumber ==
                                                flatNumber &&
                                                floorInfo.FloorMap[(point.z - slideZ) * xAxisSize + point.x - slideX]
                                                    .RoomNumber ==
                                                0)
                                            {
                                                floorInfo.FloorMap[(point.z - slideZ) * xAxisSize + point.x - slideX]
                                                        .RoomNumber =
                                                    pointNumber + 1;
                                            }
                                            else
                                            {
                                                growRoomDirectionList[pointNumber].DirectionXDown = false;
                                            }
                                        }
                                        else
                                        {
                                            growRoomDirectionList[pointNumber].DirectionXDown = false;
                                        }
                                    }
                                }
                                else
                                {
                                    if (floorInfo.FloorMap[(point.z - slideZ) * xAxisSize + point.x].FlatNumber ==
                                        flatNumber &&
                                        floorInfo.FloorMap[(point.z - slideZ) * xAxisSize + point.x].RoomNumber == 0)
                                    {
                                        floorInfo.FloorMap[(point.z - slideZ) * xAxisSize + point.x].RoomNumber =
                                            pointNumber + 1;
                                    }
                                    else
                                    {
                                        growRoomDirectionList[pointNumber].DirectionZDown = false;
                                    }
                                }
                            }
                            else
                            {
                                growRoomDirectionList[pointNumber].DirectionZDown = false;
                            }
                        }
                    }
                    else
                    {
                        if (growRoomDirectionList[pointNumber].DirectionXUp)
                        {
                            if (point.x + slideX <= xAxisSize - 1)
                            {
                                if (floorInfo.FloorMap[point.z * xAxisSize + point.x + slideX]
                                        .FlatNumber ==
                                    flatNumber &&
                                    floorInfo.FloorMap[point.z * xAxisSize + point.x + slideX]
                                        .RoomNumber ==
                                    0)
                                {
                                    floorInfo.FloorMap[point.z * xAxisSize + point.x + slideX]
                                            .RoomNumber =
                                        pointNumber + 1;
                                }
                                else
                                {
                                    growRoomDirectionList[pointNumber].DirectionXUp = false;
                                }
                            }
                            else
                            {
                                growRoomDirectionList[pointNumber].DirectionXUp = false;
                            }
                        }

                        if (growRoomDirectionList[pointNumber].DirectionXDown)
                        {
                            if (point.x - slideX >= 0)
                            {
                                if (floorInfo.FloorMap[point.z * xAxisSize + point.x - slideX]
                                        .FlatNumber ==
                                    flatNumber &&
                                    floorInfo.FloorMap[point.z * xAxisSize + point.x - slideX]
                                        .RoomNumber ==
                                    0)
                                {
                                    floorInfo.FloorMap[point.z * xAxisSize + point.x - slideX]
                                            .RoomNumber =
                                        pointNumber + 1;
                                }
                                else
                                {
                                    growRoomDirectionList[pointNumber].DirectionXDown = false;
                                }
                            }
                            else
                            {
                                growRoomDirectionList[pointNumber].DirectionXDown = false;
                            }
                        }
                    }
                }
            }

            if (slideX < slideZ)
            {
                var temp = slideZ;
                slideZ = slideX;
                slideX = temp;
            }
            else if (slideX > slideZ)
            {
                var temp = slideZ;
                slideZ = slideX;
                slideX = temp + 1;
            }
            else
            {
                slideX = 0;
                slideZ++;
            }

            if (!roomGrew)
            {
                break;
            }
        }
    }

    private static Vector3Int FindNextEdgePoint(Vector3Int startingPoint, bool zAxisDirection, bool axisUp,
        FlatMapPoint[] flatMap, int xAxisSize)
    {
        var flatNumber = flatMap[startingPoint.z * xAxisSize + startingPoint.x].FlatNumber;
        var index = 0;
        while (true)
        {
            if (zAxisDirection)
            {
                if (axisUp)
                {
                    if (flatMap[(startingPoint.z + index) * xAxisSize + startingPoint.x].FlatNumber != flatNumber)
                    {
                        return new Vector3Int(startingPoint.x, 0, startingPoint.z + index - 1);
                    }
                }
                else
                {
                    if (flatMap[(startingPoint.z - index) * xAxisSize + startingPoint.x].FlatNumber != flatNumber)
                    {
                        return new Vector3Int(startingPoint.x, 0, startingPoint.z - index + 1);
                    }
                }
            }
            else
            {
                if (axisUp)
                {
                    if (flatMap[startingPoint.z * xAxisSize + startingPoint.x + index].FlatNumber != flatNumber)
                    {
                        return new Vector3Int(startingPoint.x + index - 1, 0, startingPoint.z);
                    }
                }
                else
                {
                    if (flatMap[startingPoint.z * xAxisSize + startingPoint.x - index].FlatNumber != flatNumber)
                    {
                        return new Vector3Int(startingPoint.x - index + 1, 0, startingPoint.z);
                    }
                }
            }

            index++;
        }
    }

    private static void AssignEmptyFlatMapPoints(BuildingFloorInfo floorInfo, List<List<Vector3Int>> startingRoomPoints,
        int zAxisSize, int xAxisSize)
    {
        for (var z = 0; z < zAxisSize; z++)
        {
            for (var x = 0; x < xAxisSize; x++)
            {
                var pointInfo = floorInfo.FloorMap[z * xAxisSize + x];
                if (pointInfo.RoomNumber != 0 || pointInfo.IsPorch) continue;
                if (x != xAxisSize - 1 && floorInfo.FloorMap[z * xAxisSize + x + 1].RoomNumber > 4)
                    floorInfo.FloorMap[z * xAxisSize + x].RoomNumber =
                        floorInfo.FloorMap[z * xAxisSize + x + 1].RoomNumber;
                else if (x != 0 && floorInfo.FloorMap[z * xAxisSize + x - 1].RoomNumber > 4)
                    floorInfo.FloorMap[z * xAxisSize + x].RoomNumber =
                        floorInfo.FloorMap[z * xAxisSize + x - 1].RoomNumber;
                else if (z != zAxisSize - 1 && floorInfo.FloorMap[(z + 1) * xAxisSize + x].RoomNumber > 4)
                    floorInfo.FloorMap[z * xAxisSize + x].RoomNumber =
                        floorInfo.FloorMap[(z + 1) * xAxisSize + x].RoomNumber;
                else if (z != 0 && floorInfo.FloorMap[(z - 1) * xAxisSize + x].RoomNumber > 4)
                    floorInfo.FloorMap[z * xAxisSize + x].RoomNumber =
                        floorInfo.FloorMap[(z - 1) * xAxisSize + x].RoomNumber;
                else
                {
                    floorInfo.FloorMap[z * xAxisSize + x].RoomNumber =
                        4 + floorInfo
                            .AdditionalFlatRooms[
                                floorInfo.FloorMap[z * xAxisSize + x].FlatNumber - 1] + 1;
                    floorInfo
                        .AdditionalFlatRooms[
                            floorInfo.FloorMap[z * xAxisSize + x].FlatNumber - 1]++;
                    startingRoomPoints[pointInfo.FlatNumber - 1].Add(new Vector3Int(x, 0, z));
                }
            }
        }
    }

    private static void AssignRooms(BuildingFloorInfo floorInfo, List<Flat> flats, int zAxisSize, int xAxisSize)
    {
        var roomNumberTypes = new Dictionary<int, RoomTypes>[floorInfo.FlatCount];
        for (var i = 0; i < roomNumberTypes.Length; i++)
        {
            var mainRooms = new List<RoomTypes>
                {RoomTypes.Kitchen, RoomTypes.BedRoom, RoomTypes.LivingRoom, RoomTypes.BathRoom};
            var additionalRooms = new List<RoomTypes>
            {
                RoomTypes.Corridor, RoomTypes.BedRoom, RoomTypes.StorageRoom, RoomTypes.LivingRoom, RoomTypes.BathRoom
            };
            roomNumberTypes[i] = new Dictionary<int, RoomTypes>();
            for (int j = 0; j < 4 + floorInfo.AdditionalFlatRooms[i]; j++)
            {
                if (j < 4)
                {
                    var index = Random.Range(0, mainRooms.Count);
                    var room = mainRooms[index];
                    roomNumberTypes[i].Add(j, room);
                    mainRooms.RemoveAt(index);
                }
                else
                {
                    var index = Random.Range(0, additionalRooms.Count);
                    var room = additionalRooms[index];
                    roomNumberTypes[i].Add(j, room);
                    //mainRooms.RemoveAt(index);
                }
            }
        }

        for (var z = 0; z < zAxisSize; z++)
        {
            for (var x = 0; x < xAxisSize; x++)
            {
                var point = floorInfo.FloorMap[z * xAxisSize + x];
                if (point.IsPorch) continue;
                point.RoomType = roomNumberTypes[point.FlatNumber - 1][point.RoomNumber - 1];
            }
        }
    }

    private static void UpdateFlatMap(BuildingFloorInfo floorInfo, int zAxisSize,
        int xAxisSize)
    {
        for (var z = 0;
            z < zAxisSize;
            z++)
        {
            for (var x = 0; x < xAxisSize; x++)
            {
                var pointInfo = floorInfo.FloorMap[z * xAxisSize + x];

                if (z == 0)
                {
                    if (x == 0)
                    {
                        if ((floorInfo.FloorMap[(z + 1) * xAxisSize + x].FlatNumber !=
                             pointInfo.FlatNumber ||
                             floorInfo.FloorMap[(z + 1) * xAxisSize + x].RoomNumber !=
                             pointInfo.RoomNumber))
                        {
                            floorInfo.FloorMap[z * xAxisSize + x].WallZUp = WallTypes.Wall;
                        }

                        if ((floorInfo.FloorMap[z * xAxisSize + x + 1].FlatNumber !=
                             pointInfo.FlatNumber ||
                             floorInfo.FloorMap[z * xAxisSize + x + 1].RoomNumber !=
                             pointInfo.RoomNumber))
                        {
                            floorInfo.FloorMap[z * xAxisSize + x].WallXUp = WallTypes.Wall;
                        }

                        floorInfo.FloorMap[z * xAxisSize + x].WallXDown = WallTypes.Wall;
                        floorInfo.FloorMap[z * xAxisSize + x].WallZDown = WallTypes.Wall;
                    }
                    else if (x == xAxisSize - 1)
                    {
                        if ((floorInfo.FloorMap[(z + 1) * xAxisSize + x].FlatNumber !=
                             pointInfo.FlatNumber ||
                             floorInfo.FloorMap[(z + 1) * xAxisSize + x].RoomNumber !=
                             pointInfo.RoomNumber))
                        {
                            floorInfo.FloorMap[z * xAxisSize + x].WallZUp = WallTypes.Wall;
                        }

                        if ((floorInfo.FloorMap[z * xAxisSize + x - 1].FlatNumber !=
                             pointInfo.FlatNumber ||
                             floorInfo.FloorMap[z * xAxisSize + x - 1].RoomNumber !=
                             pointInfo.RoomNumber))
                        {
                            floorInfo.FloorMap[z * xAxisSize + x].WallXDown = WallTypes.Wall;
                        }

                        floorInfo.FloorMap[z * xAxisSize + x].WallXUp = WallTypes.Wall;
                        floorInfo.FloorMap[z * xAxisSize + x].WallZDown = WallTypes.Wall;
                    }
                    else
                    {
                        if ((floorInfo.FloorMap[(z + 1) * xAxisSize + x].FlatNumber !=
                             pointInfo.FlatNumber ||
                             floorInfo.FloorMap[(z + 1) * xAxisSize + x].RoomNumber !=
                             pointInfo.RoomNumber))
                        {
                            floorInfo.FloorMap[z * xAxisSize + x].WallZUp = WallTypes.Wall;
                        }

                        if (floorInfo.FloorMap[z * xAxisSize + x - 1].FlatNumber !=
                            pointInfo.FlatNumber ||
                            floorInfo.FloorMap[z * xAxisSize + x - 1].RoomNumber !=
                            pointInfo.RoomNumber)
                        {
                            floorInfo.FloorMap[z * xAxisSize + x].WallXDown = WallTypes.Wall;
                        }

                        if (floorInfo.FloorMap[z * xAxisSize + x + 1].FlatNumber !=
                            pointInfo.FlatNumber ||
                            floorInfo.FloorMap[z * xAxisSize + x + 1].RoomNumber !=
                            pointInfo.RoomNumber)
                        {
                            floorInfo.FloorMap[z * xAxisSize + x].WallXUp = WallTypes.Wall;
                        }

                        if (x < xAxisSize / 2)
                        {
                            if (!floorInfo.FloorMap[z * xAxisSize + x + 1].IsPorch)
                            {
                                floorInfo.FloorMap[z * xAxisSize + x].WallZDown =
                                    x % 2 == 1 ? WallTypes.Window : WallTypes.Wall;
                            }
                            else
                            {
                                floorInfo.FloorMap[z * xAxisSize + x].WallZDown = WallTypes.Wall;
                            }
                        }
                        else
                        {
                            if (!floorInfo.FloorMap[z * xAxisSize + x - 1].IsPorch)
                            {
                                floorInfo.FloorMap[z * xAxisSize + x].WallZDown =
                                    (xAxisSize - x + 1) % 2 == 1 ? WallTypes.Window : WallTypes.Wall;
                            }
                            else
                            {
                                floorInfo.FloorMap[z * xAxisSize + x].WallZDown = WallTypes.Wall;
                            }
                        }
                    }
                }
                else if (z == zAxisSize - 1)
                {
                    if (x == 0)
                    {
                        if (floorInfo.FloorMap[(z - 1) * xAxisSize + x].FlatNumber !=
                            pointInfo.FlatNumber ||
                            floorInfo.FloorMap[(z - 1) * xAxisSize + x].RoomNumber !=
                            pointInfo.RoomNumber)
                        {
                            floorInfo.FloorMap[z * xAxisSize + x].WallZDown = WallTypes.Wall;
                        }

                        if (floorInfo.FloorMap[z * xAxisSize + x + 1].FlatNumber !=
                            pointInfo.FlatNumber ||
                            floorInfo.FloorMap[z * xAxisSize + x + 1].RoomNumber !=
                            pointInfo.RoomNumber)
                        {
                            floorInfo.FloorMap[z * xAxisSize + x].WallXUp = WallTypes.Wall;
                        }

                        floorInfo.FloorMap[z * xAxisSize + x].WallXDown = WallTypes.Wall;
                        floorInfo.FloorMap[z * xAxisSize + x].WallZUp = WallTypes.Wall;
                    }
                    else if (x == xAxisSize - 1)
                    {
                        if (floorInfo.FloorMap[(z - 1) * xAxisSize + x].FlatNumber !=
                            pointInfo.FlatNumber ||
                            floorInfo.FloorMap[(z - 1) * xAxisSize + x].RoomNumber !=
                            pointInfo.RoomNumber)
                        {
                            floorInfo.FloorMap[z * xAxisSize + x].WallZDown = WallTypes.Wall;
                        }

                        if (floorInfo.FloorMap[z * xAxisSize + x - 1].FlatNumber !=
                            pointInfo.FlatNumber ||
                            floorInfo.FloorMap[z * xAxisSize + x - 1].RoomNumber !=
                            pointInfo.RoomNumber)
                        {
                            floorInfo.FloorMap[z * xAxisSize + x].WallXDown = WallTypes.Wall;
                        }

                        floorInfo.FloorMap[z * xAxisSize + x].WallXUp = WallTypes.Wall;
                        floorInfo.FloorMap[z * xAxisSize + x].WallZUp = WallTypes.Wall;
                    }
                    else
                    {
                        if ((floorInfo.FloorMap[(z - 1) * xAxisSize + x].FlatNumber !=
                             pointInfo.FlatNumber ||
                             floorInfo.FloorMap[(z - 1) * xAxisSize + x].RoomNumber !=
                             pointInfo.RoomNumber))
                        {
                            floorInfo.FloorMap[z * xAxisSize + x].WallZDown = WallTypes.Wall;
                        }

                        if (floorInfo.FloorMap[z * xAxisSize + x - 1].FlatNumber !=
                            pointInfo.FlatNumber ||
                            floorInfo.FloorMap[z * xAxisSize + x - 1].RoomNumber !=
                            pointInfo.RoomNumber)
                        {
                            floorInfo.FloorMap[z * xAxisSize + x].WallXDown = WallTypes.Wall;
                        }

                        if (floorInfo.FloorMap[z * xAxisSize + x + 1].FlatNumber !=
                            pointInfo.FlatNumber ||
                            floorInfo.FloorMap[z * xAxisSize + x + 1].RoomNumber !=
                            pointInfo.RoomNumber)
                        {
                            floorInfo.FloorMap[z * xAxisSize + x].WallXUp = WallTypes.Wall;
                        }

                        if (x < xAxisSize / 2)
                        {
                            if (!floorInfo.FloorMap[z * xAxisSize + x + 1].IsPorch)
                            {
                                floorInfo.FloorMap[z * xAxisSize + x].WallZUp =
                                    x % 2 == 1 ? WallTypes.Window : WallTypes.Wall;
                            }
                            else
                            {
                                floorInfo.FloorMap[z * xAxisSize + x].WallZUp = WallTypes.Wall;
                            }
                        }
                        else
                        {
                            if (!floorInfo.FloorMap[z * xAxisSize + x - 1].IsPorch)
                            {
                                floorInfo.FloorMap[z * xAxisSize + x].WallZUp =
                                    (xAxisSize - x + 1) % 2 == 1 ? WallTypes.Window : WallTypes.Wall;
                            }
                            else
                            {
                                floorInfo.FloorMap[z * xAxisSize + x].WallZUp = WallTypes.Wall;
                            }
                        }
                    }
                }
                else
                {
                    if (x == 0)
                    {
                        if (floorInfo.FloorMap[(z - 1) * xAxisSize + x].FlatNumber !=
                            pointInfo.FlatNumber ||
                            floorInfo.FloorMap[(z - 1) * xAxisSize + x].RoomNumber !=
                            pointInfo.RoomNumber)
                        {
                            floorInfo.FloorMap[z * xAxisSize + x].WallZDown = WallTypes.Wall;
                        }

                        if (floorInfo.FloorMap[(z + 1) * xAxisSize + x].FlatNumber !=
                            pointInfo.FlatNumber ||
                            floorInfo.FloorMap[(z + 1) * xAxisSize + x].RoomNumber !=
                            pointInfo.RoomNumber)
                        {
                            floorInfo.FloorMap[z * xAxisSize + x].WallZUp = WallTypes.Wall;
                        }

                        if ((floorInfo.FloorMap[z * xAxisSize + x + 1].FlatNumber !=
                             pointInfo.FlatNumber ||
                             floorInfo.FloorMap[z * xAxisSize + x + 1].RoomNumber !=
                             pointInfo.RoomNumber) &&
                            !floorInfo.FloorMap[z * xAxisSize + x + 1].IsPorch)
                        {
                            floorInfo.FloorMap[z * xAxisSize + x].WallXUp = WallTypes.Wall;
                        }

                        if (z < zAxisSize / 2)
                        {
                            if (!floorInfo.FloorMap[(z - 1) * xAxisSize + x].IsPorch)
                                floorInfo.FloorMap[z * xAxisSize + x].WallXDown =
                                    z % 2 == 1 ? WallTypes.Window : WallTypes.Wall;
                            else
                                floorInfo.FloorMap[z * xAxisSize + x].WallXDown = WallTypes.Wall;
                        }
                        else
                        {
                            if (!floorInfo.FloorMap[(z - 1) * xAxisSize + x].IsPorch)
                                floorInfo.FloorMap[z * xAxisSize + x].WallXDown =
                                    (zAxisSize - z + 1) % 2 == 1 ? WallTypes.Window : WallTypes.Wall;
                            else
                                floorInfo.FloorMap[z * xAxisSize + x].WallXDown = WallTypes.Wall;
                        }
                    }
                    else if (x == xAxisSize - 1)
                    {
                        if (floorInfo.FloorMap[(z - 1) * xAxisSize + x].FlatNumber !=
                            pointInfo.FlatNumber ||
                            floorInfo.FloorMap[(z - 1) * xAxisSize + x].RoomNumber !=
                            pointInfo.RoomNumber)
                        {
                            floorInfo.FloorMap[z * xAxisSize + x].WallZDown = WallTypes.Wall;
                        }

                        if (floorInfo.FloorMap[(z + 1) * xAxisSize + x].FlatNumber !=
                            pointInfo.FlatNumber ||
                            floorInfo.FloorMap[(z + 1) * xAxisSize + x].RoomNumber !=
                            pointInfo.RoomNumber)
                        {
                            floorInfo.FloorMap[z * xAxisSize + x].WallZUp = WallTypes.Wall;
                        }

                        if ((floorInfo.FloorMap[z * xAxisSize + x - 1].FlatNumber !=
                             pointInfo.FlatNumber ||
                             floorInfo.FloorMap[z * xAxisSize + x - 1].RoomNumber !=
                             pointInfo.RoomNumber) &&
                            !floorInfo.FloorMap[z * xAxisSize + x - 1].IsPorch)
                        {
                            floorInfo.FloorMap[z * xAxisSize + x].WallXDown = WallTypes.Wall;
                        }

                        if (z < zAxisSize / 2)
                        {
                            if (!floorInfo.FloorMap[(z - 1) * xAxisSize + x].IsPorch)
                                floorInfo.FloorMap[z * xAxisSize + x].WallXUp =
                                    z % 2 == 1 ? WallTypes.Window : WallTypes.Wall;
                            else
                                floorInfo.FloorMap[z * xAxisSize + x].WallXUp = WallTypes.Wall;
                        }
                        else
                        {
                            if (!floorInfo.FloorMap[(z - 1) * xAxisSize + x].IsPorch)
                                floorInfo.FloorMap[z * xAxisSize + x].WallXUp =
                                    (zAxisSize - z + 1) % 2 == 1 ? WallTypes.Window : WallTypes.Wall;
                            else
                                floorInfo.FloorMap[z * xAxisSize + x].WallXUp = WallTypes.Wall;
                        }
                    }
                    else
                    {
                        if (floorInfo.FloorMap[(z - 1) * xAxisSize + x].FlatNumber !=
                            pointInfo.FlatNumber ||
                            floorInfo.FloorMap[(z - 1) * xAxisSize + x].RoomNumber !=
                            pointInfo.RoomNumber)
                        {
                            floorInfo.FloorMap[z * xAxisSize + x].WallZDown =
                                floorInfo.FloorMap[(z - 1) * xAxisSize + x].IsPorch
                                    ? WallTypes.DoorFrame
                                    : WallTypes.Wall;
                        }

                        if (floorInfo.FloorMap[(z + 1) * xAxisSize + x].FlatNumber !=
                            pointInfo.FlatNumber ||
                            floorInfo.FloorMap[(z + 1) * xAxisSize + x].RoomNumber !=
                            pointInfo.RoomNumber)
                        {
                            floorInfo.FloorMap[z * xAxisSize + x].WallZUp =
                                floorInfo.FloorMap[(z + 1) * xAxisSize + x].IsPorch
                                    ? WallTypes.DoorFrame
                                    : WallTypes.Wall;
                        }

                        if (floorInfo.FloorMap[z * xAxisSize + x - 1].FlatNumber !=
                            pointInfo.FlatNumber ||
                            floorInfo.FloorMap[z * xAxisSize + x - 1].RoomNumber !=
                            pointInfo.RoomNumber)
                        {
                            floorInfo.FloorMap[z * xAxisSize + x].WallXDown =
                                floorInfo.FloorMap[z * xAxisSize + x - 1].IsPorch
                                    ? WallTypes.DoorFrame
                                    : WallTypes.Wall;
                        }

                        if (floorInfo.FloorMap[z * xAxisSize + x + 1].FlatNumber !=
                            pointInfo.FlatNumber ||
                            floorInfo.FloorMap[z * xAxisSize + x + 1].RoomNumber !=
                            pointInfo.RoomNumber)
                        {
                            floorInfo.FloorMap[z * xAxisSize + x].WallXUp =
                                floorInfo.FloorMap[z * xAxisSize + x + 1].IsPorch
                                    ? WallTypes.DoorFrame
                                    : WallTypes.Wall;
                        }
                    }
                }
            }
        }
    }

    [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
    [SuppressMessage("ReSharper", "RedundantBoolCompare")]
    private static void ConnectRooms(BuildingFloorInfo floorInfo, List<List<Vector3Int>> startingRoomPoints,
        int xAxisSize)
    {
        for (var flatNumber = 0;
            flatNumber < startingRoomPoints.Count;
            flatNumber++)
        {
            var foundPoints = new bool[startingRoomPoints[flatNumber].Count];

            var startingRoomPointIndex = Random.Range(0, startingRoomPoints[flatNumber].Count);
            foundPoints[startingRoomPointIndex] = true;
            var startingRoomPointCoordinates =
                startingRoomPoints[flatNumber][startingRoomPointIndex];
            var startingRoomPoint =
                floorInfo.FloorMap[startingRoomPointCoordinates.z * xAxisSize + startingRoomPointCoordinates.x];


            var nextNearestRoomPoint = Vector3Int.zero;
            var distanceToNearestPoint = 0f;

            while (foundPoints.Any(r => r == false))
            {
                for (var point = 0; point < startingRoomPoints[flatNumber].Count; point++)
                {
                    if (!foundPoints[point])
                    {
                        var pointToCheck = startingRoomPoints[flatNumber][point];
                        var distanceToPoint = math.sqrt(
                            math.pow(startingRoomPointCoordinates.x - pointToCheck.x, 2) +
                            math.pow(startingRoomPointCoordinates.z - pointToCheck.z, 2));
                        if (distanceToNearestPoint == 0f)
                        {
                            distanceToNearestPoint = distanceToPoint;
                            nextNearestRoomPoint = pointToCheck;
                        }
                        else
                        {
                            if (distanceToPoint < distanceToNearestPoint)
                            {
                                distanceToNearestPoint = distanceToPoint;
                                nextNearestRoomPoint = pointToCheck;
                            }
                        }
                    }
                }

                var mainPathBlocked = false;
                var roomWasFound = false;
                while (!roomWasFound)
                {
                    var directionX = Random.Range(0, 2) == 0;

                    if (mainPathBlocked)
                    {
                        var foundPointsCount = foundPoints.Count(r => r == true);
                        if (foundPointsCount <= 2)
                        {
                            var notFoundIndices = new List<int>();
                            for (var i = 0; i < foundPoints.Length; i++)
                            {
                                if (!foundPoints[i])
                                {
                                    notFoundIndices.Add(i);
                                }
                            }

                            var newPoint =
                                startingRoomPoints[flatNumber][
                                    notFoundIndices[Random.Range(0, notFoundIndices.Count)]];
                            nextNearestRoomPoint = newPoint;
                        }
                        else
                        {
                            var foundIndices = new List<int>();
                            for (var i = 0; i < foundPoints.Length; i++)
                            {
                                if (foundPoints[i])
                                {
                                    foundIndices.Add(i);
                                }
                            }

                            var newPoint =
                                startingRoomPoints[flatNumber][
                                    foundIndices[Random.Range(0, foundIndices.Count)]];
                            startingRoomPointCoordinates = newPoint;
                            startingRoomPoint = floorInfo.FloorMap[
                                startingRoomPointCoordinates.z * xAxisSize + startingRoomPointCoordinates.x];
                        }

                        mainPathBlocked = false;
                    }

                    if (directionX)
                    {
                        if (startingRoomPointCoordinates.x < nextNearestRoomPoint.x)
                        {
                            if (floorInfo.FloorMap[
                                    startingRoomPointCoordinates.z * xAxisSize +
                                    startingRoomPointCoordinates.x + 1]
                                .FlatNumber == flatNumber + 1)
                            {
                                if (floorInfo.FloorMap[
                                        startingRoomPointCoordinates.z * xAxisSize +
                                        startingRoomPointCoordinates.x + 1]
                                    .RoomNumber != startingRoomPoint.RoomNumber)
                                {
                                    roomWasFound = true;
                                    foundPoints[floorInfo.FloorMap[
                                            startingRoomPointCoordinates.z * xAxisSize +
                                            startingRoomPointCoordinates.x + 1]
                                        .RoomNumber - 1] = true;
                                    floorInfo.FloorMap[
                                            startingRoomPointCoordinates.z * xAxisSize +
                                            startingRoomPointCoordinates.x]
                                        .WallXUp = WallTypes.DoorFrame;
                                    floorInfo.FloorMap[
                                            startingRoomPointCoordinates.z * xAxisSize +
                                            startingRoomPointCoordinates.x + 1]
                                        .WallXDown = WallTypes.DoorFrame;
                                }

                                startingRoomPointCoordinates.x++;
                                startingRoomPoint =
                                    floorInfo.FloorMap[
                                        startingRoomPointCoordinates.z * xAxisSize +
                                        startingRoomPointCoordinates.x];
                            }
                            else
                            {
                                mainPathBlocked = true;
                            }
                        }
                        else if (startingRoomPointCoordinates.x > nextNearestRoomPoint.x)
                        {
                            if (floorInfo.FloorMap[
                                    startingRoomPointCoordinates.z * xAxisSize +
                                    startingRoomPointCoordinates.x - 1]
                                .FlatNumber == flatNumber + 1)
                            {
                                if (floorInfo.FloorMap[
                                        startingRoomPointCoordinates.z * xAxisSize +
                                        startingRoomPointCoordinates.x - 1]
                                    .RoomNumber != startingRoomPoint.RoomNumber)
                                {
                                    roomWasFound = true;
                                    foundPoints[floorInfo.FloorMap[
                                            startingRoomPointCoordinates.z * xAxisSize +
                                            startingRoomPointCoordinates.x - 1]
                                        .RoomNumber - 1] = true;
                                    floorInfo.FloorMap[
                                            startingRoomPointCoordinates.z * xAxisSize +
                                            startingRoomPointCoordinates.x]
                                        .WallXDown = WallTypes.DoorFrame;
                                    floorInfo.FloorMap[
                                            startingRoomPointCoordinates.z * xAxisSize +
                                            startingRoomPointCoordinates.x - 1]
                                        .WallXUp = WallTypes.DoorFrame;
                                }

                                startingRoomPointCoordinates.x--;
                                startingRoomPoint =
                                    floorInfo.FloorMap[
                                        startingRoomPointCoordinates.z * xAxisSize +
                                        startingRoomPointCoordinates.x];
                            }
                            else
                            {
                                mainPathBlocked = true;
                            }
                        }
                    }
                    else
                    {
                        if (startingRoomPointCoordinates.z < nextNearestRoomPoint.z)
                        {
                            if (floorInfo.FloorMap[
                                    (startingRoomPointCoordinates.z + 1) * xAxisSize +
                                    startingRoomPointCoordinates.x]
                                .FlatNumber == flatNumber + 1)
                            {
                                if (floorInfo.FloorMap[
                                        (startingRoomPointCoordinates.z + 1) * xAxisSize +
                                        startingRoomPointCoordinates.x]
                                    .RoomNumber != startingRoomPoint.RoomNumber)
                                {
                                    roomWasFound = true;
                                    foundPoints[floorInfo.FloorMap[
                                            (startingRoomPointCoordinates.z + 1) * xAxisSize +
                                            startingRoomPointCoordinates.x]
                                        .RoomNumber - 1] = true;
                                    floorInfo.FloorMap[
                                            startingRoomPointCoordinates.z * xAxisSize +
                                            startingRoomPointCoordinates.x]
                                        .WallZUp = WallTypes.DoorFrame;
                                    floorInfo.FloorMap[
                                            (startingRoomPointCoordinates.z + 1) * xAxisSize +
                                            startingRoomPointCoordinates.x]
                                        .WallZDown = WallTypes.DoorFrame;
                                }

                                startingRoomPointCoordinates.z++;
                                startingRoomPoint =
                                    floorInfo.FloorMap[
                                        startingRoomPointCoordinates.z * xAxisSize +
                                        startingRoomPointCoordinates.x];
                            }
                            else
                            {
                                mainPathBlocked = true;
                            }
                        }
                        else if (startingRoomPointCoordinates.z > nextNearestRoomPoint.z)
                        {
                            if (floorInfo.FloorMap[
                                    (startingRoomPointCoordinates.z - 1) * xAxisSize +
                                    startingRoomPointCoordinates.x]
                                .FlatNumber == flatNumber + 1)
                            {
                                if (floorInfo.FloorMap[
                                        (startingRoomPointCoordinates.z - 1) * xAxisSize +
                                        startingRoomPointCoordinates.x]
                                    .RoomNumber != startingRoomPoint.RoomNumber)
                                {
                                    roomWasFound = true;
                                    foundPoints[floorInfo.FloorMap[
                                            (startingRoomPointCoordinates.z - 1) * xAxisSize +
                                            startingRoomPointCoordinates.x]
                                        .RoomNumber - 1] = true;
                                    floorInfo.FloorMap[
                                            startingRoomPointCoordinates.z * xAxisSize +
                                            startingRoomPointCoordinates.x]
                                        .WallZDown = WallTypes.DoorFrame;
                                    floorInfo.FloorMap[
                                            (startingRoomPointCoordinates.z - 1) * xAxisSize +
                                            startingRoomPointCoordinates.x]
                                        .WallZUp = WallTypes.DoorFrame;
                                }

                                startingRoomPointCoordinates.z--;
                                startingRoomPoint =
                                    floorInfo.FloorMap[
                                        startingRoomPointCoordinates.z * xAxisSize +
                                        startingRoomPointCoordinates.x];
                            }
                            else
                            {
                                mainPathBlocked = true;
                            }
                        }
                    }

                    if (startingRoomPoint.IsPorch)
                    {
                        startingRoomPointCoordinates = startingRoomPoints[flatNumber][startingRoomPointIndex];
                        startingRoomPoint = floorInfo.FloorMap[
                            startingRoomPointCoordinates.z * xAxisSize + startingRoomPointCoordinates.x];
                    }

                    if (roomWasFound)
                        distanceToNearestPoint = 0f;
                }
            }
        }
    }
}