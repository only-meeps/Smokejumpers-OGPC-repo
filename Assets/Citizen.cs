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
        if (helicopter.touching && Fire_Spread.towns[townIndex].townPickupPoint.Contains(new Vector2(helicopter.physicsHeli.transform.position.x, helicopter.physicsHeli.transform.position.z)) && helicopter.capacity < helicopter.maxCapacity)
        {
            
            
            GameObject closestEntrance = helicopter.entrances[0];
            for(int i = 1; i < helicopter.entrances.Count; i++)
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
        if (touchingDoor && helicopter.touching && helicopter.capacity < helicopter.maxCapacity)
        {
            helicopter.capacity++;
            helicopter.citizensRescued++;
            Fire_Spread.towns[townIndex].townCitizenCount--;
            Destroy(this.gameObject);
        } 
        else if(touchingHeli && !helicopter.touching)
        {
            Debug.Log("You killed a citizen!");
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
    }

}
