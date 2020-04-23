using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlatMapPoint
{
    public RoomTypes RoomType;
    public int FlatNumber;
    public GameObject WallZUp;
    public GameObject WallZDown;
    public GameObject WallXUp;
    public GameObject WallXDown;
    public List<InteriorObject> InteriorObjects;
    public bool IsPorch = false;
}
