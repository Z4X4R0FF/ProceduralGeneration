using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Room", menuName = "DemoGame/Room", order = 0)]
public class Room : ScriptableObject
{
    [SerializeField] public int maxXLength;
    [SerializeField] public int maxZLength;
    [SerializeField] public int minXLength;
    [SerializeField] public int minZLength;
    [SerializeField] public new string name;
    [SerializeField] public RoomTypes roomType;
    [SerializeField] public GameObject wall;
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