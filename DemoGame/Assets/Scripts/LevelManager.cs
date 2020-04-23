using UnityEngine;
using UnityEngine.Serialization;

public class LevelManager : MonoBehaviour
{
    [FormerlySerializedAs("GenerationManager")]
    public TerrainGenerator generationManager;

    [FormerlySerializedAs("Player")] public GameObject player;

    [FormerlySerializedAs("PlayerSpawned")]
    public bool playerSpawned;
    // Start is called before the first frame update

    // Update is called once per frame
    private void Update()
    {
        if (generationManager.isDone && !playerSpawned)
        {
            Instantiate(player, new Vector3(10, 50, 10), Quaternion.identity);
            playerSpawned = true;
        }
    }
}