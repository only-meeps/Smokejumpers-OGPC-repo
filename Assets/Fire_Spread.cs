using NUnit.Framework;
using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
public class Town
{
    public Rect town;
    public List<GameObject> houses;
    public Rect townPickupPoint;
    public int townCitizenCount;
    public float townHeight;
    public float townNoiseHeight;
    public float townPickupPointHeight;
    public float townPickupPointNoiseHeight;
    public Marker townMarker;
}

public class Fire_Spread : MonoBehaviour
{
    public GameObject treePrefab;
    public System.Random rnd = new System.Random();
    public Rect MapSize;
    public GameObject map;
    public List<GameObject> trees = new List<GameObject>();
    public List<GameObject> burningTrees = new List<GameObject>();
    public List<GameObject> burntTrees = new List<GameObject>();
    public List<GameObject> fires = new List<GameObject>();
    public List<Town> towns = new List<Town>();
    public List<GameObject> buildingPrefabs = new List<GameObject>();
    public List<Rect> fireAreas = new List<Rect>();
    public GameObject firePrefab;
    public GameObject citizenPrefab;
    public NavMeshSurface navMeshSurface;
    public GameObject heliPadPrefab;
    public GameObject helicopterPrefab;
    public UIController UIController;
    public Sprite townIcon;
    public Sprite helipadIcon;
    public Material townPickupZoneMat;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        map.transform.localScale = new Vector3(MapSize.width / 10, 1, MapSize.height / 10);
        map.transform.position = new Vector3(0, 0, 0);
        GameObject helipad = Instantiate(heliPadPrefab, new Vector3(Random.Range(-MapSize.width / 2, MapSize.width / 2), -1, Random.Range(-MapSize.height / 2, MapSize.height / 2)), Quaternion.identity);
        Marker helipadCompassMarker = helipad.AddComponent<Marker>();
        helipadCompassMarker.icon = helipadIcon;
        UIController.AddMarker(helipadCompassMarker);
        Instantiate(helicopterPrefab, new Vector3(helipad.transform.position.x, .85f, helipad.transform.position.z), Quaternion.identity);
        
        int reps = rnd.Next(1, 5);
        for(int i = 0; i < reps; i++)
        {
            Town town = new Town();
            Rect townArea = new Rect();
            float townWidth;
            float townLength;
            float townX;
            float townZ;
            townX = Random.Range(-MapSize.width / 2, MapSize.width / 2);
            townZ = Random.Range(-MapSize.height / 2, MapSize.width / 2);
            
             
            while (Vector2.Distance(new Vector2(townX, townZ), new Vector2(helipad.transform.position.x, helipad.transform.position.z)) < 20)
            {
                townX = Random.Range(-MapSize.width / 2, MapSize.width / 2);
                townZ = Random.Range(-MapSize.height / 2, MapSize.width / 2);
            }
            
            townWidth = Random.Range(6, 20);
            townLength = Random.Range(6, 20);

            while (!RectContainsRect(new Rect(townX, townZ, townWidth + 20, townLength + 20), MapSize))
            {
                townX = Random.Range(-MapSize.width / 2, MapSize.width / 2);
                townZ = Random.Range(-MapSize.height / 2, MapSize.width / 2);
                townWidth = Random.Range(15, 20);
                townLength = Random.Range(15, 20);
            }
            
            //Debug.Log(new Rect(townX, townZ, townWidth, townLength));
            townArea = new Rect(new Vector2(townX, townZ), new Vector2(townWidth, townLength));
            town.town = townArea;
            town.townPickupPoint.x = Random.Range(town.town.x - 15, town.town.width + town.town.x + 15);
            town.townPickupPoint.y = Random.Range(town.town.y - 15, town.town.width + town.town.y + 15);
            town.townPickupPoint.width = 7;
            town.townPickupPoint.height = 7;
            town.townCitizenCount = rnd.Next(5, 10);
            DrawRect(town.townPickupPoint, 1, townPickupZoneMat, .1f);
            GameObject townOBJ = new GameObject();
            townOBJ.name = "Town " + i;
            townOBJ.transform.position = new Vector3(townX, 0, townZ);
            Marker townMarker = townOBJ.AddComponent<Marker>();
            townMarker.icon = townIcon;
            UIController.AddMarker(townMarker);
            town.townMarker = townMarker;
            towns.Add(town);
        }
        for (int i = 0; i < rnd.Next(500, 1000); i++)
        {
            float treeX;
            float treeZ;
            
            treeX = Random.Range(-MapSize.width / 2, MapSize.width / 2);
            treeZ = Random.Range(-MapSize.height / 2, MapSize.height / 2);

            for(int j = 0; j < towns.Count; j++)
            {
                if (!towns[j].town.Contains(new Vector2(treeX, treeZ)) && !towns[j].townPickupPoint.Contains(new Vector2(treeX,treeZ)))
                {
                    trees.Add(Instantiate(treePrefab, new Vector3(treeX, 0, treeZ), Quaternion.identity));
                }
            }

        }
        for (int i = 0; i < rnd.Next(1, 5); i++)
        {
            fireAreas.Add(new Rect(new Vector2(trees[rnd.Next(0, trees.Count)].transform.position.x, trees[rnd.Next(0, trees.Count)].transform.position.z), new Vector2(0, 0)));
        }
        for (int i = 0; i < towns.Count; i++)
        {
            int houses = rnd.Next(3, 8);
            List<GameObject> townHouses = new List<GameObject>();
            for (int j = 0; j < houses; j++)
            {
                float houseX = 0;
                float houseZ = 0;
                houseX = Random.Range(towns[i].town.x, towns[i].town.width + towns[i].town.x);
                houseZ = Random.Range(towns[i].town.y, towns[i].town.height + towns[i].town.y);
                townHouses.Add(Instantiate(buildingPrefabs[rnd.Next(0, buildingPrefabs.Count)], new Vector3(houseX, towns[i].townHeight, houseZ), Quaternion.identity));
                trees.Add(townHouses[j]);
            }
            
            towns[i].houses = townHouses;
            for(int j = 0; j < towns[i].townCitizenCount; j++)
            {
                GameObject citizen = Instantiate(citizenPrefab, new Vector3(Random.Range(towns[i].townPickupPoint.x - 10, towns[i].townPickupPoint.x + towns[i].townPickupPoint.width + 10), citizenPrefab.transform.lossyScale.y + 1, Random.Range(towns[i].townPickupPoint.y - 10, towns[i].townPickupPoint.y + towns[i].townPickupPoint.height + 10)), Quaternion.identity);
                citizen.GetComponent<Citizen>().townIndex = i;
            }
        }

    }
    public void DebugDrawRect(Rect rect, float height, Color color)
    {
        Debug.DrawLine(new Vector3(rect.x, height, rect.y), new Vector3(rect.width + rect.x, height, rect.y), color);
        Debug.DrawLine(new Vector3(rect.width + rect.x, height, rect.y), new Vector3(rect.width + rect.x, height, rect.height + rect.y), color);
        Debug.DrawLine(new Vector3(rect.width + rect.x, height, rect.height + rect.y), new Vector3(rect.x, height, rect.height + rect.y), color);
        Debug.DrawLine(new Vector3(rect.x, height, rect.height + rect.y), new Vector3(rect.x, height, rect.y), color);
    } 

    public void DrawRect(Rect rect, float height, Material mat, float lineWidth)
    {
        GameObject rectOBJ = new GameObject();
        LineRenderer lineRend = rectOBJ.AddComponent<LineRenderer>();

        lineRend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRend.material = mat;
        lineRend.startWidth = lineWidth;
        lineRend.endWidth = lineWidth;
        lineRend.positionCount = 5;
        lineRend.SetPosition(0, new Vector3(rect.x, height, rect.y));
        lineRend.SetPosition(1, new Vector3(rect.x + rect.width, height, rect.y));
        lineRend.SetPosition(2, new Vector3(rect.x + rect.width, height, rect.y + rect.height));
        lineRend.SetPosition(3, new Vector3(rect.x, height, rect.y + rect.height));
        lineRend.SetPosition(4, new Vector3(rect.x, height, rect.y));
    }
    public void DebugDrawRectCentered(Rect rect, float height, Color color)
    {
        Debug.DrawLine(new Vector3(-(rect.width / 2) + rect.x, height, -(rect.height / 2) + rect.y), new Vector3((rect.width / 2) + rect.x, height, -(rect.height / 2) + rect.y), color);
        Debug.DrawLine(new Vector3((rect.width / 2) + rect.x, height, -(rect.height / 2) + rect.y), new Vector3((rect.width / 2) + rect.x, height, (rect.height / 2) + rect.y), color);
        Debug.DrawLine(new Vector3((rect.width / 2) + rect.x, height, (rect.height / 2) + rect.y), new Vector3(-(rect.width / 2) + rect.x, height, (rect.height / 2) + rect.y), color);
        Debug.DrawLine(new Vector3(-(rect.width / 2) + rect.x, height, (rect.height / 2) + rect.y), new Vector3(-(rect.width / 2) + rect.x, height, -(rect.height / 2) +rect.y), color);
    }
    // Update is called once per frame
    void Update()
    {

        for(int i = 0; i < fireAreas.Count; i++)
        {
            fireAreas[i] = new Rect(fireAreas[i].position, new Vector2(fireAreas[i].width + 0.01f, fireAreas[i].height + 0.01f));
            DebugDrawRectCentered(fireAreas[i], 1, Color.red);
        }
        for (int i = 0; i < towns.Count; i++)
        {
            DebugDrawRect(towns[i].town, 1, Color.green);

            if (towns[i].townCitizenCount == 0)
            {
                UIController.RemoveMarker(towns[i].townMarker);
            }
            for (int j = 0; j < towns[i].houses.Count; j++)
            {
                DebugDrawRect(new Rect(new Vector2(towns[i].houses[j].transform.position.x, towns[i].houses[j].transform.position.z), new Vector2(1,1)), 1, Color.yellow);
            }
        }
        for(int i = 0; i < trees.Count; i++)
        {
            for(int f = 0; f < fireAreas.Count; f++)
            {
                if (RectContains(trees[i].transform.position, fireAreas[f]))
                {
                    if (!burningTrees.Contains(trees[i]) && rnd.Next(0,500) == 2)
                    {
                        burningTrees.Add(trees[i]);
                        GameObject fire = Instantiate(firePrefab, new Vector3(trees[i].transform.position.x, trees[i].transform.position.y + transform.lossyScale.y, trees[i].transform.position.z), Quaternion.identity);
                        fire.transform.localScale = new Vector3(2, 2, 2);
                        fires.Add(fire);
                    }
                    if (trees[i].GetComponent<Tree>().fireCycleLoop > rnd.Next(1000, 2000))
                    {
                        for (int j = 0; j < burningTrees.Count; j++)
                        {
                            if (trees[i] == burningTrees[j] && !burntTrees.Contains(trees[i]))
                            {
                                burntTrees.Add(burningTrees[j]);
                                Destroy(fires[j].gameObject);
                                for (int t = 0; t < trees[i].GetComponent<Tree>().burnableParts.Count; t++)
                                {
                                    Material mat = trees[i].GetComponent<Tree>().burnableParts[t].GetComponent<Renderer>().sharedMaterial;
                                    mat = new Material(mat);
                                    mat.color = Color.black;
                                    trees[i].GetComponent<Tree>().burnableParts[t].GetComponent<Renderer>().sharedMaterial = mat;
                                }
                                break;
                            }
                        }
                    }
                }
            }

        }
        for (int i = 0; i < burningTrees.Count; i++)
        {
            burningTrees[i].GetComponent<Tree>().fireCycleLoop++;
        }
        navMeshSurface.UpdateNavMesh(navMeshSurface.navMeshData);
    }
    public bool RectContains(Vector3 Input, Rect Rect)
    {
        if     ((Input.x < (Rect.width / 2) + Rect.x)
            && (Input.x > -(Rect.width / 2) + Rect.x)
            && (Input.z < (Rect.height / 2) + Rect.y)
            && (Input.z > -(Rect.height / 2) + Rect.y))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool RectContainsRect(Rect Input, Rect Rect)
    {
        if ((-Input.width / 2) + Input.x >= (-Rect.width / 2) + Rect.x && (Input.height / 2) + Input.y <= (Rect.width / 2) + Rect.y)
        {
            return true;
        }
        else if ((-Input.width / 2) + Input.x >= (-Rect.width / 2) + Rect.x && (-Input.height / 2) + Input.y >= (-Rect.width / 2) + Rect.y)
        {
            return true;
        }
        else if ((Input.width / 2) + Input.x <= (Rect.width / 2) + Rect.x && (-Input.height / 2) + Input.y >= (-Rect.width / 2) + Rect.y)
        {
            return true;
        }
        else if ((Input.width / 2) + Input.x <= (Rect.width / 2) + Rect.x && (Input.height / 2) + Input.y <= (Rect.width / 2) + Rect.y)
        {
            return true;
        }
        else return false;
    }
}
