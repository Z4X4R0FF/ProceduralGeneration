using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[RequireComponent(typeof(MeshFilter))]

// public class LevelGenerator : MonoBehaviour
// {
//     [Header("Level Settings")]
//     [Tooltip("Position of top left corner of the mesh map on scene")]
//     public Vector3 StartingPoint;
//     [Tooltip("Number of squares along X axis. ONLY ODD VALUES ARE POSSIBLE")]
//     public int LevelXLength;
//     [Tooltip("Number of squares along Z axis. ONLY ODD VALUES ARE POSSIBLE.")]
//     public int LevelZLength;
//     public GameObject LevelWall;
//     [Header("Terrain mesh settings")]
//     [Tooltip("Minimal size of one terrain mesh part")]
//     [Range(2, 255)]
//     public int TerrainPartSizeMin;
//     [Tooltip("Maximum size of one terrain mesh part")]
//     [Range(2, 255)]
//     public int TerrainPartSizeMax;
//     [Tooltip("Set to 'True' to make the algorithm choose the smallest available part size. Only works with small 'LevelLength' values")]
//     public bool MinSize = false;
//     public float PerlinNoiseScale;
//     public float HeightMultiplier;
//     [Header("Street Settings")]
//     public List<StreetsToGenerate> StreetsAlongX = new List<StreetsToGenerate>();
//     public List<StreetsToGenerate> StreetsAlongZ = new List<StreetsToGenerate>();
//     [Header("Material settings")]
//     public Material Soil;
//     public Material Road;
//     public Material Pavement;
//     public Material Grass;
//     public Material[] Materials;
//     Mesh mesh;
//     Vector3[] Vertices;
//     List<List<int>>[] SubMeshTris = new List<List<int>>[4]
//     {
//         new List<List<int>>(),
//         new List<List<int>>(),
//         new List<List<int>>(),
//         new List<List<int>>()
//     };
//     List<GameObject> terrainParts = new List<GameObject>();
//     NativeArray<LevelMapPoint> LevelMap;
//     int TerrainPartSizeX;
//     int TerrainPartSizeZ;
//     int NumberTerrainMeshPartsX;
//     int NumberTerrainMeshPartsZ;
//     public bool IsDone = false;

//     void Start()
//     {
//         LevelXLength++;
//         LevelZLength++;
//         Materials = new Material[4] { Soil, Road, Pavement, Grass };

//         Vector2Int randomPerlinNoiseStartPoint = new Vector2Int(
//             UnityEngine.Random.Range(-100000, 100000),
//             UnityEngine.Random.Range(-100000, 100000));

//         var vertices = new NativeArray<Vector3>(LevelXLength * LevelZLength, Allocator.TempJob);//[LevelXLength * LevelZLength];
//         LevelMap = new NativeArray<LevelMapPoint>(LevelXLength * LevelZLength, Allocator.TempJob);
//         var MeshPartsCount = new NativeArray<int>(2, Allocator.TempJob);
//         var TerrainPartSizes = new NativeArray<int>(2, Allocator.TempJob);
//         var calculateTerrainPartSizeJob = new CalculateTerrainPartSizeJob
//         {
//             LevelXLength = LevelXLength,
//             LevelZLength = LevelZLength,
//             MinSize = MinSize,
//             MeshPartsCount = MeshPartsCount,
//             TerrainPartSizes = TerrainPartSizes,
//             TerrainPartSizeMax = TerrainPartSizeMax,
//             TerrainPartSizeMin = TerrainPartSizeMin
//         };
//         calculateTerrainPartSizeJob.Schedule().Complete();
//         NumberTerrainMeshPartsX = MeshPartsCount[0];
//         NumberTerrainMeshPartsZ = MeshPartsCount[1];
//         TerrainPartSizeX = TerrainPartSizes[0];
//         TerrainPartSizeZ = TerrainPartSizes[1];
//         MeshPartsCount.Dispose();
//         TerrainPartSizes.Dispose();
//         //LevelMap = StreetGenerator.GenerateStreetMap(LevelZLength, LevelXLength, StreetsAlongX, StreetsAlongZ);
//         var generateMeshJob = new GenerateMeshJob
//         {
//             LevelXLength = LevelXLength,
//             LevelZLength = LevelZLength,
//             LevelMap = LevelMap,
//             vertices = vertices,
//             PerlinNoiseScale = PerlinNoiseScale,
//             HeightMultiplier = HeightMultiplier,
//             randomPerlinNoiseStartPoint = randomPerlinNoiseStartPoint
//         };
//         generateMeshJob.Schedule().Complete();
//         CreateTerrainParts(NumberTerrainMeshPartsX * NumberTerrainMeshPartsZ);
//         //GetComponent<BuildingGenerator>().GenerateBuildingBasement(LevelMap);

//         Vertices = vertices.ToArray();
//         vertices.Dispose();

//         AssembleTerrainParts();
//         //SpawnLevelBorders();
//         //UnityEditor.StaticOcclusionCulling.GenerateInBackground();
//         IsDone = true;
//         Debug.Log(IsDone);
//         LevelMap.Dispose();

//     }
//     private void OnDestroy()
//     {
//         UnityEditor.StaticOcclusionCulling.RemoveCacheFolder();
//     }

//     void CreateTerrainParts(int count)
//     {
//         for (int i = 0; i < count; i++)
//         {
//             var terrainPart = new GameObject($"TerrainPart{i}");
//             terrainPart.AddComponent<MeshRenderer>();
//             terrainPart.AddComponent<MeshCollider>();
//             terrainPart.AddComponent<MeshFilter>();
//             terrainPart.transform.parent = transform;
//             terrainParts.Add(terrainPart);
//         }
//     }

//     private void AssembleTerrainParts()
//     {
//         for (int i = 0, z = 0; z < NumberTerrainMeshPartsZ; z++)
//         {
//             for (int x = 0; x < NumberTerrainMeshPartsX; x++)
//             {
//                 terrainParts[i + x].transform.position = StartingPoint;
//                 mesh = new Mesh();
//                 terrainParts[i + x].GetComponent<MeshFilter>().mesh = mesh;
//                 CreateMeshTris(x, z);
//                 UpdateMesh(terrainParts[i + x], x, z);
//             }
//             i += NumberTerrainMeshPartsX;
//         }
//     }

//     void CreateMeshTris(int partNumberX, int partNumberZ)
//     {
//         var subMeshTris = new List<int>[4] { new List<int>(), new List<int>(), new List<int>(), new List<int>() };
//         var slideX = (TerrainPartSizeX - 1) * partNumberX;
//         var slideZ = (TerrainPartSizeZ - 1) * partNumberZ;
//         int vert = 0;
//         for (int z = 0; z < TerrainPartSizeZ - 1; z++)
//         {
//             for (int x = 0; x < TerrainPartSizeX - 1; x++)
//             {
//                 var materialNumber = LevelMap[(z + slideZ) * LevelZLength + (x + slideX)].MaterialNumber;
//                 subMeshTris[materialNumber].Add(vert);
//                 subMeshTris[materialNumber].Add(vert + TerrainPartSizeX);
//                 subMeshTris[materialNumber].Add(vert + TerrainPartSizeX + 1);
//                 subMeshTris[materialNumber].Add(vert + TerrainPartSizeX + 1);
//                 subMeshTris[materialNumber].Add(vert + 1);
//                 subMeshTris[materialNumber].Add(vert);

//                 vert++;
//             }
//             vert++;
//         }
//         for (int i = 0; i < subMeshTris.Length; i++)
//             SubMeshTris[i].Add(subMeshTris[i]);
//     }
//     void UpdateMesh(GameObject terrainPart, int partNumberX, int partNumberZ)
//     {
//         mesh.Clear();
//         var verticesPart = new Vector3[TerrainPartSizeX * TerrainPartSizeZ];
//         var uvsPart = new Vector2[TerrainPartSizeX * TerrainPartSizeZ];
//         for (int i = 0; i < TerrainPartSizeZ; i++)
//         {
//             Array.Copy(Vertices,
//                 (i * LevelXLength) + ((TerrainPartSizeX - 1) * partNumberX) + partNumberZ * LevelXLength * (TerrainPartSizeX - 1),
//                 verticesPart, i * TerrainPartSizeX,
//                 TerrainPartSizeX);
//         }
//         mesh.vertices = verticesPart;
//         mesh.subMeshCount = 4;
//         mesh.SetTriangles(SubMeshTris[0].Last(), 0);
//         mesh.SetTriangles(SubMeshTris[1].Last(), 1);
//         mesh.SetTriangles(SubMeshTris[2].Last(), 2);
//         mesh.SetTriangles(SubMeshTris[3].Last(), 3);
//         terrainPart.GetComponent<MeshRenderer>().materials = Materials;
//         for (int i = 0; i < uvsPart.Length; i++)
//         {
//             uvsPart[i] = new Vector2(verticesPart[i].x, verticesPart[i].z);
//         }
//         mesh.uv = uvsPart;
//         mesh.RecalculateNormals();
//         terrainPart.GetComponent<MeshCollider>().sharedMesh = mesh;
//     }

// }
// public struct CalculateTerrainPartSizeJob : IJob
// {
//     public bool MinSize;
//     public int TerrainPartSizeMin, TerrainPartSizeMax;
//     public NativeArray<int> TerrainPartSizes;
//     public int LevelXLength, LevelZLength;
//     public NativeArray<int> MeshPartsCount;
//     public void Execute()
//     {
//         int terrainPartSizeX = MinSize ? TerrainPartSizeMin : TerrainPartSizeMax;
//         int terrainPartSizeZ = MinSize ? TerrainPartSizeMin : TerrainPartSizeMax;
//         while (TerrainPartSizes[0] == 0)
//             if ((LevelXLength - terrainPartSizeX) % (terrainPartSizeX - 1) == 0)
//             {
//                 TerrainPartSizes[0] = terrainPartSizeX;
//             }
//             else
//                 terrainPartSizeX = MinSize ? terrainPartSizeX + 1 : terrainPartSizeX - 1;
//         while (TerrainPartSizes[1] == 0)
//             if ((LevelZLength - terrainPartSizeZ) % (terrainPartSizeZ - 1) == 0)
//             {
//                 TerrainPartSizes[1] = terrainPartSizeZ;
//             }
//             else
//                 terrainPartSizeZ = MinSize ? terrainPartSizeZ + 1 : terrainPartSizeZ - 1;
//         MeshPartsCount[0] = (LevelXLength - terrainPartSizeX) / (TerrainPartSizes[0] - 1) + 1;
//         MeshPartsCount[1] = (LevelZLength - terrainPartSizeZ) / (TerrainPartSizes[1] - 1) + 1;
//     }
// }

// public struct GenerateMeshJob : IJob
// {
//     public NativeArray<Vector3> vertices;
//     public int LevelXLength, LevelZLength;
//     public NativeArray<LevelMapPoint> LevelMap;
//     public float PerlinNoiseScale, HeightMultiplier;
//     public Vector2Int randomPerlinNoiseStartPoint;
//     public void Execute()
//     {
//         var slideAlongZ = 0;
//         var slideAlongZOnNextRow = 0;
//         for (int i = 0, z = 0; z < LevelZLength; z++)
//         {
//             int slideAlongX = 0;
//             for (int x = 0; x < LevelXLength; x++, i++)
//             {
//                 var y = GeneratePerlinNoise(randomPerlinNoiseStartPoint.x + x - slideAlongX,
//                     randomPerlinNoiseStartPoint.y + z - slideAlongZ, PerlinNoiseScale) * HeightMultiplier;
//                 if (LevelMap[z * LevelZLength + x].IsStreetAlongZ && !LevelMap[z * LevelZLength + x].IsStreetAlongX)
//                 {
//                     if (!LevelMap[z * LevelZLength + x].WasHandledByTerrainGenerator)
//                     {
//                         int streetWidth = FindStreetEnd(false, z, x, false);
//                         for (int k = 0; k < streetWidth; k++)
//                         {
//                             vertices[i + k] = new Vector3(x + k, y, z);
//                             var levelMapPoint = LevelMap[z * LevelZLength + x + k];
//                             levelMapPoint.WasHandledByTerrainGenerator = true;
//                             levelMapPoint.y = y;
//                             LevelMap[z * LevelZLength + x + k] = levelMapPoint;
//                         }
//                         slideAlongX += streetWidth;
//                     }
//                 }
//                 else if (!LevelMap[z * LevelZLength + x].IsStreetAlongZ && LevelMap[z * LevelZLength + x].IsStreetAlongX)
//                 {
//                     if (!LevelMap[z * LevelZLength + x].WasHandledByTerrainGenerator)
//                     {
//                         int streetWidth = FindStreetEnd(true, z, x, false);
//                         for (int k = 0; k < streetWidth; k++)
//                         {
//                             vertices[i + (LevelXLength * k)] = new Vector3(x, y, z + k);
//                             var levelMapPoint = LevelMap[(z + k) * LevelZLength + x];
//                             levelMapPoint.WasHandledByTerrainGenerator = true;
//                             levelMapPoint.y = y;
//                             LevelMap[(z + k) * LevelZLength + x] = levelMapPoint;
//                         }
//                         slideAlongZOnNextRow = streetWidth;
//                     }
//                 }
//                 else if (LevelMap[z * LevelZLength + x].IsStreetAlongX && LevelMap[z * LevelZLength + x].IsStreetAlongZ)
//                 {
//                     if (!LevelMap[z * LevelZLength + x].WasHandledByTerrainGenerator)
//                     {
//                         int streetWidthX = FindStreetEnd(false, z, x, true);
//                         int streetWidthZ = FindStreetEnd(true, z, x, true);
//                         for (int k = 0; k < streetWidthX; k++)
//                         {
//                             for (int l = 0; l < streetWidthZ; l++)
//                             {
//                                 vertices[i + (LevelXLength * l) + k] = new Vector3(x + k, y, z + l);
//                                 var levelMapPoint = LevelMap[(z + l) * LevelZLength + (x + k)];
//                                 levelMapPoint.WasHandledByTerrainGenerator = true;
//                                 levelMapPoint.y = y;
//                                 LevelMap[(z + l) * LevelZLength + (x + k)] = levelMapPoint;
//                             }
//                         }
//                         slideAlongZOnNextRow = streetWidthZ;
//                         slideAlongX += streetWidthX;
//                     }
//                 }
//                 else
//                 {
//                     vertices[i] = new Vector3(x, y, z);
//                     var levelMapPoint = LevelMap[z * LevelZLength + x];
//                     levelMapPoint.y = y;
//                     LevelMap[z * LevelZLength + x] = levelMapPoint;
//                 }
//             }
//             slideAlongZ += slideAlongZOnNextRow;
//             slideAlongZOnNextRow = 0;
//         }
//     }

//     private float GeneratePerlinNoise(float x, float z, float scale)
//     {
//         return Mathf.PerlinNoise(x / scale, z / scale);
//     }

//     private int FindStreetEnd(bool isStreetAlongX, int z, int x, bool isCross)
//     {
//         int answer = 0;
//         if (isStreetAlongX)
//         {
//             while (LevelMap[(z + answer) * LevelZLength + x].IsStreetAlongX && (!isCross || LevelMap[(z + answer) * LevelZLength + x].IsStreetAlongZ))
//             {
//                 answer++;
//             }
//         }
//         else
//         {
//             while (LevelMap[z * LevelZLength + (x + answer)].IsStreetAlongZ && (!isCross || LevelMap[z * LevelZLength + (x + answer)].IsStreetAlongX))
//             {
//                 answer++;
//             }
//         }
//         return answer;
//     }
// }
public class LevelGenerator : MonoBehaviour
{
    private JobHandle _job;
    [FormerlySerializedAs("Grass")] public Material grass;

    [FormerlySerializedAs("HeightMultiplier")]
    public float heightMultiplier;

    [FormerlySerializedAs("IsDone")] public bool isDone;
    [FormerlySerializedAs("LevelWall")] public GameObject levelWall;

    [FormerlySerializedAs("LevelXLength")] [Tooltip("Number of squares along X axis. ONLY ODD VALUES ARE POSSIBLE")]
    public int levelXLength;

    [FormerlySerializedAs("LevelZLength")] [Tooltip("Number of squares along Z axis. ONLY ODD VALUES ARE POSSIBLE.")]
    public int levelZLength;

    [FormerlySerializedAs("Materials")] public Material[] materials;

    [FormerlySerializedAs("MinSize")]
    [Tooltip(
        "Set to 'True' to make the algorithm choose the smallest available part size. Only works with small 'LevelLength' values")]
    public bool minSize;

    [FormerlySerializedAs("Pavement")] public Material pavement;

    [FormerlySerializedAs("PerlinNoiseScale")]
    public float perlinNoiseScale;

    [FormerlySerializedAs("Road")] public Material road;

    [FormerlySerializedAs("Soil")] [Header("Material settings")]
    public Material soil;

    [FormerlySerializedAs("StartingPoint")]
    [Header("Level Settings")]
    [Tooltip("Position of top left corner of the mesh map on scene")]
    public Vector3 startingPoint;

    [FormerlySerializedAs("StreetsAlongX")] [Header("Street Settings")]
    public List<StreetsToGenerate> streetsAlongX = new List<StreetsToGenerate>();

    [FormerlySerializedAs("StreetsAlongZ")]
    public List<StreetsToGenerate> streetsAlongZ = new List<StreetsToGenerate>();

    [FormerlySerializedAs("TerrainPartSizeMax")] [Tooltip("Maximum size of one terrain mesh part")] [Range(2, 255)]
    public int terrainPartSizeMax;

    [FormerlySerializedAs("TerrainPartSizeMin")]
    [Header("Terrain mesh settings")]
    [Tooltip("Minimal size of one terrain mesh part")]
    [Range(2, 255)]
    public int terrainPartSizeMin;

    private void Start()
    {
        levelXLength++;
        levelZLength++;
        materials = new[] {soil, road, pavement, grass};

        var nativeStreetsAlongX = new NativeArray<StreetsToGenerate>(streetsAlongX.Count, Allocator.TempJob);
        var nativeStreetsAlongZ = new NativeArray<StreetsToGenerate>(streetsAlongZ.Count, Allocator.TempJob);
        for (var i = 0; i < nativeStreetsAlongX.Length; i++) nativeStreetsAlongX[i] = nativeStreetsAlongX[i];
        for (var i = 0; i < nativeStreetsAlongX.Length; i++) nativeStreetsAlongZ[i] = nativeStreetsAlongZ[i];

        _job = new GenerateLevelJob
        {
            LevelXLength = levelXLength,
            LevelZLength = levelZLength,
            StreetsAlongX = nativeStreetsAlongX,
            StreetsAlongZ = nativeStreetsAlongZ,
            PerlinNoiseScale = perlinNoiseScale,
            HeightMultiplier = heightMultiplier,
            MinSize = minSize,
            TerrainPartSizeMin = terrainPartSizeMin,
            TerrainPartSizeMax = terrainPartSizeMax,
            StartingPoint = startingPoint,
            //Executor = this.gameObject,
            //LevelWall = LevelWall,
            Materials = materials
        }.Schedule();
        //job.Complete();
    }

    private void Update()
    {
        if (!isDone)
            if (_job.IsCompleted)
            {
                _job.Complete();
                isDone = true;
            }
    }
}

public struct GenerateLevelJob : IJob
{
    #region natives

    #endregion

    #region outside

    public int LevelXLength, LevelZLength;
    public NativeArray<StreetsToGenerate> StreetsAlongX;
    public NativeArray<StreetsToGenerate> StreetsAlongZ;
    public float PerlinNoiseScale, HeightMultiplier;
    public bool MinSize;
    public int TerrainPartSizeMin, TerrainPartSizeMax;
    public Vector3 StartingPoint;
    public Material[] Materials;

    #endregion

    #region internal

    private int _terrainPartSizeX, _terrainPartSizeZ;
    private int _numberTerrainMeshPartsX, _numberTerrainMeshPartsZ;
    private Mesh _mesh;
    private Vector3[] _vertices;
    private List<GameObject> _terrainParts;
    private List<List<int>>[] _subMeshTris;
    private LevelMapPoint[,] _levelMap;

    #endregion

    public void Execute()
    {
        _subMeshTris = new[]
        {
            new List<List<int>>(),
            new List<List<int>>(),
            new List<List<int>>(),
            new List<List<int>>()
        };

        CalculateTerrainPartSize();
        _levelMap = StreetGenerator.GenerateStreetMap(LevelZLength, LevelXLength, StreetsAlongX.ToList(),
            StreetsAlongZ.ToList());
        GenerateMesh();
        CreateTerrainParts(_numberTerrainMeshPartsX * _numberTerrainMeshPartsZ);
        //GetComponent<BuildingGenerator>().GenerateBuildingBasement(LevelMap);
        AssembleTerrainParts();
        //SpawnLevelBorders();
        //UnityEditor.StaticOcclusionCulling.GenerateInBackground();
    }

    public float GeneratePerlinNoise(float x, float z, float scale)
    {
        return Mathf.PerlinNoise(x / scale, z / scale);
    }

    private void UpdateMesh(GameObject terrainPart, int partNumberX, int partNumberZ)
    {
        _mesh.Clear();
        var verticesPart = new Vector3[_terrainPartSizeX * _terrainPartSizeZ];
        var uvsPart = new Vector2[_terrainPartSizeX * _terrainPartSizeZ];
        for (var i = 0; i < _terrainPartSizeZ; i++)
            Array.Copy(_vertices,
                i * LevelXLength + (_terrainPartSizeX - 1) * partNumberX +
                partNumberZ * LevelXLength * (_terrainPartSizeX - 1),
                verticesPart, i * _terrainPartSizeX,
                _terrainPartSizeX);
        _mesh.vertices = verticesPart;
        _mesh.subMeshCount = 4;
        _mesh.SetTriangles(_subMeshTris[0].Last(), 0);
        _mesh.SetTriangles(_subMeshTris[1].Last(), 1);
        _mesh.SetTriangles(_subMeshTris[2].Last(), 2);
        _mesh.SetTriangles(_subMeshTris[3].Last(), 3);
        terrainPart.GetComponent<MeshRenderer>().materials = Materials;
        for (var i = 0; i < uvsPart.Length; i++) uvsPart[i] = new Vector2(verticesPart[i].x, verticesPart[i].z);
        _mesh.uv = uvsPart;
        _mesh.RecalculateNormals();
        terrainPart.GetComponent<MeshCollider>().sharedMesh = _mesh;
    }

    private IEnumerator GenerateMesh()
    {
        _vertices = new Vector3[LevelXLength * LevelZLength];
        var randomPerlinNoiseStartPoint = new Vector2Int(
            Random.Range(-100000, 100000),
            Random.Range(-100000, 100000));
        var slideAlongZ = 0;
        var slideAlongZOnNextRow = 0;
        for (int i = 0, z = 0; z < LevelZLength; z++)
        {
            var slideAlongX = 0;
            for (var x = 0; x < LevelXLength; x++, i++)
            {
                var y = GeneratePerlinNoise(randomPerlinNoiseStartPoint.x + x - slideAlongX,
                    randomPerlinNoiseStartPoint.y + z - slideAlongZ, PerlinNoiseScale) * HeightMultiplier;

                if (_levelMap[z, x].IsStreetAlongZ && !_levelMap[z, x].IsStreetAlongX)
                {
                    if (!_levelMap[z, x].WasHandledByTerrainGenerator)
                    {
                        var streetWidth = FindStreetEnd(false, z, x, false);
                        for (var k = 0; k < streetWidth; k++)
                        {
                            _vertices[i + k] = new Vector3(x + k, y, z);
                            _levelMap[z, x + k].WasHandledByTerrainGenerator = true;
                            _levelMap[z, x + k].Y = y;
                        }

                        slideAlongX += streetWidth;
                    }
                }
                else if (!_levelMap[z, x].IsStreetAlongZ && _levelMap[z, x].IsStreetAlongX)
                {
                    if (!_levelMap[z, x].WasHandledByTerrainGenerator)
                    {
                        var streetWidth = FindStreetEnd(true, z, x, false);
                        for (var k = 0; k < streetWidth; k++)
                        {
                            _vertices[i + LevelXLength * k] = new Vector3(x, y, z + k);
                            _levelMap[z + k, x].WasHandledByTerrainGenerator = true;
                            _levelMap[z + k, x].Y = y;
                        }

                        slideAlongZOnNextRow = streetWidth;
                    }
                }
                else if (_levelMap[z, x].IsStreetAlongX && _levelMap[z, x].IsStreetAlongZ)
                {
                    if (!_levelMap[z, x].WasHandledByTerrainGenerator)
                    {
                        var streetWidthX = FindStreetEnd(false, z, x, true);
                        var streetWidthZ = FindStreetEnd(true, z, x, true);
                        for (var k = 0; k < streetWidthX; k++)
                        for (var l = 0; l < streetWidthZ; l++)
                        {
                            _vertices[i + LevelXLength * l + k] = new Vector3(x + k, y, z + l);
                            _levelMap[z + l, x + k].WasHandledByTerrainGenerator = true;
                            _levelMap[z + l, x + k].Y = y;
                        }

                        slideAlongZOnNextRow = streetWidthZ;
                        slideAlongX += streetWidthX;
                    }
                }
                else
                {
                    _vertices[i] = new Vector3(x, y, z);
                    _levelMap[z, x].Y = y;
                }
            }

            slideAlongZ += slideAlongZOnNextRow;
            slideAlongZOnNextRow = 0;
            yield return null;
        }
    }

    private void CreateMeshTris(int partNumberX, int partNumberZ)
    {
        var subMeshTris = new[] {new List<int>(), new List<int>(), new List<int>(), new List<int>()};
        var slideX = (_terrainPartSizeX - 1) * partNumberX;
        var slideZ = (_terrainPartSizeZ - 1) * partNumberZ;
        var vert = 0;
        for (var z = 0; z < _terrainPartSizeZ - 1; z++)
        {
            for (var x = 0; x < _terrainPartSizeX - 1; x++)
            {
                var materialNumber = _levelMap[z + slideZ, x + slideX].MaterialNumber;
                subMeshTris[materialNumber].Add(vert);
                subMeshTris[materialNumber].Add(vert + _terrainPartSizeX);
                subMeshTris[materialNumber].Add(vert + _terrainPartSizeX + 1);
                subMeshTris[materialNumber].Add(vert + _terrainPartSizeX + 1);
                subMeshTris[materialNumber].Add(vert + 1);
                subMeshTris[materialNumber].Add(vert);

                vert++;
            }

            vert++;
        }

        for (var i = 0; i < subMeshTris.Length; i++)
            _subMeshTris[i].Add(subMeshTris[i]);
    }

    private int FindStreetEnd(bool isStreetAlongX, int z, int x, bool isCross)
    {
        var answer = 0;
        if (isStreetAlongX)
            while (_levelMap[z + answer, x].IsStreetAlongX && (!isCross || _levelMap[z + answer, x].IsStreetAlongZ))
                answer++;
        else
            while (_levelMap[z, x + answer].IsStreetAlongZ && (!isCross || _levelMap[z, x + answer].IsStreetAlongX))
                answer++;
        return answer;
    }

    private IEnumerator CalculateTerrainPartSize()
    {
        var terrainPartSizeX = MinSize ? TerrainPartSizeMin : TerrainPartSizeMax;
        var terrainPartSizeZ = MinSize ? TerrainPartSizeMin : TerrainPartSizeMax;
        while (_terrainPartSizeX == 0)
            if ((LevelXLength - terrainPartSizeX) % (terrainPartSizeX - 1) == 0)
                _terrainPartSizeX = terrainPartSizeX;
            else
                terrainPartSizeX = MinSize ? terrainPartSizeX + 1 : terrainPartSizeX - 1;
        while (_terrainPartSizeZ == 0)
            if ((LevelZLength - terrainPartSizeZ) % (terrainPartSizeZ - 1) == 0)
                _terrainPartSizeZ = terrainPartSizeZ;
            else
                terrainPartSizeZ = MinSize ? terrainPartSizeZ + 1 : terrainPartSizeZ - 1;
        _numberTerrainMeshPartsX = (LevelXLength - terrainPartSizeX) / (_terrainPartSizeX - 1) + 1;
        _numberTerrainMeshPartsZ = (LevelZLength - terrainPartSizeZ) / (_terrainPartSizeZ - 1) + 1;
        yield return null;
    }

    private void CreateTerrainParts(int count)
    {
        for (var i = 0; i < count; i++)
        {
            var terrainPart = new GameObject($"TerrainPart{i}");
            terrainPart.AddComponent<MeshRenderer>();
            terrainPart.AddComponent<MeshCollider>();
            terrainPart.AddComponent<MeshFilter>();
            //terrainPart.transform.parent = Executor.transform;
            _terrainParts.Add(terrainPart);
        }
    }

    private void AssembleTerrainParts()
    {
        for (int i = 0, z = 0; z < _numberTerrainMeshPartsZ; z++)
        {
            for (var x = 0; x < _numberTerrainMeshPartsX; x++)
            {
                _terrainParts[i + x].transform.position = StartingPoint;
                _mesh = new Mesh();
                _terrainParts[i + x].GetComponent<MeshFilter>().mesh = _mesh;
                CreateMeshTris(x, z);
                UpdateMesh(_terrainParts[i + x], x, z);
            }

            i += _numberTerrainMeshPartsX;
        }
    }

    // private void SpawnLevelBorders()
    // {
    //     for (int z = 1; z < LevelZLength - 2; z++)
    //     {
    //         Instantiate(LevelWall, new Vector3(
    //                 0 + 0.05f,
    //                 (LevelMap[z, 0].y + LevelMap[z, 0 + 1].y) / 2f + 0.5f,
    //                 z + 0.5f),
    //             Quaternion.Euler(0f, 180f, 0f));
    //         Instantiate(LevelWall, new Vector3(
    //                 LevelXLength - 1 - 0.05f,
    //                 (LevelMap[z, LevelXLength - 2].y + LevelMap[z, LevelXLength - 1].y) / 2f + 0.5f,
    //                 z + 0.5f),
    //             Quaternion.Euler(0f, 0f, 0f));
    //     }
    //     for (int x = 1; x < LevelXLength - 2; x++)
    //     {
    //         Instantiate(LevelWall, new Vector3(
    //                 x + 0.5f,
    //                 (LevelMap[0, x].y + LevelMap[0 + 1, x].y) / 2f + 0.5f,
    //                 0 + 0.05f),
    //             Quaternion.Euler(0f, 90f, 0f));
    //         Instantiate(LevelWall, new Vector3(
    //                 x + 0.5f,
    //                 (LevelMap[LevelZLength - 2, x].y + LevelMap[LevelZLength - 1, x].y) / 2f + 0.5f,
    //                 LevelZLength - 1 - 0.05f),
    //             Quaternion.Euler(0f, -90f, 0f));
    //     }
    // }
}