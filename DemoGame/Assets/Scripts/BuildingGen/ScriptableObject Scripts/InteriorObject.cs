using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "InteriorObject", menuName = "DemoGame/InteriorObject", order = 0)]
public class InteriorObject : ScriptableObject
{
    [SerializeField] public float length;
    [SerializeField] public GameObject prefab;
    [SerializeField] public InteriorObjectType type;
    [SerializeField] public float width;
}