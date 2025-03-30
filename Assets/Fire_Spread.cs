using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
public class Town
{
    public Rect town;
    public List<GameObject> houses;
    public float townHeight;
    public float townNoiseHeight;
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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        map.transform.localScale = new Vector3(MapSize.width / 10, 1, MapSize.height / 10);
        map.transform.position = new Vector3(0, 0, 0);
        for (int i = 0; i < rnd.Next(100, 300); i++)
        {
            float treeX;
            float treeZ;
            treeX = Random.Range(-MapSize.width / 2, MapSize.width / 2);
            treeZ = Random.Range(-MapSize.height / 2, MapSize.height / 2);
            trees.Add(Instantiate(treePrefab, new Vector3(treeX, 0, treeZ), Quaternion.identity));
   
        }
        for (int i = 0; i < rnd.Next(1, 5); i++)
        {
            fireAreas.Add(new Rect(new Vector2(trees[rnd.Next(0, trees.Count)].transform.position.x, trees[rnd.Next(0, trees.Count)].transform.position.z), new Vector2(0, 0)));
        }
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
            townWidth = Random.Range(6, 20);
            townLength = Random.Range(6, 20);
            while (!RectContainsRect(new Rect(townX, townZ, townWidth, townLength) , MapSize))
            {
                townX = Random.Range(-MapSize.width / 2, MapSize.width / 2);
                townZ = Random.Range(-MapSize.height / 2, MapSize.width / 2);
                townWidth = Random.Range(15, 20);
                townLength = Random.Range(15, 20);
            }
            Debug.Log(new Rect(townX, townZ, townWidth, townLength));
            townArea = new Rect(new Vector2(townX, townZ), new Vector2(townWidth, townLength));
            town.town = townArea;
            towns.Add(town);
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
        }

    }
    public void DrawRect(Rect rect, float height, Color color)
    {
        Debug.DrawLine(new Vector3(rect.x, height, rect.y), new Vector3(rect.width + rect.x, height, rect.y), color);
        Debug.DrawLine(new Vector3(rect.width + rect.x, height, rect.y), new Vector3(rect.width + rect.x, height, rect.height + rect.y), color);
        Debug.DrawLine(new Vector3(rect.width + rect.x, height, rect.height + rect.y), new Vector3(rect.x, height, rect.height + rect.y), color);
        Debug.DrawLine(new Vector3(rect.x, height, rect.height + rect.y), new Vector3(rect.x, height, rect.y), color);
    }
    // Update is called once per frame
    void Update()
    { 
        for(int i = 0; i < fireAreas.Count; i++)
        {
            fireAreas[i] = new Rect(fireAreas[i].position, new Vector2(fireAreas[i].width - 0.01f, fireAreas[i].height- 0.01f));
            DrawRect(fireAreas[i], 1, Color.red);
        }
        for (int i = 0; i < towns.Count; i++)
        {
            DrawRect(towns[i].town, 1, Color.green);
            for(int j = 0; j < towns[i].houses.Count; j++)
            {
                DrawRect(new Rect(new Vector2(towns[i].houses[j].transform.position.x, towns[i].houses[j].transform.position.z), new Vector2(1,1)), 1, Color.yellow);
            }
        }
        for(int i = 0; i < trees.Count; i++)
        {
            for(int f = 0; f < fireAreas.Count; f++)
            {
                if (RectContains(trees[i].transform.position, fireAreas[f]))
                {
                    if (!burningTrees.Contains(trees[i]) && rnd.Next(0, 100) == 2)
                    {
                        burningTrees.Add(trees[i]);
                        fires.Add(Instantiate(firePrefab, new Vector3(trees[i].transform.position.x, trees[i].transform.position.y + trees[i].transform.lossyScale.y, trees[i].transform.position.z), Quaternion.identity));
                    }
                    if (trees[i].GetComponent<Tree>().fireCycleLoop > 1200)
                    {
                        for (int j = 0; j < burningTrees.Count; j++)
                        {
                            if (trees[i] == burningTrees[j] && !burntTrees.Contains(trees[i]))
                            {
                                burntTrees.Add(burningTrees[j]);
                                Destroy(fires[j].gameObject);
                                for (int t = 0; t < trees[i].GetComponent<Tree>().burnableParts.Count; t++)
                                {
                                    trees[i].GetComponent<Tree>().burnableParts[t].GetComponent<MeshRenderer>().sharedMaterial.color = Color.black;
                                    
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
    }
    public bool RectContains(Vector3 Input, Rect Rect)
    {
        if((Input.x < Rect.width/2) && (Input.x > -Rect.width/2) && (Input.z < Rect.height/2) && (Input.z > -Rect.height/2))
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
