using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public struct Building
{
    public bool isEven;
    [Range(1, 40)] public int minFloorCount;
    [Range(1, 40)] public int maxFloorCount;
    [Range(1, 3)] public int minFlatCount;
    [Range(1, 3)] public int maxFlatCount;
    public List<Flat> flats;
    [Header("Building Parts")] public GameObject porchBase;
    public GameObject porch;
    public GameObject cornerPart;
    public GameObject wallPart;
    public GameObject windowPart;
    public GameObject cellingCornerPart;
    public GameObject cellingWallPart;
    public GameObject doorFramePart;
    public GameObject roofSidePart;
    public GameObject roofCornerPart;
    public GameObject roofTopPart;
    public GameObject roofPorchPart;
    [Header("Materials")] [SerializeField] public Material basementPrimaryMaterial;
    [SerializeField] public Material basementSecondaryMaterial;
    [SerializeField] public Material structurePrimaryMaterial;
    [SerializeField] public Material structureSecondaryMaterial;
    [SerializeField] public Material roofMaterial;

    [SerializeField] public Material windowMaterial;

    public List<Material> MaterialList
    {
        get
        {
            if (_materialList != null) return _materialList;
            _materialList = new List<Material>
            {
                basementPrimaryMaterial,
                basementSecondaryMaterial,
                structurePrimaryMaterial,
                structureSecondaryMaterial,
                roofMaterial,
                windowMaterial,
            };
            _materialList.AddRange(flats.Select(r => r.RoomList).SelectMany(r => r).ToList()
                .SelectMany(r => r.MaterialList).Distinct().ToList());

            return _materialList;
        }
    }

    public List<Material> FlatMaterialList
    {
        get
        {
            if (_flatMaterialList != null) return _flatMaterialList;
            _flatMaterialList = new List<Material>();
            var obj = flats.SelectMany(r => r.RoomList).ToList();
            var obj2 = obj.SelectMany(r => r.MaterialList).ToList();
            _flatMaterialList = obj2.Distinct().ToList();

            return _flatMaterialList;
        }
    }


    private List<Material> _materialList;
    private List<Material> _flatMaterialList;
}