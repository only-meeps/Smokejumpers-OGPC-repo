using TreeEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class Helicopter : MonoBehaviour
{
    public InputActionAsset inputActions;
    public HelicopterMovement inputs;
    private InputAction tiltF;
    private InputAction tiltB;
    private InputAction tiltR;
    private InputAction tiltL;
    private InputAction rotateR;
    private InputAction rotateL;
    private InputAction up;
    private InputAction down;
    private InputAction engineToggle;
    public Rigidbody rb;
    public float tiltLimiter;
    public float tiltSpeed;
    public float moveSpeedMultiplier;
    public float rotationSpeedMultiplier;
    public float upDownMultiplier;
    public GameObject physicsHeli;
    private float xAngle;
    private float zAngle;
    public bool engineOn;
    public bool touching;
    private Quaternion startingRot;
    public GameObject camera;
    public float cameraMoveSpeed;
    public BoxCollider boxCollider;
    public float raycastInterval;
    public float altitudeTarget;
    public float thrust;
    public HeliCollider heliCollider;
    public int citizensRescued;
    public int citizensKilled;
    public int citizensDiedInFire;
    public int citizensLeft;
    public List<GameObject> entrances = new List<GameObject>();
    public int capacity;
    public int maxCapacity;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        startingRot = new Quaternion(0,0,0,0);
        Debug.Log("startingRot " +  startingRot);
        inputs = new HelicopterMovement();
    }

    public void OnEnable()
    {
        engineToggle = inputs.General.OnOff_Engine_Toggle;
        tiltF = inputs.General.TiltF;
        tiltB = inputs.General.TiltB;
        tiltR = inputs.General.TiltR;
        tiltL = inputs.General.TiltL;
        rotateR = inputs.General.RotateR;
        rotateL = inputs.General.RotateL;
        up = inputs.General.Up;
        down = inputs.General.Down;
        inputs.General.Enable();
        engineToggle.Enable();
        tiltF.Enable();
        tiltB.Enable();
        tiltR.Enable();
        tiltL.Enable();
        rotateR.Enable();
        rotateL.Enable();
        up.Enable();
        down.Enable();
    }
    public void OnDisable()
    {
        engineToggle = inputs.General.OnOff_Engine_Toggle;
        tiltF = inputs.General.TiltF;
        tiltB = inputs.General.TiltB;
        tiltR = inputs.General.TiltR;
        tiltL = inputs.General.TiltL;
        rotateR = inputs.General.RotateR;
        rotateL = inputs.General.RotateL;
        up = inputs.General.Up;
        down = inputs.General.Down;
        inputs.General.Disable();
        engineToggle.Disable();
        tiltF.Disable();
        tiltB.Disable();
        tiltR.Disable();
        tiltL.Disable();
        rotateR.Disable();
        rotateL.Enable();
        up.Disable();
        down.Disable();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        citizensLeft = GameObject.FindGameObjectsWithTag("Citizen").Length;
        if(citizensLeft == 0)
        {
            if(citizensDiedInFire == 0 && citizensKilled == 0)
            {
                Debug.Log("You finished the map with a perfect score of " + citizensRescued * 100);
            }
            else if(citizensKilled > 0 && citizensDiedInFire == 0)
            {
                Debug.Log("You finished the map with a score of " + ((citizensRescued * 100) - (citizensDiedInFire * 50) - (citizensKilled * 200)) + ". You rescued " + citizensRescued + " citizens, and killed " + citizensKilled + " citizens");
            }
            else if(citizensDiedInFire > 0 && citizensKilled == 0)
            {
                Debug.Log("You finished the map with a score of " + ((citizensRescued * 100) - (citizensDiedInFire * 50) - (citizensKilled * 200)) + ". You rescued " + citizensRescued + " and unfortunatly missed " + citizensDiedInFire + " citizens who died in a fire");
            }
            else if(citizensKilled == 0 && citizensRescued == 0 && citizensDiedInFire > 0)
            {
                Debug.Log("Are you actually going to play the game?");
            }
            else
            {
                Debug.Log("You finished the map with a score of " + ((citizensRescued * 100) - (citizensDiedInFire * 50) - (citizensKilled * 200)) + ". You rescued " + citizensRescued + " citizens, killed " + citizensKilled + " citizens and missed " + citizensDiedInFire + " citizens who died in a fire");
            }        
        }
        if (heliCollider.touchingObj != null)
        {
            if (heliCollider.touchingObj.name == "helipad")
            {
                capacity = 0;
            }
        }
        touching = heliCollider.touching;
        if(transform.eulerAngles.x > 180)
        {
            xAngle = (transform.eulerAngles.x - 360);
        }
        else
        {
            xAngle = transform.eulerAngles.x;
        }
        if (transform.eulerAngles.z > 180)
        {
            zAngle = (transform.eulerAngles.z - 360);
        }
        else
        {
            zAngle = transform.eulerAngles.z;
        }
        if (engineToggle.IsPressed())
        {
            if (engineOn)
            {
                engineOn = false;
            }
            else
            {
                engineOn = true;
            }
        }


        if (engineOn)
        {

            if (!heliCollider.touching)
            {
                if (tiltF.IsPressed() && xAngle <= tiltLimiter)
                {
                    if (!heliCollider.touching)
                    {
                        transform.Rotate(Vector3.right * tiltSpeed * Time.deltaTime);
                    }

                }
                else if (tiltB.IsPressed() && xAngle >= -tiltLimiter)
                {
                    if (!heliCollider.touching)
                    {
                        transform.Rotate(Vector3.left * tiltSpeed * Time.deltaTime);
                    }
                }

                if (tiltL.IsPressed() && zAngle <= tiltLimiter)
                {
                    if (!heliCollider.touching)
                    {
                        transform.Rotate(Vector3.forward * tiltSpeed * Time.deltaTime);
                    }
                }
                else if (tiltR.IsPressed() && zAngle >= -tiltLimiter)
                {
                    if (!heliCollider.touching)
                    {
                        transform.Rotate(Vector3.back * tiltSpeed * Time.deltaTime);
                    }
                }

                if (down.IsPressed())
                {
                    if (!heliCollider.touching)
                    {
                        //physicsHeli.transform.Translate(Vector3.down * upDownMultiplier * Time.deltaTime);
                        rb.MovePosition(new Vector3(rb.position.x,rb.position.y - upDownMultiplier * Time.deltaTime, rb.position.z));
                    }
                }
                if (rotateR.IsPressed())
                {
                    if (!heliCollider.touching)
                    {
                        physicsHeli.transform.Rotate(Vector3.up * rotationSpeedMultiplier * Time.deltaTime);
                    }
                }
                if (rotateL.IsPressed())
                {
                    if (!heliCollider.touching)
                    {
                        physicsHeli.transform.Rotate(Vector3.down * rotationSpeedMultiplier * Time.deltaTime);
                    }
                }
            }
            if (up.IsPressed())
            {
                rb.MovePosition(new Vector3(rb.position.x, rb.position.y + upDownMultiplier * Time.deltaTime, rb.position.z));
            }


            if (xAngle > 0)
            {
                physicsHeli.transform.Translate(Vector3.forward * xAngle * moveSpeedMultiplier * Time.deltaTime);
            }
            if (xAngle < 0)
            {
                physicsHeli.transform.Translate(Vector3.forward * xAngle * moveSpeedMultiplier * Time.deltaTime);
            }
            if (zAngle > 0)
            {
                physicsHeli.transform.Translate(Vector3.left * zAngle * moveSpeedMultiplier * Time.deltaTime);
            }
            if (zAngle < 0)
            {
                physicsHeli.transform.Translate(Vector3.left * zAngle * moveSpeedMultiplier * Time.deltaTime);
            }
            
            transform.rotation = new Quaternion(transform.rotation.x, physicsHeli.transform.rotation.y, transform.rotation.z, transform.rotation.w);
            if(!tiltF.IsPressed() && !tiltB.IsPressed() && !tiltL.IsPressed() && !tiltR.IsPressed() && !touching)
            {
                if(xAngle <= -1 || xAngle >= 1)
                {
                    if (xAngle > 0)
                    {
                        transform.Rotate(Vector3.left * tiltSpeed * Time.deltaTime);
                    }
                    if (xAngle < 0)
                    {
                        transform.Rotate(Vector3.right * tiltSpeed * Time.deltaTime);
                    }
                }
                else
                {
                    transform.rotation = new Quaternion(0, transform.rotation.y, transform.rotation.z, transform.rotation.w);
                }
                if (zAngle <= -1 || zAngle >= 1)
                {
                    if (zAngle > 0)
                    {
                        transform.Rotate(Vector3.back * tiltSpeed * Time.deltaTime);
                    }
                    if (zAngle < 0)
                    {
                        transform.Rotate(Vector3.forward * tiltSpeed * Time.deltaTime);
                    }
                }
                else
                {
                    transform.rotation = new Quaternion(transform.rotation.x, transform.rotation.y, 0, transform.rotation.w);
                }
            }
            

        }

        if(physicsHeli.transform.position.x > camera.transform.position.x + 15)
        {
            camera.transform.Translate(Vector3.right * cameraMoveSpeed * Time.deltaTime);
        }
        if(physicsHeli.transform.position.x < camera.transform.position.x - 15)
        {
            camera.transform.Translate(Vector3.left * cameraMoveSpeed * Time.deltaTime);
        }
        if(physicsHeli.transform.position.z > camera.transform.position.z +15)
        {
            camera.transform.Translate(Vector3.up * cameraMoveSpeed * Time.deltaTime);
        }
        if(physicsHeli.transform.position.z < camera.transform.position.z -15)
        {
            camera.transform.Translate(Vector3.down * cameraMoveSpeed * Time.deltaTime);
        }
        camera.transform.position = new Vector3(camera.transform.position.x, physicsHeli.transform.position.y + 40, camera.transform.position.z);
        for(float x = 0; x < boxCollider.size.x; x+=raycastInterval)
        {
            for(float z = 0; z < boxCollider.size.z; z+=raycastInterval)
            {
                RaycastHit hit;
                if (Physics.Raycast(new Vector3(x + (boxCollider.transform.position.x) - boxCollider.size.x / 2 ,  boxCollider.transform.position.y, z + (boxCollider.transform.position.z) - boxCollider.size.z / 2) , Vector3.down, out hit, 1f))
                {
                    Debug.DrawLine(new Vector3(x + (boxCollider.transform.position.x ) - boxCollider.size.x / 2, boxCollider.transform.position.y, z + (boxCollider.transform.position.z) - boxCollider.size.z / 2), hit.point, Color.green);
                }
            }
        }
        if (heliCollider.touching)
        {
            rb.useGravity = true;
        }
        else
        {
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
