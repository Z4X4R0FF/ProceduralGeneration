using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public void LoadLevel(int sceneIndex)
    {
        StartCoroutine(LoadAsynchronously(sceneIndex));
    }

    private IEnumerator LoadAsynchronously(int sceneIndex)
    {
        //GameObject.Find("GenerationManager").GetComponent<TerrainGenerator>().DoBigProcess();
        var operation = SceneManager.LoadSceneAsync(sceneIndex);
        while (!operation.isDone)
        {
            Debug.Log(operation.progress);
            yield return null;
        }
    }
}