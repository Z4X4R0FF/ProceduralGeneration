using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Flat", menuName = "DemoGame/Flat", order = 0)]
public class Flat : ScriptableObject
{
    [SerializeField] public List<Room> bathRooms;
    [SerializeField] public List<Room> bedRooms;
    [SerializeField] public Room corridor;
    [SerializeField] public Room kitchen;
    [SerializeField] public List<Room> livingRooms;
    [SerializeField] public List<Room> storageRooms;
}