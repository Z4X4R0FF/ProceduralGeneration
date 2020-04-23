using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public struct StreetsToGenerate
{
    [FormerlySerializedAs("Width")] public int width;
    [FormerlySerializedAs("Count")] public int count;

    [FormerlySerializedAs("StreetRadius")]
    [Tooltip("Number of squares that will be banned for placing another streets")]
    public int streetRadius;
}