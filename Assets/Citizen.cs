using UnityEngine;
using UnityEngine.AI;

public class Citizen : MonoBehaviour
{
    public NavMeshAgent agent;
    public Helicopter helicopter;
    public Fire_Spread Fire_Spread;
    public MapGenerator MapGenerator;
    public int townIndex;
    public Vector3 startingPos;
    public bool touchingDoor;
    public bool touchingHeli;
    public bool touchingHospitalDoor;
    public bool alreadyOnHeli;
    public Vector3 manuallyAssignedTarget;
    public bool usingFireSpread;
    public Rigidbody rb;
    public int burnTime;
    public int maxBurnTime;
    System.Random rnd = new System.Random();
    public Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        maxBurnTime = rnd.Next(0, 10000);
        startingPos = transform.position;
        helicopter = FindObjectsByType<Helicopter>(FindObjectsSortMode.None)[0];
        if (FindObjectsByType<Fire_Spread>(FindObjectsSortMode.None).Length != 0)
        {
            usingFireSpread = true;
            Fire_Spread = FindObjectsByType<Fire_Spread>(FindObjectsSortMode.None)[0];
        }
        else if(FindObjectsByType<MapGenerator>(FindObjectsSortMode.None).Length != 0)
        {
            MapGenerator = FindObjectsByType<MapGenerator>(FindObjectsSortMode.None)[0];
            usingFireSpread = false;
        }
        else
        {
            Debug.LogError("No MapGen or Fire Spread scripts found, please add them!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        rb.angularVelocity = Vector3.zero;
        rb.linearVelocity = Vector3.zero;
        if (usingFireSpread)
        {
            if (!alreadyOnHeli)
            {
                for (int i = 0; i < Fire_Spread.fireAreas.Count; i++)
                {
                    if (RectContains(transform.position, Fire_Spread.fireAreas[i]))
                    {
                        Debug.Log("A citizen died in a fire");
                        helicopter.citizensDiedInFire++;
                        Fire_Spread.towns[townIndex].townCitizenCount--;
                        Fire_Spread.towns[townIndex].townDeadCitizenCount++;
                        Destroy(this.gameObject);
                    }
                }
                if (helicopter.touching && Fire_Spread.towns[townIndex].townPickupPoint.Contains(new Vector2(helicopter.physicsHeli.transform.position.x, helicopter.physicsHeli.transform.position.z)) && helicopter.capacity < helicopter.maxCapacity)
                {


                    GameObject closestEntrance = helicopter.entrances[0];
                    for (int i = 1; i < helicopter.entrances.Count; i++)
                    {
                        if (Vector3.Distance(helicopter.entrances[i].transform.position, transform.position) < Vector3.Distance(closestEntrance.transform.position, transform.position))
                        {
                            closestEntrance = helicopter.entrances[i].gameObject;
                        }
                    }
                    agent.SetDestination(closestEntrance.transform.position);

                }
                else
                {
                    agent.SetDestination(startingPos);
                }

            }
            else
            {
                agent.SetDestination(manuallyAssignedTarget);
            }
            if (touchingDoor && helicopter.touching && helicopter.capacity < helicopter.maxCapacity && !alreadyOnHeli)
            {
                helicopter.capacity++;
                Fire_Spread.towns[townIndex].townCitizenCount--;
                Fire_Spread.towns[townIndex].townPickedUpCitizenCount++;
                Destroy(this.gameObject);
            }
            else if (touchingHeli && helicopter.heliCollider.touchingObj == this.gameObject)
            {
                Debug.Log("You killed a citizen!");
                helicopter.citizensKilled++;
                Fire_Spread.towns[townIndex].townCitizenCount--;
                Fire_Spread.towns[townIndex].townDeadCitizenCount++;
                Destroy(this.gameObject);
            }
        }
        else
        {
            if (!alreadyOnHeli)
            {
                for (int i = 0; i < MapGenerator.fireAreas.Count; i++)
                {
                    if (RectContains(new Vector2(transform.position.x, transform.position.z), MapGenerator.fireAreas[i]) && maxBurnTime > burnTime)
                    {
                        burnTime++;
                    }
                    else if (burnTime > maxBurnTime)
                    {
                        Debug.Log("A citizen died in a fire");
                        helicopter.citizensDiedInFire++;
                        MapGenerator.towns[townIndex].townCitizenCount--;
                        MapGenerator.towns[townIndex].townDeadCitizenCount++;
                        Destroy(this.gameObject);
                    }
                }
                if (helicopter.touching && MapGenerator.towns[townIndex].townPickupPoint.Contains(new Vector2(helicopter.physicsHeli.transform.position.x, helicopter.physicsHeli.transform.position.z)) && helicopter.capacity < helicopter.maxCapacity)
                {


                    GameObject closestEntrance = helicopter.entrances[0];
                    for (int i = 1; i < helicopter.entrances.Count; i++)
                    {
                        if (Vector3.Distance(helicopter.entrances[i].transform.position, transform.position) < Vector3.Distance(closestEntrance.transform.position, transform.position))
                        {
                            closestEntrance = helicopter.entrances[i].gameObject;
                        }
                    }
                    agent.SetDestination(closestEntrance.transform.position);
                    animator.speed = 3;
                }
                else
                {
                    agent.SetDestination(startingPos);
                    animator.speed = 3;
                }

            }
            else
            {
                agent.SetDestination(manuallyAssignedTarget);
            }
            if (touchingDoor && helicopter.touching && helicopter.capacity < helicopter.maxCapacity && !alreadyOnHeli)
            {
                helicopter.capacity++;
                MapGenerator.towns[townIndex].townCitizenCount--;
                helicopter.citizens++;
                MapGenerator.towns[townIndex].townPickedUpCitizenCount++;
                Destroy(this.gameObject);
            }
            else if (touchingHeli && helicopter.heliCollider.touchingObj == this.gameObject)
            {
                Debug.Log("You killed a citizen!");
                helicopter.citizensKilled++;
                MapGenerator.towns[townIndex].townCitizenCount--;
                MapGenerator.towns[townIndex].townDeadCitizenCount++;
                Destroy(this.gameObject);
            }
        }
        if (touchingHospitalDoor)
        {
            helicopter.citizensRescued++;
            Destroy(this.gameObject);
        }
        if(Vector3.Distance(startingPos, this.transform.position) < 0.05)
        {
            animator.speed = 0;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Door")
        {
            touchingDoor = true;
        }
        if (other.gameObject.tag == "Hospital Door")
        {
            Debug.Log("touching door");
            touchingHospitalDoor = true;
        }
    }

    public void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.name == "HelicopterMain")
        {
            touchingHeli = true;
        }

    }
    public bool RectContains(Vector3 Input, Rect Rect)
    {
        float halfWidth = Rect.width / 2f;
        float halfHeight = Rect.height / 2f;

        if ((Input.x < Rect.x + halfWidth)
            && (Input.x > Rect.x - halfWidth)
            && (Input.z < Rect.y + halfHeight)
            && (Input.z > Rect.y - halfHeight))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject.tag == "Helicopter")
        {
            touchingHeli = false;

        }
    }
    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "Door")
        {
            touchingDoor = false;
        }
    }

}
