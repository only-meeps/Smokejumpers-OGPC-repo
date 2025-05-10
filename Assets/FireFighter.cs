using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class FireFighter : MonoBehaviour
{
    public NavMeshAgent agent;
    public Helicopter helicopter;
    public HeliCollider heliCollider;
    public MapGenerator MapGenerator;
    public int dropOffPointIndex;
    public Vector3 startingPos;
    public bool touchingDoor;
    public bool touchingHeli;
    public bool touchingFireStationDoor;
    public bool alreadyOnHeli;
    public Vector3 finalPos;
    public Rigidbody rb;
    System.Random rnd = new System.Random();
    public Animator animator;
    public GameObject onTriggerObj;
    public int helipadIndex;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startingPos = transform.position;
        helicopter = FindObjectsByType<Helicopter>(FindObjectsSortMode.None)[0];
        heliCollider = FindFirstObjectByType<HeliCollider>();
        MapGenerator = FindObjectsByType<MapGenerator>(FindObjectsSortMode.None)[0];
        if (alreadyOnHeli)
        {
            for (int f = 0; f < GameObject.FindObjectsByType<Mission>(FindObjectsSortMode.None).Length; f++)
            {
                if (FindObjectsByType<Mission>(FindObjectsSortMode.None)[f].missionTag == "FireFighterDropOffPoint")
                {

                    FindObjectsByType<Mission>(FindObjectsSortMode.None)[f].missionObj.SetActive(false);
                    MapGenerator.assaignedMissions.Remove(FindObjectsByType<Mission>(FindObjectsSortMode.None)[f]);
                    break;
                }
            }
            finalPos = new Vector3(Random.Range(MapGenerator.fireFighterDropOffPoints[dropOffPointIndex].x, MapGenerator.fireFighterDropOffPoints[dropOffPointIndex].x + MapGenerator.fireFighterDropOffPoints[dropOffPointIndex].width), MapGenerator.fireFighterDropOffPointsHeight[dropOffPointIndex], Random.Range(MapGenerator.fireFighterDropOffPoints[dropOffPointIndex].y, MapGenerator.fireFighterDropOffPoints[dropOffPointIndex].y + MapGenerator.fireFighterDropOffPoints[dropOffPointIndex].height));
        }
    }

    // Update is called once per frame
    void Update()
    {
        rb.angularVelocity = Vector3.zero;
        rb.linearVelocity = Vector3.zero;
        if (!alreadyOnHeli)
        {
             if (helicopter.touching && heliCollider.touchingObj.tag == "helipad")
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
            agent.SetDestination(finalPos);
        }
        if (touchingDoor && helicopter.touching && helicopter.capacity < helicopter.maxCapacity && !alreadyOnHeli)
        {
            helicopter.capacity++;
            helicopter.fireFighters++;
                
            Destroy(this.gameObject);
        }
        if(!helicopter.touching && touchingFireStationDoor)
        {
            MapGenerator.helipads[helipadIndex].fireFightersDeployed--;
            Destroy(this.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        onTriggerObj = other.gameObject;
        if (other.gameObject.tag == "Door")
        {
            touchingDoor = true;
        }
        if (other.gameObject.tag == "Fire station door")
        {
            touchingFireStationDoor = true;
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
        if (collision.gameObject.tag == "Helicopter")
        {
            touchingHeli = false;

        }

    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Door")
        {
            touchingDoor = false;
        }
        if (other.gameObject.tag == "Fire station door")
        {
            touchingFireStationDoor = false;
        }
    }

}
