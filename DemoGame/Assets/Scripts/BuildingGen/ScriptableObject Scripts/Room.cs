using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Room", menuName = "DemoGame/Room", order = 0)]
public class Room : ScriptableObject
{
    [SerializeField] public GameObject wall;
    [SerializeField] public GameObject doorFrame;
    [SerializeField] public GameObject windowFrame;
    [SerializeField] public GameObject floor;
    [SerializeField] public GameObject ceiling;
    
    [SerializeField] public Material wallMaterial;
    [SerializeField] public Material floorMaterial;
    [SerializeField] public Material ceilingMaterial;

    public List<Material> MaterialList =>
        new List<Material>
        {
            wallMaterial, floorMaterial, ceilingMaterial
        };
}

public enum RoomTypes
{
    EmptyRoom = 0,
    LivingRoom = 1,
    BedRoom = 2,
    BathRoom = 3,
    StorageRoom = 4,
    Corridor = 5,
    Kitchen = 6
}