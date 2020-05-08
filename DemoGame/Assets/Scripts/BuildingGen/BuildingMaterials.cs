using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public class BuildingMaterials
{
    [SerializeField] public Material BasementPrimaryMaterial;
    [SerializeField] public Material BasementSecondaryMaterial;
    [SerializeField] public Material StructurePrimaryMaterial;
    [SerializeField] public Material StructureSecondaryMaterial;
    [SerializeField] public Material RoofMaterial;
    [SerializeField] public Material WindowMaterial;

    //floor needs to move on Flat Section
    [SerializeField] public Material FloorMaterial;
    [SerializeField] public Material RoomMaterial;

    public List<Material> MaterialList =>
        _materialList ?? (_materialList = new List<Material>
        {
            BasementPrimaryMaterial,
            BasementSecondaryMaterial,
            StructurePrimaryMaterial,
            StructureSecondaryMaterial,
            RoofMaterial,
            WindowMaterial,
            FloorMaterial,
            RoomMaterial
        });

    private List<Material> _materialList;
}