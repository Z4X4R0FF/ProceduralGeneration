using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Room", menuName = "DemoGame/Room", order = 0)]
public class Room : ScriptableObject
{
    [SerializeField]
    public string Name;
    [SerializeField]
    public RoomTypes RoomType;
    [SerializeField]
    public int minXLength;
    [SerializeField]
    public int minZLength;
    [SerializeField]
    public int maxXLength;
    [SerializeField]
    public int maxZLength;
    [SerializeField]
    public GameObject Wall;
}
public enum RoomTypes
{
    LivingRoom,
    BedRoom,
    BathRoom,
    StorageRoom,
    Corridor,
    Kitchen
}