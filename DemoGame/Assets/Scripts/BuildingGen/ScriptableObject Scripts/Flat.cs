using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Flat", menuName = "DemoGame/Flat", order = 0)]
public class Flat : ScriptableObject
{
    [SerializeField]
    public List<Room> LivingRooms;
    [SerializeField]
    public List<Room> BedRooms;
    [SerializeField]
    public Room Kitchen;
    [SerializeField]
    public List<Room> WashingRooms;
    [SerializeField]
    public List<Room> StorageRooms;
    [SerializeField]
    public Room Corridor;
}

