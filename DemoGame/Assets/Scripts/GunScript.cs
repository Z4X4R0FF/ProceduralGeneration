using UnityEngine;

public class GunScript : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0)) Fire();
    }

    private void Fire()
    {
    }
}