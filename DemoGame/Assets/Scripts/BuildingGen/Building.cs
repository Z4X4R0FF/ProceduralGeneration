using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public struct Building
{
    public bool isEven;
    public int minFloorCount;
    public int maxFloorCount;
    public int minFlatCount;
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
    [Header("Materials")] public List<Material> materials;
}