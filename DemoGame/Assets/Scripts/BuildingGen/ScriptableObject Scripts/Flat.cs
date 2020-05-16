using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Flat", menuName = "DemoGame/Flat", order = 0)]
public class Flat : ScriptableObject
{
    [SerializeField] public Room bathRoom;
    [SerializeField] public Room bedRoom;
    [SerializeField] public List<Room> additionalBedRooms;
    [SerializeField] public List<Room> additionalLivingRooms;
    [SerializeField] public Room corridor;
    [SerializeField] public Room kitchen;
    [SerializeField] public Room livingRoom;
    [SerializeField] public List<Room> storageRooms;

    public List<Room> RoomList
    {
        get
        {
            if (_roomList.Any()) return _roomList;
            _roomList = new List<Room> {bathRoom, bedRoom, corridor, kitchen, livingRoom};
            _roomList.AddRange(additionalBedRooms);
            _roomList.AddRange(additionalLivingRooms);
            _roomList.AddRange(storageRooms);

            return _roomList;
        }
    }

    private List<Room> _roomList=null;

}