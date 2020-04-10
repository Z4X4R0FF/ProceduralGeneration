using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentLoader : MonoBehaviour
{
    public MeshFilter[] MeshFilters;
    public MeshRenderer[] MeshRenderers;
    private void Awake()
    {
        MeshFilters = GetComponentsInChildren<MeshFilter>();
        MeshRenderers = GetComponentsInChildren<MeshRenderer>();
    }
}
