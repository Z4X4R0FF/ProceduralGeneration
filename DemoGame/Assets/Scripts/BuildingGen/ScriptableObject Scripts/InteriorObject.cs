using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InteriorObject", menuName = "DemoGame/InteriorObject", order = 0)]
public class InteriorObject : ScriptableObject {
    [SerializeField]
    public GameObject Prefab;
    [SerializeField]
    public InteriorObjectType Type;
    [SerializeField]
    public float Length;
    [SerializeField]
    public float Width;
}
