using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public enum RotationAxes
    {
        MouseXAndY = 0,
        MouseX = 1,
        MouseY = 2
    }

    private Quaternion _originalRotation;
    private Quaternion _parentRotation;
    private readonly List<float> _rotArrayX = new List<float>();
    private readonly List<float> _rotArrayY = new List<float>();
    private float _rotationX;
    private float _rotationY;
    private float _rotAverageX;
    private float _rotAverageY;

    [Header("Settings")] public RotationAxes axes = RotationAxes.MouseXAndY;

    public float frameCounter = 20;
    public float maximumX = 360F;
    public float maximumY = 60F;
    public float minimumX = -360F;
    public float minimumY = -60F;

    [Header("links")] public Transform parent;

    public float sensitivityX = 15F;
    public float sensitivityY = 15F;

    private void Update()
    {
        if (axes == RotationAxes.MouseXAndY)
        {
            //Resets the average rotation
            _rotAverageY = 0f;
            _rotAverageX = 0f;

            //Gets rotational input from the mouse
            _rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
            _rotationX += Input.GetAxis("Mouse X") * sensitivityX;

            //Adds the rotation values to their relative array
            _rotArrayY.Add(_rotationY);
            _rotArrayX.Add(_rotationX);

            //If the arrays length is bigger or equal to the value of frameCounter remove the first value in the array
            if (_rotArrayY.Count >= frameCounter) _rotArrayY.RemoveAt(0);
            if (_rotArrayX.Count >= frameCounter) _rotArrayX.RemoveAt(0);

            //Adding up all the rotational input values from each array
            for (var j = 0; j < _rotArrayY.Count; j++) _rotAverageY += _rotArrayY[j];
            for (var i = 0; i < _rotArrayX.Count; i++) _rotAverageX += _rotArrayX[i];

            //Standard maths to find the average
            _rotAverageY /= _rotArrayY.Count;
            _rotAverageX /= _rotArrayX.Count;

            //Clamp the rotation average to be within a specific value range
            _rotAverageY = ClampAngle(_rotAverageY, minimumY, maximumY);
            _rotAverageX = ClampAngle(_rotAverageX, minimumX, maximumX);

            //Get the rotation you will be at next as a Quaternion
            var yQuaternion = Quaternion.AngleAxis(_rotAverageY, Vector3.left);
            var xQuaternion = Quaternion.AngleAxis(_rotAverageX, Vector3.up);

            //Rotate
            transform.localRotation = _originalRotation * yQuaternion;
            parent.localRotation = _parentRotation * xQuaternion;
        }
        else if (axes == RotationAxes.MouseX)
        {
            _rotAverageX = 0f;
            _rotationX += Input.GetAxis("Mouse X") * sensitivityX;
            _rotArrayX.Add(_rotationX);
            if (_rotArrayX.Count >= frameCounter) _rotArrayX.RemoveAt(0);
            for (var i = 0; i < _rotArrayX.Count; i++) _rotAverageX += _rotArrayX[i];
            _rotAverageX /= _rotArrayX.Count;
            _rotAverageX = ClampAngle(_rotAverageX, minimumX, maximumX);
            var xQuaternion = Quaternion.AngleAxis(_rotAverageX, Vector3.up);

            parent.localRotation = _parentRotation * xQuaternion;
        }
        else
        {
            _rotAverageY = 0f;
            _rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
            _rotArrayY.Add(_rotationY);
            if (_rotArrayY.Count >= frameCounter) _rotArrayY.RemoveAt(0);
            for (var j = 0; j < _rotArrayY.Count; j++) _rotAverageY += _rotArrayY[j];
            _rotAverageY /= _rotArrayY.Count;
            _rotAverageY = ClampAngle(_rotAverageY, minimumY, maximumY);
            var yQuaternion = Quaternion.AngleAxis(_rotAverageY, Vector3.left);
            transform.localRotation = _originalRotation * yQuaternion;
        }
    }

    private void Start()
    {
        var rb = GetComponent<Rigidbody>();
        if (rb)
            rb.freezeRotation = true;
        _originalRotation = transform.localRotation;
        _parentRotation = parent.localRotation;

        Cursor.lockState = CursorLockMode.Locked;
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        angle = angle % 360;
        if (angle >= -360F && angle <= 360F)
        {
            if (angle < -360F) angle += 360F;
            if (angle > 360F) angle -= 360F;
        }

        return Mathf.Clamp(angle, min, max);
    }
}