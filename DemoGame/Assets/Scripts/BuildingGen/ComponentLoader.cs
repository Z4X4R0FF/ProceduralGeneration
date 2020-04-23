using UnityEngine;
using UnityEngine.Serialization;

public class ComponentLoader : MonoBehaviour
{
    [FormerlySerializedAs("MeshFilters")] public MeshFilter[] meshFilters;

    [FormerlySerializedAs("MeshRenderers")]
    public MeshRenderer[] meshRenderers;

    private void Awake()
    {
        meshFilters = GetComponentsInChildren<MeshFilter>();
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
    }
}