using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public struct Building
{
    public bool IsEven;
    public int MinFloorCount;
    public int MaxFloorCount;
    public GameObject PorchBase;
    public GameObject Porch;
    public GameObject CornerPart;
    public GameObject WallPart;
    public GameObject WindowPart;
    public GameObject EmptyPart;
    public GameObject CellingCornerPart;
    public GameObject CellingWallPart;
    public GameObject DoorFramePart;
    public GameObject RoofSidePart;
    public GameObject RoofCornerPart;
    public GameObject RoofTopPart;
    public GameObject RoofPorchPart;
    public List<Material> Materials;
}