using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InteriorObjectType", menuName = "DemoGame/InteriorObjectType", order = 0)]
public class InteriorObjectType : ScriptableObject {
    [SerializeField]
    public string Name;
    [SerializeField]
    public List<RoomTypes> EnabledRooms;
    
}
