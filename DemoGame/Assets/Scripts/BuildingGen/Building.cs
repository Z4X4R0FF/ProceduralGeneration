using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Building
{
    public bool isEven;
    [Range(1, 40)]
    public int minFloorCount;
    [Range(1, 40)]
    public int maxFloorCount;
    [Range(1, 3)]
    public int minFlatCount;
    [Range(1, 3)]
    public int maxFlatCount;
    public List<Flat> flats;
    [Header("Building Parts")] public GameObject porchBase;
    public GameObject porch;
    public GameObject cornerPart;
    public GameObject wallPart;
    public GameObject windowPart;
    public GameObject emptyPart;
    public GameObject cellingCornerPart;
    public GameObject cellingWallPart;
    public GameObject doorFramePart;
    public GameObject roofSidePart;
    public GameObject roofCornerPart;
    public GameObject roofTopPart;
    public GameObject roofPorchPart;
    [Header("Materials")] 
    [SerializeField]
    public BuildingMaterials materials;
}