using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrowRoomDirection
{
    public bool DirectionXUp;
    public bool DirectionXDown;
    public bool DirectionZUp;
    public bool DirectionZDown;
    public bool CanGrow => DirectionXUp || DirectionXDown || DirectionZUp || DirectionZDown;
}
