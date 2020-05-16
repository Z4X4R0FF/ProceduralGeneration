using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlatMapPoint
{
    public RoomTypes RoomType;
    public int RoomNumber = 0;
    public int FlatNumber;
    public WallTypes WallZUp;
    public WallTypes WallZDown;
    public WallTypes WallXUp;
    public WallTypes WallXDown;
    public bool IsPorch = false;
}

public enum WallTypes
{
    Empty=0,
    Wall=1,
    DoorFrame=2,
    Window=3
}
