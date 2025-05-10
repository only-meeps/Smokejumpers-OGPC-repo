using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;
using Unity.Cinemachine;
using JetBrains.Annotations;

public class Helicopter : MonoBehaviour
{
    public InputActionAsset inputActions;
    public Vector3 spawnPoint;
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
    private InputAction restart;
    private InputAction pause;
    public Rigidbody rb;
    public float tiltLimiter;
    public float tiltSpeed;
    public float moveSpeedMultiplier;
    public float rotationSpeedMultiplier;
    public float upDownMultiplier;
    public GameObject physicsHeli;
    public CinemachineCamera cinemachineCam;
    private float xAngle;
    private float zAngle;
    public bool engineOn;
    public bool touching;
    private Quaternion startingRot;
    public GameObject cameraPrefab;
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
    public int fireFighters;
    public int citizens;
    public int maxCapacity;
    public GameObject citizenPrefab;
    public GameObject fireFighterPrefab;
    public float fuel;
    public float fuelEfficency;
    public GameObject explosionPrefab;
    bool crashed;
    public GameObject fracturedHeliPrefab;
    public AudioClip crashSound;
    public AudioSource heliAudioSource;
    float initialFuel;
    public MapGenerator mapGenerator;
    private int helicopterRespawnLimiter;
    public AudioClip fuelWarningSound;
    public AudioClip roterSounds;
    public AudioSource roterSoundSource;
    public Animator heliAnimator;
    bool titleScreen;
    public Vector3 titleScreenFollowOffset = new Vector3(4,4,5 ); 
    public Vector3 gameFollowOffset = new Vector3(0,30,0.6f);
    public Vector3 titleScreenFollowRot = new Vector3(25,-158,0);
    public Vector3 gameFollowRot = new Vector3(90,0,0);
    public GameObject pauseUI;
    public List<GameObject> fracturedHeliObjs;
    public AnimationCurve fuelCurve;
    public float maxHeight = 90;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        heliAnimator.speed = 1;
        initialFuel = fuel;
        crashed = false;
        inputs = new HelicopterMovement();
        engineToggle = inputs.General.OnOff_Engine_Toggle;
        tiltF = inputs.General.TiltF;
        tiltB = inputs.General.TiltB;
        tiltR = inputs.General.TiltR;
        tiltL = inputs.General.TiltL;
        rotateR = inputs.General.RotateR;
        rotateL = inputs.General.RotateL;
        up = inputs.General.Up;
        down = inputs.General.Down;
        restart = inputs.General.Restart;
        pause = inputs.General.Pause;
        
        cinemachineCam = Instantiate(cameraPrefab).GetComponent<CinemachineCamera>();
        cinemachineCam.Follow = transform;

        roterSoundSource.clip = roterSounds;
        roterSoundSource.loop = true;
        roterSoundSource.Play();

        mapGenerator = FindFirstObjectByType<MapGenerator>();
        startingRot = new Quaternion(0,0,0,0);
        //Debug.Log("startingRot " +  startingRot);
    }

    public void OnEnable()
    {
        inputs.General.Enable();
        restart.Enable();
        pause.Enable();
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
        inputs.General.Disable();
        restart.Disable();
        pause.Disable();
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

    public void TitleScreen()
    {
        titleScreen = true;
        inputs.Disable();
        cinemachineCam.gameObject.GetComponent<CinemachineFollow>().FollowOffset = titleScreenFollowOffset;
        cinemachineCam.gameObject.transform.eulerAngles = titleScreenFollowRot;
    }
    public void GameView()
    {
        titleScreen = false;
        inputs.Enable();
        cinemachineCam.gameObject.GetComponent<CinemachineFollow>().FollowOffset = gameFollowOffset;
        cinemachineCam.gameObject.transform.eulerAngles = gameFollowRot;
        Debug.Log(cinemachineCam.transform.eulerAngles);
    }
    // Update is called once per frame
    public void FixedUpdate()
    {
        roterSoundSource.volume = PlayerPrefs.GetFloat("HeliFX");
        heliAudioSource.volume = PlayerPrefs.GetFloat("HeliFX");
        if (mapGenerator.waterHeight > transform.position.y && crashed == false)
        {
            Crash();
        }
        helicopterRespawnLimiter++;
        touching = heliCollider.touching;
        //Debug.Log("tiltF : " + tiltF.IsPressed() + " tiltB : " + tiltB.IsPressed() + " tiltR : " + tiltR.IsPressed() + " tiltL : " + tiltL.IsPressed() + " Helicollider.touching : " + touching);
        if ((tiltF.IsPressed() || tiltB.IsPressed() || tiltR.IsPressed() || tiltL.IsPressed() || rotateL.IsPressed() || rotateR.IsPressed()) && crashed == false && heliCollider.crashed == true && heliCollider.landed == false)
        {
            Crash();
        }
        if (touching)
        {

            if (heliCollider.touchingObj.tag == "helipad" && heliCollider.touchingObj.GetComponent<Helipad>().hospital)
            {
                for (int i = 0; i < citizens; i++)
                {
                    System.Random rnd = new System.Random();
                    Citizen citizen = Instantiate(citizenPrefab, entrances[rnd.Next(0, entrances.Count)].transform.position, Quaternion.identity).GetComponent<Citizen>();
                    Vector3 closestHospitalDoor = new Vector3();
                    for (int j = 0; j < GameObject.FindGameObjectsWithTag("Hospital Door").Length; j++)
                    {
                        if (Vector3.Distance(closestHospitalDoor, citizen.transform.position) > Vector3.Distance(GameObject.FindGameObjectsWithTag("Hospital Door")[j].transform.position, citizen.transform.position))
                        {
                            closestHospitalDoor = GameObject.FindGameObjectsWithTag("Hospital Door")[j].transform.position;

                        }
                    }
                    citizen.manuallyAssignedTarget = closestHospitalDoor;
                    citizen.alreadyOnHeli = true;
                    capacity--;
                    citizens--;
                }

            }
            for(int i = 0; i < mapGenerator.fireFighterDropOffPoints.Count; i++)
            {
                if (heliCollider.touching && mapGenerator.fireFighterDropOffPoints[i].Contains(new Vector2(transform.position.x, transform.position.z)))
                {
                    for (int f = 0; f < fireFighters; f++)
                    {
                        System.Random rnd = new System.Random();
                        FireFighter fireFighter = Instantiate(fireFighterPrefab, entrances[rnd.Next(0, entrances.Count)].transform.position, Quaternion.identity).GetComponent<FireFighter>();
                        fireFighter.alreadyOnHeli = true;
                        capacity--;
                        fireFighters--;
                    }

                }
            }

        }
        if (transform.eulerAngles.x > 180)
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
        touching = heliCollider.touching;
        Helipad helipad;
        if(heliCollider.touching == false && !titleScreen)
        {
            fuelEfficency = fuelCurve.Evaluate(transform.position.y / maxHeight) * 4;
        }
        else if(heliCollider.touchingObj.TryGetComponent<Helipad>(out helipad) && fuel < initialFuel && heliCollider.touchingObj)
        {
            if (helipad.gasStation)
            {
                fuelEfficency = -20;
            }
            else
            {
                fuelEfficency = 0;
            }
        }

        else 
        {
            fuelEfficency = 0;
        }/*
        if (heliCollider.touchingObj.name == "helipad" && heliCollider.touchingObj.GetComponent<Helipad>().gasStation)
        {
            fuel += 2;
        }
        */
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
        //Debug.Log(rb.linearVelocity + " " + rb.angularVelocity);
        //Debug.Log(mapGenerator.playableMapSize.Contains(new Vector2(transform.position.x, transform.position.z), true));
        //Debug.Log(new Vector2(transform.position.x, transform.position.z));

        if (engineOn && fuel > 0 && mapGenerator.playableMapSize.Contains(new Vector2(transform.position.x, transform.position.z), true))
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
                if (down.IsPressed())
                {
                    rb.MovePosition(new Vector3(rb.position.x, rb.position.y - upDownMultiplier * Time.deltaTime, rb.position.z));
                }
            }
            if (up.IsPressed())
            {
                //Debug.Log("up");
                rb.transform.position = (new Vector3(rb.position.x, rb.position.y + upDownMultiplier * Time.deltaTime, rb.position.z));
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
                if(xAngle <= -1.5 || xAngle >= 1.5)
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
                

                
                if (zAngle <= -1.5 || zAngle >= 1.5)
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
        else if(fuel <= 0)
        {
            rb.mass = 1;
        }
        else if(!mapGenerator.playableMapSize.Contains(new Vector2(transform.position.x, transform.position.z)) && crashed == false)
        {

            Crash();
        }
        if (engineOn)
        {
            fuel -= fuelEfficency;
        }
        if (heliCollider.touching || fuel <= 0)
        {
            rb.useGravity = true;
            //transform.rotation = rb.rotation;
            if (fuel <= 0 && heliCollider.touching && crashed == false)
            {
                
                Crash();
                rb.freezeRotation = false;
            }
        }
        else
        {
            rb.angularVelocity = Vector3.zero;
            rb.linearVelocity = Vector3.zero;
            //rb.rotation = new Quaternion(0, rb.rotation.y, 0, rb.rotation.w);
            rb.useGravity = false;
        }
        if (restart.IsPressed() && helicopterRespawnLimiter > 100)  
        {

            Debug.Log("Respawn");
            Respawn();
        }
        if(fuel < (initialFuel / 4) && crashed == false && heliAudioSource.isPlaying == false)
        {
            heliAudioSource.clip = fuelWarningSound;
            heliAudioSource.loop = true;
            heliAudioSource.Play();
        }
        else if(heliAudioSource.clip == fuelWarningSound && fuel > (initialFuel / 4) && heliAudioSource.isPlaying)
        {
            heliAudioSource.Stop();
        }

    }
    public void Update()
    {
        if (pause.WasPressedThisFrame())
        {
            if (Time.timeScale == 0)
            {
                pauseUI.SetActive(false);
                Time.timeScale = 1f;
            }
            else
            {
                pauseUI.SetActive(true);
                Time.timeScale = 0f;
            }
        }
        if(fracturedHeliObjs.Count > 2)
        {
            Destroy(fracturedHeliObjs[0]);
        }
    }
    public void Respawn()
    {
        heliAnimator.speed = 1;
        roterSoundSource.clip = roterSounds;
        roterSoundSource.loop = true;
        roterSoundSource.Play();
        rb.angularVelocity = Vector3.zero;
        rb.linearVelocity = Vector3.zero;
        for (int i = 0; i < gameObject.GetComponentsInChildren<MeshRenderer>().Length; i++)
        {
            gameObject.GetComponentsInChildren<MeshRenderer>()[i].enabled = true;
        }
        crashed = false;
        mapGenerator.timesRespawned++;
        capacity = 0;
        citizens = 0;
        fireFighters = 0;
        fuel = initialFuel;
        transform.position = spawnPoint;
        rb.transform.position = spawnPoint;
        rb.transform.rotation = new Quaternion();
        transform.rotation = new Quaternion();
    }

    void Crash()
    {
        heliAnimator.speed = 0f;
        roterSoundSource.Stop();
        fuel = 0;
        crashed = true;
        capacity = 0;
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        GameObject fracturedHeli = Instantiate(fracturedHeliPrefab, transform.position, Quaternion.identity);
        for(int i = 0; i < fracturedHeli.GetComponentsInChildren<Rigidbody>().Length; i++)
        {
            fracturedHeli.GetComponentsInChildren<Rigidbody>()[i].AddExplosionForce(30, transform.position, 100);
        }
        for(int i = 0; i < gameObject.GetComponentsInChildren<MeshRenderer>().Length; i++)
        {
            gameObject.GetComponentsInChildren<MeshRenderer>()[i].enabled = false;
        }
        fracturedHeli.transform.eulerAngles = gameObject.transform.eulerAngles;
        fracturedHeliObjs.Add(fracturedHeli);
        heliAudioSource.Stop();
        heliAudioSource.clip = crashSound;
        heliAudioSource.loop = false;
        heliAudioSource.Play();
    }
}
