using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StreetsToGenerate
{
    public int Width;
    public int Count;
    [Tooltip("Number of squares that will be banned for placing another streets")]
    public int StreetRadius;
}
