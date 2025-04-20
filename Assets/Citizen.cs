using Mono.Cecil.Cil;
using UnityEngine;
using UnityEngine.AI;

public class Citizen : MonoBehaviour
{
    public NavMeshAgent agent;
    public Helicopter helicopter;
    public Fire_Spread Fire_Spread;
    public int townIndex;
    public Vector3 startingPos;
    public bool touchingDoor;
    public bool touchingHeli;
    public bool touchingHospitalDoor;
    public bool alreadyOnHeli;
    public Vector3 manuallyAssignedTarget;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        startingPos = transform.position;
        helicopter = FindObjectsByType<Helicopter>(FindObjectsSortMode.None)[0];
        Fire_Spread = FindObjectsByType<Fire_Spread>(FindObjectsSortMode.None)[0];
    }

    // Update is called once per frame
    void Update()
    {
        if (!alreadyOnHeli)
        {
            for (int i = 0; i < Fire_Spread.fireAreas.Count; i++)
            {
                if (Fire_Spread.RectContains(transform.position, Fire_Spread.fireAreas[i]))
                {
                    Debug.Log("A citizen died in a fire");
                    helicopter.citizensDiedInFire++;
                    Fire_Spread.towns[townIndex].townCitizenCount--;
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
            Destroy(this.gameObject);
        }
        if (touchingHospitalDoor)
        {
            helicopter.citizensRescued++;
            Destroy(this.gameObject);
        }
        else if(touchingHeli && !helicopter.touching)
        {
            Debug.Log("You killed a citizen!");
            helicopter.citizensKilled++;
            Fire_Spread.towns[townIndex].townCitizenCount--;
            Destroy(this.gameObject);
        }

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Door")
        {
            touchingDoor = true;
        }
        if(other.gameObject.tag == "Hospital Door")
        {
            touchingHospitalDoor = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.name == "HelicopterMain")
        {
            touchingHeli = true;
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        touchingHeli = false;
    }
    private void OnTriggerExit(Collider other)
    {
        touchingDoor = false;
        touchingHospitalDoor = false;
    }

}
