using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlatMapPoint
{
    public RoomTypes RoomType;
    public int RoomNumber = 0;
    public int FlatNumber;
    public bool WallZUp = false;
    public bool WallZDown = false;
    public bool WallXUp = false;
    public bool WallXDown = false;
    public List<InteriorObject> InteriorObjects;
    public bool IsPorch = false;
}