using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class BuildingMaterials
{
    [FormerlySerializedAs("BasementPrimaryMaterial")] [SerializeField] public Material basementPrimaryMaterial;
    [FormerlySerializedAs("BasementSecondaryMaterial")] [SerializeField] public Material basementSecondaryMaterial;
    [FormerlySerializedAs("StructurePrimaryMaterial")] [SerializeField] public Material structurePrimaryMaterial;
    [FormerlySerializedAs("StructureSecondaryMaterial")] [SerializeField] public Material structureSecondaryMaterial;
    [FormerlySerializedAs("RoofMaterial")] [SerializeField] public Material roofMaterial;
    [FormerlySerializedAs("WindowMaterial")] [SerializeField] public Material windowMaterial;

    //floor needs to move on Flat Section
    [FormerlySerializedAs("FloorMaterial")] [SerializeField] public Material floorMaterial;
    [FormerlySerializedAs("RoomMaterial")] [SerializeField] public Material roomMaterial;

    public List<Material> MaterialList =>
        _materialList ?? (_materialList = new List<Material>
        {
            basementPrimaryMaterial,
            basementSecondaryMaterial,
            structurePrimaryMaterial,
            structureSecondaryMaterial,
            roofMaterial,
            windowMaterial,
            floorMaterial,
            roomMaterial
        });

    private List<Material> _materialList;
}