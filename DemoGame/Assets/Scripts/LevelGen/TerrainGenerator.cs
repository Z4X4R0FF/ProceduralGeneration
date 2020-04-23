using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[RequireComponent(typeof(MeshFilter))]
public class TerrainGenerator : MonoBehaviour
{
    private LevelMapPoint[,] _levelMap;
    private Mesh _mesh;
    private int _numberTerrainMeshPartsX;
    private int _numberTerrainMeshPartsZ;

    private readonly List<List<int>>[] _subMeshTris =
    {
        new List<List<int>>(),
        new List<List<int>>(),
        new List<List<int>>(),
        new List<List<int>>()
    };

    private readonly List<GameObject> _terrainParts = new List<GameObject>();
    private int _terrainPartSizeX;
    private int _terrainPartSizeZ;
    private Vector3[] _vertices;
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

        StartCoroutine(DoBigProcess());
    }

    private void OnDestroy()
    {
        StaticOcclusionCulling.RemoveCacheFolder();
    }

    private IEnumerator DoBigProcess()
    {
        yield return StartCoroutine(CalculateTerrainPartSize());
        _levelMap = StreetGenerator.GenerateStreetMap(levelZLength, levelXLength, streetsAlongX, streetsAlongZ);
        yield return null;
        yield return StartCoroutine(GenerateMesh());
        CreateTerrainParts(_numberTerrainMeshPartsX * _numberTerrainMeshPartsZ);
        yield return null;
        yield return StartCoroutine(GetComponent<BuildingGenerator>().GenerateBuildingBasement(_levelMap));
        yield return null;
        yield return StartCoroutine(AssembleTerrainParts());
        SpawnLevelBorders();
        StaticOcclusionCulling.GenerateInBackground();
        while (StaticOcclusionCulling.isRunning) yield return null;
        yield return null;
        isDone = true;
        yield return null;
    }

    private static float GeneratePerlinNoise(float x, float z, float scale)
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
                i * levelXLength + (_terrainPartSizeX - 1) * partNumberX +
                partNumberZ * levelXLength * (_terrainPartSizeX - 1),
                verticesPart, i * _terrainPartSizeX,
                _terrainPartSizeX);
        _mesh.vertices = verticesPart;
        _mesh.subMeshCount = 4;
        _mesh.SetTriangles(_subMeshTris[0].Last(), 0);
        _mesh.SetTriangles(_subMeshTris[1].Last(), 1);
        _mesh.SetTriangles(_subMeshTris[2].Last(), 2);
        _mesh.SetTriangles(_subMeshTris[3].Last(), 3);
        terrainPart.GetComponent<MeshRenderer>().materials = materials;
        for (var i = 0; i < uvsPart.Length; i++) uvsPart[i] = new Vector2(verticesPart[i].x, verticesPart[i].z);
        _mesh.uv = uvsPart;
        _mesh.RecalculateNormals();
        terrainPart.GetComponent<MeshCollider>().sharedMesh = _mesh;
    }

    private IEnumerator GenerateMesh()
    {
        _vertices = new Vector3[levelXLength * levelZLength];
        var randomPerlinNoiseStartPoint = new Vector2Int(
            Random.Range(-100000, 100000),
            Random.Range(-100000, 100000));
        var slideAlongZ = 0;
        var slideAlongZOnNextRow = 0;
        for (int i = 0, z = 0; z < levelZLength; z++)
        {
            var slideAlongX = 0;
            for (var x = 0; x < levelXLength; x++, i++)
            {
                var y = GeneratePerlinNoise(randomPerlinNoiseStartPoint.x + x - slideAlongX,
                    randomPerlinNoiseStartPoint.y + z - slideAlongZ, perlinNoiseScale) * heightMultiplier;

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
                            _vertices[i + levelXLength * k] = new Vector3(x, y, z + k);
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
                            _vertices[i + levelXLength * l + k] = new Vector3(x + k, y, z + l);
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
        var vertex = 0;
        for (var z = 0; z < _terrainPartSizeZ - 1; z++)
        {
            for (var x = 0; x < _terrainPartSizeX - 1; x++)
            {
                var materialNumber = _levelMap[z + slideZ, x + slideX].MaterialNumber;
                subMeshTris[materialNumber].Add(vertex);
                subMeshTris[materialNumber].Add(vertex + _terrainPartSizeX);
                subMeshTris[materialNumber].Add(vertex + _terrainPartSizeX + 1);
                subMeshTris[materialNumber].Add(vertex + _terrainPartSizeX + 1);
                subMeshTris[materialNumber].Add(vertex + 1);
                subMeshTris[materialNumber].Add(vertex);

                vertex++;
            }

            vertex++;
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
        var terrainPartSizeX = minSize ? terrainPartSizeMin : terrainPartSizeMax;
        var terrainPartSizeZ = minSize ? terrainPartSizeMin : terrainPartSizeMax;
        while (_terrainPartSizeX == 0)
            if ((levelXLength - terrainPartSizeX) % (terrainPartSizeX - 1) == 0)
                _terrainPartSizeX = terrainPartSizeX;
            else
                terrainPartSizeX = minSize ? terrainPartSizeX + 1 : terrainPartSizeX - 1;
        while (_terrainPartSizeZ == 0)
            if ((levelZLength - terrainPartSizeZ) % (terrainPartSizeZ - 1) == 0)
                _terrainPartSizeZ = terrainPartSizeZ;
            else
                terrainPartSizeZ = minSize ? terrainPartSizeZ + 1 : terrainPartSizeZ - 1;
        _numberTerrainMeshPartsX = (levelXLength - terrainPartSizeX) / (_terrainPartSizeX - 1) + 1;
        _numberTerrainMeshPartsZ = (levelZLength - terrainPartSizeZ) / (_terrainPartSizeZ - 1) + 1;
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
            terrainPart.transform.parent = transform;
            _terrainParts.Add(terrainPart);
        }
    }

    private IEnumerator AssembleTerrainParts()
    {
        for (int i = 0, z = 0; z < _numberTerrainMeshPartsZ; z++)
        {
            for (var x = 0; x < _numberTerrainMeshPartsX; x++)
            {
                _terrainParts[i + x].transform.position = startingPoint;
                _mesh = new Mesh();
                _terrainParts[i + x].GetComponent<MeshFilter>().mesh = _mesh;
                CreateMeshTris(x, z);
                UpdateMesh(_terrainParts[i + x], x, z);
                yield return null;
            }

            i += _numberTerrainMeshPartsX;
        }
    }

    private void SpawnLevelBorders()
    {
        for (var z = 1; z < levelZLength - 2; z++)
        {
            Instantiate(levelWall, new Vector3(
                    0 + 0.05f,
                    (_levelMap[z, 0].Y + _levelMap[z, 0 + 1].Y) / 2f + 0.5f,
                    z + 0.5f),
                Quaternion.Euler(0f, 180f, 0f));
            Instantiate(levelWall, new Vector3(
                    levelXLength - 1 - 0.05f,
                    (_levelMap[z, levelXLength - 2].Y + _levelMap[z, levelXLength - 1].Y) / 2f + 0.5f,
                    z + 0.5f),
                Quaternion.Euler(0f, 0f, 0f));
        }

        for (var x = 1; x < levelXLength - 2; x++)
        {
            Instantiate(levelWall, new Vector3(
                    x + 0.5f,
                    (_levelMap[0, x].Y + _levelMap[0 + 1, x].Y) / 2f + 0.5f,
                    0 + 0.05f),
                Quaternion.Euler(0f, 90f, 0f));
            Instantiate(levelWall, new Vector3(
                    x + 0.5f,
                    (_levelMap[levelZLength - 2, x].Y + _levelMap[levelZLength - 1, x].Y) / 2f + 0.5f,
                    levelZLength - 1 - 0.05f),
                Quaternion.Euler(0f, -90f, 0f));
        }
    }
}