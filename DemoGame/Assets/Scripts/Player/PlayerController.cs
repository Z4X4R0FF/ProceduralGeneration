using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Start is called before the first frame update
    private readonly float _moveSpeed = 0.05f;

    // Update is called once per frame
    private void Update()
    {
        var moveHorizontal = Input.GetAxis("Horizontal");
        var moveVertical = Input.GetAxis("Vertical");
        transform.Translate(moveHorizontal * _moveSpeed, 0, moveVertical * _moveSpeed);
    }
}