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
    public bool DoorFrameZUp = false;
    public bool DoorFrameZDown = false;
    public bool DoorFrameXUp = false;
    public bool DoorFrameXDown = false;
    public List<InteriorObject> InteriorObjects;
    public bool IsPorch = false;
}