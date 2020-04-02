using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
[RequireComponent(typeof(MeshFilter))]
public class TerrainGenerator : MonoBehaviour
{
    [Header("Level Settings")]
    [Tooltip("Position of top left corner of the mesh map on scene")]
    public Vector3 StartingPoint;
    [Tooltip("Number of squares along X axis. ONLY ODD VALUES ARE POSSIBLE")]
    public int LevelXLength;
    [Tooltip("Number of squares along Z axis. ONLY ODD VALUES ARE POSSIBLE.")]
    public int LevelZLength;
    public GameObject LevelWall;
    [Header("Terrain mesh settings")]
    [Tooltip("Minimal size of one terrain mesh part")]
    [Range(2, 255)]
    public int TerrainPartSizeMin;
    [Tooltip("Maximum size of one terrain mesh part")]
    [Range(2, 255)]
    public int TerrainPartSizeMax;
    [Tooltip("Set to 'True' to make the algorithm choose the smallest available part size. Only works with small 'LevelLength' values")]
    public bool MinSize = false;
    public float PerlinNoiseScale;
    public float HeightMultiplier;
    [Header("Street Settings")]
    public List<StreetsToGenerate> StreetsAlongX = new List<StreetsToGenerate>();
    public List<StreetsToGenerate> StreetsAlongZ = new List<StreetsToGenerate>();
    [Header("Material settings")]
    public Material Soil;
    public Material Road;
    public Material Pavement;
    public Material Grass;
    public Material[] Materials;
    Mesh mesh;
    Vector3[] vertices;
    List<List<int>>[] SubMeshTris = new List<List<int>>[4]
    {
        new List<List<int>>(),
        new List<List<int>>(),
        new List<List<int>>(),
        new List<List<int>>()
    };
    List<GameObject> terrainParts = new List<GameObject>();
    LevelMapPoint[,] LevelMap;
    int TerrainPartSizeX;
    int TerrainPartSizeZ;
    int NumberTerrainMeshPartsX;
    int NumberTerrainMeshPartsZ;
    public bool IsDone = false;

    void Awake()
    {

        LevelXLength++;
        LevelZLength++;
        Materials = new Material[4] { Soil, Road, Pavement, Grass };

        StartCoroutine("DoBigProcess");

    }
    private void OnDestroy()
    {
        UnityEditor.StaticOcclusionCulling.RemoveCacheFolder();
    }
    public IEnumerator DoBigProcess()
    {
        yield return StartCoroutine("CalculateTerrainPartSize");
        LevelMap = StreetGenerator.GenerateStreetMap(LevelZLength, LevelXLength, StreetsAlongX, StreetsAlongZ);
        yield return null;
        yield return StartCoroutine("GenerateMesh");
        CreateTerrainParts(NumberTerrainMeshPartsX * NumberTerrainMeshPartsZ);
        yield return null;
        yield return StartCoroutine(GetComponent<BuildingGenerator>().GenerateBuildingBasement(LevelMap));
        yield return null;
        yield return StartCoroutine("AssembleTerrainParts");
        SpawnLevelBorders();
        UnityEditor.StaticOcclusionCulling.GenerateInBackground();
        while (UnityEditor.StaticOcclusionCulling.isRunning)
        {
            yield return null;
        }
        yield return null;
        IsDone = true;
        yield return null;
    }
    public float GeneratePerlinNoise(float x, float z, float scale)
    {
        return Mathf.PerlinNoise(x / scale, z / scale);
    }
    void UpdateMesh(GameObject terrainPart, int partNumberX, int partNumberZ)
    {
        mesh.Clear();
        var verticesPart = new Vector3[TerrainPartSizeX * TerrainPartSizeZ];
        var uvsPart = new Vector2[TerrainPartSizeX * TerrainPartSizeZ];
        for (int i = 0; i < TerrainPartSizeZ; i++)
        {
            Array.Copy(vertices,
                (i * LevelXLength) + ((TerrainPartSizeX - 1) * partNumberX) + partNumberZ * LevelXLength * (TerrainPartSizeX - 1),
                verticesPart, i * TerrainPartSizeX,
                TerrainPartSizeX);
        }
        mesh.vertices = verticesPart;
        mesh.subMeshCount = 4;
        mesh.SetTriangles(SubMeshTris[0].Last(), 0);
        mesh.SetTriangles(SubMeshTris[1].Last(), 1);
        mesh.SetTriangles(SubMeshTris[2].Last(), 2);
        mesh.SetTriangles(SubMeshTris[3].Last(), 3);
        terrainPart.GetComponent<MeshRenderer>().materials = Materials;
        for (int i = 0; i < uvsPart.Length; i++)
        {
            uvsPart[i] = new Vector2(verticesPart[i].x, verticesPart[i].z);
        }
        mesh.uv = uvsPart;
        mesh.RecalculateNormals();
        terrainPart.GetComponent<MeshCollider>().sharedMesh = mesh;
    }
    private IEnumerator GenerateMesh()
    {
        vertices = new Vector3[LevelXLength * LevelZLength];
        Vector2Int randomPerlinNoiseStartPoint = new Vector2Int(
            UnityEngine.Random.Range(-100000, 100000),
            UnityEngine.Random.Range(-100000, 100000));
        var slideAlongZ = 0;
        var slideAlongZOnNextRow = 0;
        for (int i = 0, z = 0; z < LevelZLength; z++)
        {
            int slideAlongX = 0;
            for (int x = 0; x < LevelXLength; x++, i++)
            {
                var y = GeneratePerlinNoise(randomPerlinNoiseStartPoint.x + x - slideAlongX,
                    randomPerlinNoiseStartPoint.y + z - slideAlongZ, PerlinNoiseScale) * HeightMultiplier;

                if (LevelMap[z, x].IsStreetAlongZ && !LevelMap[z, x].IsStreetAlongX)
                {
                    if (!LevelMap[z, x].WasHandledByTerrainGenerator)
                    {
                        int streetWidth = FindStreetEnd(false, z, x, false);
                        for (int k = 0; k < streetWidth; k++)
                        {
                            vertices[i + k] = new Vector3(x + k, y, z);
                            LevelMap[z, x + k].WasHandledByTerrainGenerator = true;
                            LevelMap[z, x + k].y = y;
                        }
                        slideAlongX += streetWidth;
                    }
                }
                else if (!LevelMap[z, x].IsStreetAlongZ && LevelMap[z, x].IsStreetAlongX)
                {
                    if (!LevelMap[z, x].WasHandledByTerrainGenerator)
                    {
                        int streetWidth = FindStreetEnd(true, z, x, false);
                        for (int k = 0; k < streetWidth; k++)
                        {
                            vertices[i + (LevelXLength * k)] = new Vector3(x, y, z + k);
                            LevelMap[z + k, x].WasHandledByTerrainGenerator = true;
                            LevelMap[z + k, x].y = y;
                        }
                        slideAlongZOnNextRow = streetWidth;
                    }
                }
                else if (LevelMap[z, x].IsStreetAlongX && LevelMap[z, x].IsStreetAlongZ)
                {
                    if (!LevelMap[z, x].WasHandledByTerrainGenerator)
                    {
                        int streetWidthX = FindStreetEnd(false, z, x, true);
                        int streetWidthZ = FindStreetEnd(true, z, x, true);
                        for (int k = 0; k < streetWidthX; k++)
                        {
                            for (int l = 0; l < streetWidthZ; l++)
                            {
                                vertices[i + (LevelXLength * l) + k] = new Vector3(x + k, y, z + l);
                                LevelMap[z + l, x + k].WasHandledByTerrainGenerator = true;
                                LevelMap[z + l, x + k].y = y;
                            }
                        }
                        slideAlongZOnNextRow = streetWidthZ;
                        slideAlongX += streetWidthX;
                    }
                }
                else
                {
                    vertices[i] = new Vector3(x, y, z);
                    LevelMap[z, x].y = y;
                }
            }
            slideAlongZ += slideAlongZOnNextRow;
            slideAlongZOnNextRow = 0;
            yield return null;
        }
    }
    void CreateMeshTris(int partNumberX, int partNumberZ)
    {
        var subMeshTris = new List<int>[4] { new List<int>(), new List<int>(), new List<int>(), new List<int>() };
        var slideX = (TerrainPartSizeX - 1) * partNumberX;
        var slideZ = (TerrainPartSizeZ - 1) * partNumberZ;
        int vert = 0;
        for (int z = 0; z < TerrainPartSizeZ - 1; z++)
        {
            for (int x = 0; x < TerrainPartSizeX - 1; x++)
            {
                var materialNumber = LevelMap[z + slideZ, x + slideX].MaterialNumber;
                subMeshTris[materialNumber].Add(vert);
                subMeshTris[materialNumber].Add(vert + TerrainPartSizeX);
                subMeshTris[materialNumber].Add(vert + TerrainPartSizeX + 1);
                subMeshTris[materialNumber].Add(vert + TerrainPartSizeX + 1);
                subMeshTris[materialNumber].Add(vert + 1);
                subMeshTris[materialNumber].Add(vert);

                vert++;
            }
            vert++;
        }
        for (int i = 0; i < subMeshTris.Length; i++)
            SubMeshTris[i].Add(subMeshTris[i]);
    }
    int FindStreetEnd(bool isStreetAlongX, int z, int x, bool isCross)
    {
        int answer = 0;
        if (isStreetAlongX)
        {
            while (LevelMap[z + answer, x].IsStreetAlongX && (!isCross || LevelMap[z + answer, x].IsStreetAlongZ))
            {
                answer++;
            }
        }
        else
        {
            while (LevelMap[z, x + answer].IsStreetAlongZ && (!isCross || LevelMap[z, x + answer].IsStreetAlongX))
            {
                answer++;
            }
        }
        return answer;
    }
    private IEnumerator CalculateTerrainPartSize()
    {
        int terrainPartSizeX = MinSize ? TerrainPartSizeMin : TerrainPartSizeMax;
        int terrainPartSizeZ = MinSize ? TerrainPartSizeMin : TerrainPartSizeMax;
        while (TerrainPartSizeX == 0)
            if ((LevelXLength - terrainPartSizeX) % (terrainPartSizeX - 1) == 0)
            {
                TerrainPartSizeX = terrainPartSizeX;
            }
            else
                terrainPartSizeX = MinSize ? terrainPartSizeX + 1 : terrainPartSizeX - 1;
        while (TerrainPartSizeZ == 0)
            if ((LevelZLength - terrainPartSizeZ) % (terrainPartSizeZ - 1) == 0)
            {
                TerrainPartSizeZ = terrainPartSizeZ;
            }
            else
                terrainPartSizeZ = MinSize ? terrainPartSizeZ + 1 : terrainPartSizeZ - 1;
        NumberTerrainMeshPartsX = (LevelXLength - terrainPartSizeX) / (TerrainPartSizeX - 1) + 1;
        NumberTerrainMeshPartsZ = (LevelZLength - terrainPartSizeZ) / (TerrainPartSizeZ - 1) + 1;
        yield return null;
    }
    void CreateTerrainParts(int count)
    {
        for (int i = 0; i < count; i++)
        {
            var terrainPart = new GameObject($"TerrainPart{i}");
            terrainPart.AddComponent<MeshRenderer>();
            terrainPart.AddComponent<MeshCollider>();
            terrainPart.AddComponent<MeshFilter>();
            terrainPart.transform.parent = transform;
            terrainParts.Add(terrainPart);
        }
    }
    private IEnumerator AssembleTerrainParts()
    {
        for (int i = 0, z = 0; z < NumberTerrainMeshPartsZ; z++)
        {
            for (int x = 0; x < NumberTerrainMeshPartsX; x++)
            {
                terrainParts[i + x].transform.position = StartingPoint;
                mesh = new Mesh();
                terrainParts[i + x].GetComponent<MeshFilter>().mesh = mesh;
                CreateMeshTris(x, z);
                UpdateMesh(terrainParts[i + x], x, z);
                yield return null;
            }
            i += NumberTerrainMeshPartsX;
        }
    }

    private void SpawnLevelBorders()
    {
        for (int z = 1; z < LevelZLength - 2; z++)
        {
            Instantiate(LevelWall, new Vector3(
                    0 + 0.05f,
                    (LevelMap[z, 0].y + LevelMap[z, 0 + 1].y) / 2f + 0.5f,
                    z + 0.5f),
                Quaternion.Euler(0f, 180f, 0f));
            Instantiate(LevelWall, new Vector3(
                    LevelXLength - 1 - 0.05f,
                    (LevelMap[z, LevelXLength - 2].y + LevelMap[z, LevelXLength - 1].y) / 2f + 0.5f,
                    z + 0.5f),
                Quaternion.Euler(0f, 0f, 0f));
        }
        for (int x = 1; x < LevelXLength - 2; x++)
        {
            Instantiate(LevelWall, new Vector3(
                    x + 0.5f,
                    (LevelMap[0, x].y + LevelMap[0 + 1, x].y) / 2f + 0.5f,
                    0 + 0.05f),
                Quaternion.Euler(0f, 90f, 0f));
            Instantiate(LevelWall, new Vector3(
                    x + 0.5f,
                    (LevelMap[LevelZLength - 2, x].y + LevelMap[LevelZLength - 1, x].y) / 2f + 0.5f,
                    LevelZLength - 1 - 0.05f),
                Quaternion.Euler(0f, -90f, 0f));
        }
    }
}