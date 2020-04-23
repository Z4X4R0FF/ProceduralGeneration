using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "InteriorObjectType", menuName = "DemoGame/InteriorObjectType", order = 0)]
public class InteriorObjectType : ScriptableObject
{
    [SerializeField] public List<RoomTypes> enabledRooms;
    [SerializeField] public new string name;
}