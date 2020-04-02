using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class Building
{
    public bool IsEven;
    public int MinFloorCount;
    public int MaxFloorCount;
    //public int MinAisleSize;
    //public int MaxAisleSize;
    public GameObject PorchBase;
    public GameObject Porch;
    public GameObject CornerPart;
    public GameObject WallPart;
    public GameObject WindowPart;
    public GameObject EmptyPart;
    public GameObject CellingCornerPart;
    public GameObject CellingWallPart;
    public List<Material> Materials;
}