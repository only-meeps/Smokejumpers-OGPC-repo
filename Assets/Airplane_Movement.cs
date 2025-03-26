using UnityEngine;
using UnityEngine.InputSystem;

public class Airplane_Movement : MonoBehaviour
{
    public InputActionAsset action;
    public Plane_Movement planeMovement;
    public InputAction up;
    public InputAction down;
    public InputAction left;
    public InputAction right;
    public InputAction throttleUp;
    public InputAction throttleDown;
    public Rigidbody rb;
    public float speed;
    public float pullUpMultiplier = 0.5f;
    public float pullDownMultiplier = 0.5f;
    public float targetHeight;
    public float targetRotation;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        planeMovement = new Plane_Movement();

    }
    public void OnEnable()
    {
        up = planeMovement.Fireplane.Up;
        down = planeMovement.Fireplane.Down;
        left = planeMovement.Fireplane.Left;
        right = planeMovement.Fireplane.Right;
        throttleUp = planeMovement.Fireplane.Throttleup;
        throttleDown = planeMovement.Fireplane.Throttledown;
        up.Enable();
        down.Enable();
        left.Enable();
        right.Enable();
        throttleUp.Enable();
        throttleDown.Enable();
    }
    public void OnDisable()
    {
        up = planeMovement.Fireplane.Up;
        down = planeMovement.Fireplane.Down;
        left = planeMovement.Fireplane.Left;
        right = planeMovement.Fireplane.Right;
        throttleUp = planeMovement.Fireplane.Throttleup;
        throttleDown = planeMovement.Fireplane.Throttledown;
        up.Disable();
        down.Disable();
        left.Disable();
        right.Disable();
        throttleUp.Disable();
        throttleDown.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        if (up.IsPressed())
        {
            targetHeight += .01f;
        }
        if(down.IsPressed())
        {
            targetHeight -= .01f;
        }
        if(left.IsPressed())
        {
            targetRotation -= 0.01f;
        }
        if(right.IsPressed())
        {
            targetRotation += 0.01f;
        }
        Debug.Log("Rotation" + transform.rotation + "Position" + transform.position);
        transform.Translate(Vector3.left * speed);
        if(targetHeight > transform.position.y - 2 && targetHeight < transform.position.y + 2)
        {
            transform.rotation = new Quaternion();
        }
        else if(targetHeight > transform.position.y)
        {
            if(transform.rotation.z < .2)
            {
                transform.Rotate(Vector3.back * 1f);
            }
        }
        else if(targetHeight < transform.position.y)
        {
            if(transform.rotation.z > -.2)
            {
                transform.Rotate(Vector3.forward * 1f);
            }
        }
    }
}
