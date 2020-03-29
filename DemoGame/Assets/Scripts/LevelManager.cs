using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public TerrainGenerator GenerationManager;
    public GameObject Player;
    public bool PlayerSpawned = false;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (GenerationManager.IsDone && !PlayerSpawned)
        {
            Instantiate(Player, new Vector3(10, 50, 10), Quaternion.identity);
            PlayerSpawned=true;
        }
    }
}