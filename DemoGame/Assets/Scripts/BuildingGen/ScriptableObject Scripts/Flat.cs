using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Flat", menuName = "DemoGame/Flat", order = 0)]
public class Flat : ScriptableObject
{
    [SerializeField] public List<Room> bathRooms;
    [SerializeField] public Room bedRoom;
    [SerializeField] public List<Room> additionalBedRooms;
    [SerializeField] public List<Room> additionalLivingRooms;
    [SerializeField] public Room corridor;
    [SerializeField] public Room kitchen;
    [SerializeField] public Room livingRooms;
    [SerializeField] public List<Room> storageRooms;

    public int AdditionalRoomsCount =>
        bathRooms.Count + storageRooms.Count + additionalBedRooms.Count + additionalLivingRooms.Count;
}