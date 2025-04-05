using System.Collections;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor.AssetImporters;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.XR;
using static UnityEditor.U2D.ScriptablePacker;
using Unity.AI.Navigation;

public class MapGenerator : MonoBehaviour
{
    public GameObject treePrefab;
    public System.Random rnd = new System.Random();
    public int mapSize;
    public int chunkSize;
    public List<GameObject> trees = new List<GameObject>();
    public List<GameObject> burningTrees = new List<GameObject>();
    public List<GameObject> burntTrees = new List<GameObject>();
    public List<GameObject> fires = new List<GameObject>();
    public List<Town> towns = new List<Town>();
    public List<GameObject> buildingPrefabs = new List<GameObject>();
    public List<Rect> fireAreas = new List<Rect>();
    public GameObject[,] chunks;
    public GameObject firePrefab;
    public GameObject citizenPrefab;
    public NavMeshSurface[,] navMeshSurface;
    public GameObject heliPadPrefab;
    public GameObject helicopterPrefab;
    public UIController UIController;
    public Sprite townIcon;
    public Sprite helipadIcon;
    public Material townPickupZoneMat;
    public int noiseScale;
    public int octaves;
    public float persistance;
    public float lacunarity;
    public MapDisplay display;
    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;
    public Material terrainMat;
    public TextureData textureData;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        navMeshSurface = new NavMeshSurface[mapSize, mapSize];

        //Create chunk gameobjects and gameobject component lists
        chunks = new GameObject[mapSize, mapSize];
        display.textureRender = new Renderer[mapSize, mapSize];
        display.meshFilter = new MeshFilter[mapSize, mapSize];
        display.MeshRenderer = new MeshRenderer[mapSize, mapSize];
        display.meshCollider = new MeshCollider[mapSize, mapSize];

        //Spawn helicopter & helipad
        GameObject helipad = Instantiate(heliPadPrefab, new Vector3(UnityEngine.Random.Range(0, mapSize * chunkSize), -1, UnityEngine.Random.Range(0, mapSize * chunkSize)), Quaternion.identity);
        Instantiate(helicopterPrefab, new Vector3(helipad.transform.position.x, .85f, helipad.transform.position.z), Quaternion.identity);

        //Assign compass markers to the helipad
        Marker helipadCompassMarker = helipad.AddComponent<Marker>();
        helipadCompassMarker.icon = helipadIcon;
        UIController.AddMarker(helipadCompassMarker);

        //Pick number of towns
        int townCount = rnd.Next(1, 5);

        //Create towns
        for (int i = 0; i < townCount; i++)
        {
            //Create variables
            Town town = new Town();
            Rect townArea = new Rect();
            float townWidth;
            float townLength;
            float townX;
            float townZ;

            //Assign position
            townX = UnityEngine.Random.Range(0, mapSize * chunkSize);
            townZ = UnityEngine.Random.Range(0, -mapSize * chunkSize);

            //Check distance to helipad
            while (Vector2.Distance(new Vector2(townX, townZ), new Vector2(helipad.transform.position.x, helipad.transform.position.z)) < 2)
            {
                townX = UnityEngine.Random.Range(0, mapSize * chunkSize);
                townZ = UnityEngine.Random.Range(0, -mapSize * chunkSize);
            }

            //Assign width
            townWidth = UnityEngine.Random.Range(6, 20);
            townLength = UnityEngine.Random.Range(6, 20);

            //Make sure it is inside the map
            while (!RectContainsRect(new Rect(townX, townZ, townWidth, townLength), new Rect(new Vector2(0, 0), new Vector2(chunkSize * mapSize, -chunkSize * mapSize))))
            {
                townX = UnityEngine.Random.Range(0, mapSize * chunkSize);
                townZ = UnityEngine.Random.Range(0, -mapSize * chunkSize);
                townWidth = UnityEngine.Random.Range(15, 20);
                townLength = UnityEngine.Random.Range(15, 20);
            }

            //Assign town position and size
            townArea = new Rect(new Vector2(townX, townZ), new Vector2(townWidth, townLength));
            town.town = townArea;

            //Create town pickup zone
            town.townPickupPoint.x = UnityEngine.Random.Range(town.town.x - 15, town.town.width + town.town.x + 15);
            town.townPickupPoint.y = UnityEngine.Random.Range(town.town.y - 15, town.town.width + town.town.y + 15);
            town.townPickupPoint.width = 7;
            town.townPickupPoint.height = 7;

            //Pick number of citizens
            town.townCitizenCount = rnd.Next(5, 10);

            //Draw town pickup zone
            DrawRect(town.townPickupPoint, 1, townPickupZoneMat, .1f);

            //Create town compass marker
            GameObject townOBJ = new GameObject();
            townOBJ.name = "Town " + i;
            townOBJ.transform.position = new Vector3(townX, 0, townZ);
            Marker townMarker = townOBJ.AddComponent<Marker>();
            townMarker.icon = townIcon;
            UIController.AddMarker(townMarker);
            town.townMarker = townMarker;

            //Add town to list of towns
            towns.Add(town);
        }

        //Create a global noise map for all chunks, making sure to generate an extra bit for the missing bits of chunks (generate an extra 2 chunks)
        float[,] globalNoiseMap = NoiseGeneration.GenerateNoiseMap((mapSize + 2) * chunkSize, (mapSize + 2) * chunkSize, rnd.Next(0, 100), noiseScale, octaves, persistance, lacunarity, new Vector2(0, 0), NoiseGeneration.NormalizeMode.Global);

        //Flatten terrain around structures such as towns
        for (int y = 0; y < globalNoiseMap.GetLength(1); y++)
        {
            for (int x = 0; x < globalNoiseMap.GetLength(0); x++)
            {
                for (int t = 0; t < towns.Count; t++)
                {
                    if (towns[t].town.Contains(new Vector2(x, y)))
                    {
                        //If the townheight has not been set yet, then set the townheight to the terrain height, else set the terrain height to the townheight
                        if (towns[t].townHeight == 0)
                        {
                            towns[t].townHeight = globalNoiseMap[x, y] * meshHeightMultiplier;
                            towns[t].townNoiseHeight = globalNoiseMap[x, y];
                        }
                        else
                        {
                            globalNoiseMap[x, y] = towns[t].townNoiseHeight;
                        }
                    }
                }
            }
        }

        //Spawn chunks and assign verticies
        for (int chunkY = 0; chunkY < mapSize; chunkY++)
        {
            for (int chunkX = 0; chunkX < mapSize; chunkX++)
            {
                //Create the chunk gameobject, place it, and name it
                chunks[chunkX, chunkY] = GameObject.CreatePrimitive(PrimitiveType.Plane);
                chunks[chunkX, chunkY].transform.position = new Vector3(chunkX * (chunkSize + 1), 0, -(chunkY * (chunkSize + 1)));
                chunks[chunkX, chunkY].gameObject.name = "Chunk " + chunkX + " , " + chunkY;

                //Create a localnoisemap making sure to get the extra 1 tile to account for 10 verticies and 9 tiles
                float[,] localNoiseMap = new float[chunkSize + 1, chunkSize + 1];

                //Retrieve chunk from globalnoisemap and set it to localnoisemap (less than or equal to making sure to have the extra 1 tile)
                for (int y = 0; y <= chunkSize; y++)
                {
                    for (int x = 0; x <= chunkSize; x++)
                    {
                        localNoiseMap[x, y] = globalNoiseMap[x + (chunkSize * chunkX), y + (chunkSize * chunkY)];
                    }
                }
                MapData mapData = GenerateMapData(localNoiseMap);

                //Add components
                if (chunks[chunkX, chunkY].GetComponent<Renderer>() == null)
                {
                    chunks[chunkX, chunkY].AddComponent<Renderer>();
                }
                display.textureRender[chunkX, chunkY] = chunks[chunkX, chunkY].GetComponent<Renderer>();

                if (chunks[chunkX, chunkY].GetComponent<MeshFilter>() == null)
                {
                    chunks[chunkX, chunkY].AddComponent<MeshFilter>();
                }
                display.meshFilter[chunkX, chunkY] = chunks[chunkX, chunkY].GetComponent<MeshFilter>();
                if (chunks[chunkX, chunkY].GetComponent<MeshRenderer>() == null)
                {
                    chunks[chunkX, chunkY].AddComponent<MeshRenderer>();
                }
                display.MeshRenderer[chunkX, chunkY] = chunks[chunkX, chunkY].GetComponent<MeshRenderer>();

                if (chunks[chunkX, chunkY].GetComponent<MeshCollider>() == null)
                {
                    chunks[chunkX, chunkY].AddComponent<MeshCollider>();
                }

                display.meshCollider[chunkX, chunkY] = chunks[chunkX, chunkY].GetComponent<MeshCollider>();

                display.MeshRenderer[chunkX, chunkY].sharedMaterial = terrainMat;

                //Finially draw the mesh
                display.DrawMesh(MeshGen.GenerateTerrainMesh(localNoiseMap, meshHeightMultiplier, meshHeightCurve, 0, true, chunkX, chunkY, chunkSize, chunkSize), chunkX, chunkY);
            }
        }

        //Create trees
        //Eventually should be based off of noisemap
        for (int i = 0; i < rnd.Next(500, 1000); i++)
        {
            //Create variables
            float treeX;
            float treeZ;

            //Assign position
            treeX = UnityEngine.Random.Range(0, chunkSize * mapSize);
            treeZ = UnityEngine.Random.Range(0, chunkSize * mapSize);

            //Check if inside town and spawn tree
            for (int j = 0; j < towns.Count; j++)
            {
                if (!towns[j].town.Contains(new Vector2(treeX, treeZ)) && !towns[j].townPickupPoint.Contains(new Vector2(treeX, treeZ)))
                {
                    trees.Add(Instantiate(treePrefab, new Vector3(treeX, 0, treeZ), Quaternion.identity));
                }
            }

        }

        //Create fires
        for (int i = 0; i < rnd.Next(1, 5); i++)
        {
            fireAreas.Add(new Rect(new Vector2(trees[rnd.Next(0, trees.Count)].transform.position.x, trees[rnd.Next(0, trees.Count)].transform.position.z), new Vector2(0, 0)));
        }

        //Spawn town houses
        for (int i = 0; i < towns.Count; i++)
        {
            //Pick house count for town
            int houses = rnd.Next(3, 8);

            List<GameObject> townHouses = new List<GameObject>();

            //Spawn houses
            for (int j = 0; j < houses; j++)
            {
                //Assign variables
                float houseX = 0;
                float houseZ = 0;

                //Assign position
                houseX = UnityEngine.Random.Range(towns[i].town.x, towns[i].town.width + towns[i].town.x);
                houseZ = UnityEngine.Random.Range(towns[i].town.y, towns[i].town.height + towns[i].town.y);

                //Add house to town list
                townHouses.Add(Instantiate(buildingPrefabs[rnd.Next(0, buildingPrefabs.Count)], new Vector3(houseX, towns[i].townHeight, houseZ), Quaternion.identity));

                //Add house to burnable objects
                trees.Add(townHouses[j]);
            }

            towns[i].houses = townHouses;

            //Spawn citizens
            for (int j = 0; j < towns[i].townCitizenCount; j++)
            {
                GameObject citizen = Instantiate(citizenPrefab, new Vector3(UnityEngine.Random.Range(towns[i].townPickupPoint.x - 10, towns[i].townPickupPoint.x + towns[i].townPickupPoint.width + 10), citizenPrefab.transform.lossyScale.y + 1, UnityEngine.Random.Range(towns[i].townPickupPoint.y - 10, towns[i].townPickupPoint.y + towns[i].townPickupPoint.height + 10)), Quaternion.identity);
                //Set citizen town index
                
                citizen.GetComponent<Citizen>().townIndex = i;
            }
        }

    }
    //Debug method to draw a rect CORNER ALIGNED
    public void DebugDrawRect(Rect rect, float height, Color color)
    {
        Debug.DrawLine(new Vector3(rect.x, height, rect.y), new Vector3(rect.width + rect.x, height, rect.y), color);
        Debug.DrawLine(new Vector3(rect.width + rect.x, height, rect.y), new Vector3(rect.width + rect.x, height, rect.height + rect.y), color);
        Debug.DrawLine(new Vector3(rect.width + rect.x, height, rect.height + rect.y), new Vector3(rect.x, height, rect.height + rect.y), color);
        Debug.DrawLine(new Vector3(rect.x, height, rect.height + rect.y), new Vector3(rect.x, height, rect.y), color);
    }

    //Method to draw a rect CORNER ALIGNED
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

    //Apply mat to terrain
    MapData GenerateMapData(float[,] noiseMap)
    {
        textureData.ApplyToMaterial(terrainMat);
        return new MapData(noiseMap);
    }

    //Debug method to draw a rect CENTER ALIGNED
    public void DebugDrawRectCentered(Rect rect, float height, Color color)
    {
        Debug.DrawLine(new Vector3(-(rect.width / 2) + rect.x, height, -(rect.height / 2) + rect.y), new Vector3((rect.width / 2) + rect.x, height, -(rect.height / 2) + rect.y), color);
        Debug.DrawLine(new Vector3((rect.width / 2) + rect.x, height, -(rect.height / 2) + rect.y), new Vector3((rect.width / 2) + rect.x, height, (rect.height / 2) + rect.y), color);
        Debug.DrawLine(new Vector3((rect.width / 2) + rect.x, height, (rect.height / 2) + rect.y), new Vector3(-(rect.width / 2) + rect.x, height, (rect.height / 2) + rect.y), color);
        Debug.DrawLine(new Vector3(-(rect.width / 2) + rect.x, height, (rect.height / 2) + rect.y), new Vector3(-(rect.width / 2) + rect.x, height, -(rect.height / 2) + rect.y), color);
    }

    void Update()
    {
        //Debug Draw the fire areas and update their positions
        for (int i = 0; i < fireAreas.Count; i++)
        {
            fireAreas[i] = new Rect(fireAreas[i].position, new Vector2(fireAreas[i].width + 0.01f, fireAreas[i].height + 0.01f));
            DebugDrawRectCentered(fireAreas[i], 1, Color.red);
        }

        //Debug draw the towns and update the compass markers
        for (int i = 0; i < towns.Count; i++)
        {
            DebugDrawRect(towns[i].town, 1, Color.green);

            if (towns[i].townCitizenCount == 0)
            {
                UIController.RemoveMarker(towns[i].townMarker);
            }
            for (int j = 0; j < towns[i].houses.Count; j++)
            {
                DebugDrawRect(new Rect(new Vector2(towns[i].houses[j].transform.position.x, towns[i].houses[j].transform.position.z), new Vector2(1, 1)), 1, Color.yellow);
            }
        }

        //Check trees
        for (int i = 0; i < trees.Count; i++)
        {
            for (int f = 0; f < fireAreas.Count; f++)
            {
                //Check if tree is inside of fire
                if (RectContains(trees[i].transform.position, fireAreas[f]))
                {
                    //Check if tree is included in the burning trees list and, if not, then start burning it
                    if (!burningTrees.Contains(trees[i]) && rnd.Next(0, 500) == 2)
                    {
                        burningTrees.Add(trees[i]);

                        //Add fire effect
                        GameObject fire = Instantiate(firePrefab, new Vector3(trees[i].transform.position.x, trees[i].transform.position.y + transform.lossyScale.y, trees[i].transform.position.z), Quaternion.identity);
                        fire.transform.localScale = new Vector3(2, 2, 2);
                        fires.Add(fire);
                    }

                    //Check if tree is burned out
                    if (trees[i].GetComponent<Tree>().fireCycleLoop > rnd.Next(1000, 2000))
                    {
                        for (int j = 0; j < burningTrees.Count; j++)
                        {
                            if (trees[i] == burningTrees[j] && !burntTrees.Contains(trees[i]))
                            {
                                burntTrees.Add(burningTrees[j]);

                                //Remove the fire
                                Destroy(fires[j].gameObject);

                                //Turn all parts under the burnable parts list black
                                for (int t = 0; t < trees[i].GetComponent<Tree>().burnableParts.Count; t++)
                                {
                                    //Create a new material
                                    Material mat = trees[i].GetComponent<Tree>().burnableParts[t].GetComponent<Renderer>().sharedMaterial;

                                    //Change color of material
                                    mat = new Material(mat);
                                    mat.color = Color.black;

                                    //Apply material
                                    trees[i].GetComponent<Tree>().burnableParts[t].GetComponent<Renderer>().sharedMaterial = mat;
                                }
                                break;
                            }
                        }
                    }
                }
            }

        }

        //Update burn time
        for (int i = 0; i < burningTrees.Count; i++)
        {
            burningTrees[i].GetComponent<Tree>().fireCycleLoop++;
        }
    }
    //Check if a object is inside a rect DEPRECATED : USE .Contains INSTEAD
    public bool RectContains(Vector3 Input, Rect Rect)
    {
        if ((Input.x < (Rect.width / 2) + Rect.x)
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

    //Check if a rect is inside another rect
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

//Storage class for terrain data
[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
}
public struct MapData
{
    public readonly float[,] heightMap;


    public MapData(float[,] heightMap)
    {
        this.heightMap = heightMap;
    }
}

