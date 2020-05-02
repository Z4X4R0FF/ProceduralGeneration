using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Room", menuName = "DemoGame/Room", order = 0)]
public class Room : ScriptableObject
{
    [SerializeField] public int minXLength;
    [SerializeField] public int minZLength;
    [SerializeField] public new string name;
    [SerializeField] public RoomTypes roomType;
    [SerializeField] public GameObject wall;
    [SerializeField] public GameObject doorFrame;
    [SerializeField] public bool isPassThrough;
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