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
<<<<<<< HEAD
=======
using Unity.AI.Navigation;
>>>>>>> parent of 5d240b9 (Towns and building still do not flatten terrain)

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode { NoiseMap, ColorMap, Mesh };
    public DrawMode drawMode;
    public int chunkWidth;
    public int chunkHeight;
    public float noiseScale;
    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;
    public int seed;
    public Vector2 offset;
    public TerrainType[] regions;
    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;
    public int LOD;
    public Material treeNormal;
    public Material treeFire;
    public Material terrainMat;
    public Rect DangerZone;
    public System.Random rnd = new System.Random();
    public GameObject map;
    public List<GameObject> treePrefabs = new List<GameObject>();
    public List<GameObject> trees = new List<GameObject>();
    public List<GameObject> burningTrees = new List<GameObject>();
    public List<GameObject> burntTrees = new List<GameObject>();
    public List<GameObject> fires = new List<GameObject>();
    public List<Town> towns = new List<Town>();
    public List<GameObject> buildingPrefabs = new List<GameObject>();
    public int totalHouses;
    public GameObject firePrefab;
<<<<<<< HEAD
    public TextureData textureData;
    public NoiseData noiseData;
    public TerrainData terrainData;
    public int chunksX;
    public int chunksY;
    public GameObject[,] chunks;
    public GameObject player;
    public List<Rect> fireAreas = new List<Rect>();
    public Vector2 townPosEditor;
    public Vector2 townPos;
    public float minHeight
    {
        get
        {
            return meshHeightMultiplier * meshHeightCurve.Evaluate(0);
        }
    }
    public float maxHeight
    {
        get
        {
            return meshHeightMultiplier * meshHeightCurve.Evaluate(1);
        }
    }
=======
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

>>>>>>> parent of 5d240b9 (Towns and building still do not flatten terrain)
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
<<<<<<< HEAD
        chunks = new GameObject[chunksX, chunksY];
        terrainMat = new Material(terrainMat);
        DangerZone = new Rect(new Vector2(0, 0), new Vector2(chunkWidth, chunkHeight));
        rnd = new System.Random(noiseData.seed);
        UnityEngine.Random.InitState(noiseData.seed);
        CreateMap();
    }
    private void Start()
    {
=======
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

>>>>>>> parent of 5d240b9 (Towns and building still do not flatten terrain)
    }
    public void DrawRect(Rect rect, float height, Color color)
    {
        Debug.DrawLine(new Vector3(rect.x, height, rect.y), new Vector3(rect.width + rect.x, height, rect.y), color);
        Debug.DrawLine(new Vector3(rect.width + rect.x, height, rect.y), new Vector3(rect.width + rect.x, height, rect.height + rect.y), color);
        Debug.DrawLine(new Vector3(rect.width + rect.x, height, rect.height + rect.y), new Vector3(rect.x, height, rect.height + rect.y), color);
        Debug.DrawLine(new Vector3(rect.x, height, rect.height + rect.y), new Vector3(rect.x, height, rect.y), color);
    }

<<<<<<< HEAD
    // Update is called once per frame
    void Update()
    {
        if (townPosEditor != townPos)
        {
            for (int i = 0; i < GameObject.FindGameObjectsWithTag("Terrain").Length; i++)
            {
                Destroy(GameObject.FindGameObjectsWithTag("Terrain")[i]);
            }
            towns = new List<Town>();
            townPos = townPosEditor;
            CreateMap();
        }


        /*
        for(int chunkX = 0; chunkX < chunksX; chunkX++)
        {
            for (int chunkY = 0; chunkY < chunksX; chunkY++)
            {
                if (chunks[chunkX, chunkY].transform.position.x > player.transform.position.x - (chunkWidth / 2) && chunks[chunkX, chunkY].transform.position.x < player.transform.position.x + (chunkWidth / 2))
                {
                    if (chunks[chunkX, chunkY].transform.position.z > player.transform.position.z - (chunkHeight / 2) && chunks[chunkX, chunkY].transform.position.z < player.transform.position.z + (chunkHeight / 2))
                    {
                        chunks[chunkX,chunkY].SetActive(true);
                    }
                    else
                    {
                        chunks[chunkX, chunkY].SetActive(false);
                    }
                }
                else
                {
                    chunks[chunkX, chunkY].SetActive(false);
                }
            }
        }
        */
        /*
        Color color = Color.white;
        for (int chunkY = 0; chunkY < chunksY; chunkY++)
        {
            for (int chunkX = 0; chunkX < chunksX; chunkX++)
            {
                if(color == Color.white)
                {
                    color = Color.black;
                }
                else
                {
                    color = Color.white;
                }
                DrawRect(new Rect(new Vector2(chunks[chunkX, chunkY].transform.position.x, chunks[chunkX, chunkY].transform.position.z), new Vector2(chunkWidth, chunkHeight)), color);
            }
        }
        */
=======
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
>>>>>>> parent of 5d240b9 (Towns and building still do not flatten terrain)
        for (int i = 0; i < fireAreas.Count; i++)
        {
            fireAreas[i] = new Rect(new Vector2(fireAreas[i].x, fireAreas[i].y), new Vector2(fireAreas[i].width + 0.001f, fireAreas[i].width + 0.001f));
            DrawRect(fireAreas[i], 0, Color.red);
        }



        for (int i = 0; i < towns.Count; i++)
        {
<<<<<<< HEAD
            DrawRect(towns[i].town, towns[i].townHeight, Color.green);
=======
            DebugDrawRect(towns[i].town, 1, Color.green);

            if (towns[i].townCitizenCount == 0)
            {
                UIController.RemoveMarker(towns[i].townMarker);
            }
>>>>>>> parent of 5d240b9 (Towns and building still do not flatten terrain)
            for (int j = 0; j < towns[i].houses.Count; j++)
            {
                DrawRect(new Rect(new Vector2(towns[i].houses[j].transform.position.x, towns[i].houses[j].transform.position.z), new Vector2(1, 1)), towns[i].townHeight, Color.yellow);
            }
        }
        for (int i = 0; i < trees.Count; i++)
        {
            for (int f = 0; f < fireAreas.Count; f++)
            {
                if (RectContains(trees[i].transform.position, fireAreas[f]))
                {
                    if (!burningTrees.Contains(trees[i]) && rnd.Next(0, 100) == 2)
                    {
                        burningTrees.Add(trees[i]);
                        fires.Add(Instantiate(firePrefab, new Vector3(trees[i].transform.position.x, trees[i].transform.position.y + trees[i].transform.lossyScale.y / 2, trees[i].transform.position.z), Quaternion.identity));
                        for (int k = 0; k < trees[i].GetComponent<Tree>().burnableParts.Count; k++)
                        {
                            trees[i].GetComponent<Tree>().burnableParts[k].GetComponent<MeshRenderer>().sharedMaterial.color = Color.red;
                        }
                    }
                    if (trees[i].GetComponent<Tree>().fireCycleLoop > 1200)
                    {
                        for (int j = 0; j < burningTrees.Count; j++)
                        {
                            if (trees[i] == burningTrees[j] && !burntTrees.Contains(trees[i]))
                            {
                                burntTrees.Add(burningTrees[j]);
                                Destroy(fires[j].gameObject);
                                for (int k = 0; k < trees[i].GetComponent<Tree>().burnableParts.Count; k++)
                                {
                                    trees[i].GetComponent<Tree>().burnableParts[k].GetComponent<MeshRenderer>().sharedMaterial.color = Color.black;
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
<<<<<<< HEAD
=======
    //Check if a object is inside a rect DEPRECATED : USE .Contains INSTEAD
>>>>>>> parent of 5d240b9 (Towns and building still do not flatten terrain)
    public bool RectContains(Vector3 Input, Rect Rect)
    {
        if (Input.x > Rect.x && Input.x < Rect.width + Rect.x &&
            Input.z > Rect.y && Input.z < Rect.height + Rect.y)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

<<<<<<< HEAD
    public void SpawnTrees(int tilePosX, int tilePosY)
    {
        int treesCount = rnd.Next(50, 200);
        int[] treesX = new int[treesCount];
        int[] treesZ = new int[treesCount];
        for (int i = 0; i < treesCount; i++)
=======
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
>>>>>>> parent of 5d240b9 (Towns and building still do not flatten terrain)
        {
            treesX[i] = rnd.Next(0, chunkWidth);
            treesZ[i] = rnd.Next(0, chunkHeight);
        }
        for (int y = 0; y < chunkHeight; y++)
        {
            for (int x = 0; x < chunkWidth; x++)
            {
                for (int i = 0; i < treesCount; i++)
                {
                    if (x == treesX[i] && y == treesZ[i])
                    {
                        Vector3 potentialTreePos = new Vector3(x - (chunkWidth / 2f) + tilePosX, 20 * meshHeightMultiplier, y - (chunkHeight / 2f) + tilePosY);

                        RaycastHit hit;
                        if (Physics.Raycast(potentialTreePos, Vector3.down, out hit, Mathf.Infinity))
                        {
                            if (hit.collider.tag == "Terrain")
                            {
                                Vector3 treePos = new Vector3(hit.point.x, hit.point.y, hit.point.z);
                                Debug.Log("Tree " + i + " is at x position " + treePos.x + ", z position " + treePos.z + " and a height of " + treePos.y);
                                Quaternion quaternion = new Quaternion(0, 0, 0, 0);
                                GameObject tree = Instantiate(treePrefabs[rnd.Next(0, treePrefabs.Count)], treePos, quaternion);
                                trees.Add(tree);
                                tree.gameObject.name = "Tree " + i;

                            }
                        }
                    }
                }
            }
        }
    }

    //public void CreateMap()
    //{
    //    MapDisplay display = FindObjectOfType<MapDisplay>();
    //    display.textureRender = new Renderer[chunksX,chunksY];
    //    display.meshFilter = new MeshFilter[chunksX,chunksY];
    //    display.MeshRenderer = new MeshRenderer[chunksX,chunksY];
    //    display.meshCollider = new MeshCollider[chunksX,chunksY];
    //    int townsAmount = 1;
    //    for (int i = 0; i < townsAmount; i++)
    //    {
    //        Town town = new Town();
    //        Rect townArea = new Rect();


    //        townArea.x = UnityEngine.Random.Range(0, (chunkWidth * chunksX));
    //        townArea.y = UnityEngine.Random.Range(0, (chunkHeight * chunksY));
    //        townArea.width = UnityEngine.Random.Range(6, 20);
    //        townArea.height = UnityEngine.Random.Range(6, 20);

    //        townArea.position = townPos;
    //        townArea.width = 6;
    //        townArea.height = 6;
    //        int houses = rnd.Next(1, 5);
    //        List<GameObject> townHouses = new List<GameObject>();


    //        for (int j = 0; j < houses; j++)
    //        {
    //            townHouses.Add(buildingPrefabs[rnd.Next(0, buildingPrefabs.Count)]);
    //            townHouses[j].transform.position = new Vector3(UnityEngine.Random.Range(townArea.x, townArea.width + townArea.x), 0, UnityEngine.Random.Range(townArea.y, townArea.height + townArea.y));
    //            trees.Add(townHouses[j]);
    //        }

    //        town.town = townArea;
    //        town.houses = townHouses;
    //        towns.Add(town);
    //    }

    //    int fires = rnd.Next(1, 5);
    //    for(int i = 0; i < fires; i++)
    //    {
    //        fireAreas.Add(new Rect(new Vector2(rnd.Next(0,chunksX), rnd.Next(0,chunksY)), new Vector2(0,0)));
    //    }
    //    int treesCount = rnd.Next(100, 500);

    //    for(int i = 0; i < treesCount; i++)
    //    {
    //        float treeX = UnityEngine.Random.Range(0, (chunkWidth * chunksX));
    //        float treeY = UnityEngine.Random.Range(0, (chunkWidth * chunksY));

    //        GameObject tree = treePrefabs[rnd.Next(0, treePrefabs.Count)];
    //        tree.transform.position = new Vector3(treeX, 0, treeY);
    //        trees.Add(tree);
    //    }

    //    for (int chunkY = 0; chunkY < chunksY; chunkY++)
    //    {
    //        for(int chunkX = 0; chunkX < chunksX; chunkX++)
    //        {
    //            GameObject primitive = GameObject.CreatePrimitive(PrimitiveType.Plane);
    //            chunks[chunkX, chunkY] = primitive;
    //            chunks[chunkX, chunkY].transform.position = new Vector3(chunkX * (chunkWidth + 3) + (chunkWidth / 2), 0, chunkY * (chunkHeight + 3) + (chunkHeight / 2));
    //            chunks[chunkX, chunkY].gameObject.name = " Chunk " + chunkX + " " + chunkY;
    //            chunks[chunkX, chunkY].gameObject.tag = "Terrain";
    //            float[,] noiseMap = NoiseGeneration.GenerateNoiseMap(chunkWidth + 3, chunkHeight + 3, seed, noiseScale, octaves, persistance, lacunarity, new Vector2((chunkX * (chunkWidth + 3)), (chunkY * (chunkHeight + 3))), noiseData.normalizeMode);
    //            for (int noiseY = 0; noiseY < chunkHeight; noiseY++)
    //            {
    //                for (int noiseX = 0; noiseX < chunkWidth; noiseX++)
    //                {
    //                    int globalNoiseX = noiseX + (chunkX * chunkWidth ) + 3;
    //                    int globalNoiseY = noiseY + (chunkY * chunkHeight) + 3;
    //                    //Debug.Log("noise Pos " + globalNoiseX + " " + globalNoiseY);

    //                    for (int t = 0; t < towns.Count; t++)
    //                    {
    //                        if (towns[t].town.Contains(new Vector2(globalNoiseX, chunkHeight - globalNoiseY)))
    //                        {
    //                            Debug.Log("Found Rect" + towns[t].town + " on chunk " + chunkX + " , " + chunkY + " at noise position " + noiseX + " , " + noiseY + " world position " + globalNoiseX + " , " + globalNoiseY);
    //                            if (towns[t].townHeight == 0)
    //                            { 
    //                                towns[t].townNoiseHeight = noiseMap[noiseX, noiseY];
    //                                towns[t].townHeight = meshHeightCurve.Evaluate(noiseMap[noiseX, noiseY]) * meshHeightMultiplier;
    //                                Debug.Log("townheight of town " + t + " set to " + towns[t].townHeight);
    //                            }
    //                            else
    //                            {
    //                                Debug.Log("Terrain height set to " + towns[t].townHeight);
    //                                noiseMap[noiseX, noiseY] = towns[t].townNoiseHeight;

    //                            }

    //                        }


    //                    }
    //                }
    //                MapData mapData = GenerateMapData(noiseMap);

    //                if (chunks[chunkX, chunkY].GetComponent<Renderer>() == null)
    //                {
    //                    chunks[chunkX, chunkY].AddComponent<Renderer>();
    //                }
    //                display.textureRender[chunkX, chunkY] = chunks[chunkX, chunkY].GetComponent<Renderer>();

    //                if (chunks[chunkX, chunkY].GetComponent<MeshFilter>() == null)
    //                {
    //                    chunks[chunkX, chunkY].AddComponent<MeshFilter>();
    //                }
    //                display.meshFilter[chunkX, chunkY] = chunks[chunkX, chunkY].GetComponent<MeshFilter>();
    //                if (chunks[chunkX, chunkY].GetComponent<MeshRenderer>() == null)
    //                {
    //                    chunks[chunkX, chunkY].AddComponent<MeshRenderer>();
    //                }
    //                display.MeshRenderer[chunkX, chunkY] = chunks[chunkX, chunkY].GetComponent<MeshRenderer>();
    //                if (chunks[chunkX, chunkY].GetComponent<MeshCollider>() == null)
    //                {
    //                    chunks[chunkX, chunkY].AddComponent<MeshCollider>();
    //                }

    //                display.meshCollider[chunkX, chunkY] = chunks[chunkX, chunkY].GetComponent<MeshCollider>();
    //                display.MeshRenderer[chunkX, chunkY].sharedMaterial = terrainMat;

    //                display.DrawMesh(MeshGen.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, 0, true), chunkX, chunkY);
    //            }
    //        }
    //    }

    //}


    public void CreateMap()
    {
        //Spawn towns
        MapDisplay display = FindObjectOfType<MapDisplay>();
        display.textureRender = new Renderer[chunksX, chunksY];
        display.meshFilter = new MeshFilter[chunksX, chunksY];
        display.MeshRenderer = new MeshRenderer[chunksX, chunksY];
        display.meshCollider = new MeshCollider[chunksX, chunksY];
        int townsAmount = 1;
        for (int i = 0; i < townsAmount; i++)
        {
            Town town = new Town();
            Rect townArea = new Rect();


            //townArea.x = UnityEngine.Random.Range(0, (chunkWidth * chunksX));
            //townArea.y = UnityEngine.Random.Range(0, (chunkHeight * chunksY));
            //townArea.width = UnityEngine.Random.Range(6, 20);
            //townArea.height = UnityEngine.Random.Range(6, 20);

            townArea.position = townPos;
            townArea.width = 6;
            townArea.height = 6;
            int houses = rnd.Next(1, 5);
            List<GameObject> townHouses = new List<GameObject>();


            for (int j = 0; j < houses; j++)
            {
                townHouses.Add(buildingPrefabs[rnd.Next(0, buildingPrefabs.Count)]);
                townHouses[j].transform.position = new Vector3(UnityEngine.Random.Range(townArea.x, townArea.width + townArea.x), 0, UnityEngine.Random.Range(townArea.y, townArea.height + townArea.y));
                trees.Add(townHouses[j]);
            }

            town.town = townArea;
            town.houses = townHouses;
            towns.Add(town);
        }

        //Create noiseMap
        float[,] globalNoiseMap = NoiseGeneration.GenerateNoiseMap(chunksX * chunkWidth, chunksY * chunkHeight, seed, noiseScale, octaves, persistance, lacunarity, new Vector2(0, 0), noiseData.normalizeMode);
        ////Flatten terrain around structures such as towns
        //for(int y = 0; y < globalNoiseMap.GetLength(1); y++)
        //{
        //    for(int x = 0; x < globalNoiseMap.GetLength(0); x++)
        //    {
        //        for (int t = 0; t < towns.Count; t++)
        //        {
        //            if (towns[t].town.Contains(new Vector2(x, y)))
        //            {
        //                if (towns[t].townHeight == 0)
        //                {
        //                    towns[t].townHeight = globalNoiseMap[x, y] * meshHeightMultiplier;
        //                    towns[t].townNoiseHeight = globalNoiseMap[x, y];
        //                }
        //                else
        //                {
        //                    globalNoiseMap[x, y] = towns[t].townNoiseHeight;
        //                }
        //            }
        //        }
        //    }
        //}
        for (int chunkY = 0; chunkY < chunksY; chunkY++)
        {
            for (int chunkX = 0; chunkX < chunksX; chunkX++)
            {
                chunks[chunkX, chunkY] = GameObject.CreatePrimitive(PrimitiveType.Plane);
                chunks[chunkX, chunkY].transform.position = new Vector3((chunkX * chunkWidth), 0, -(chunkY * chunkHeight));
                chunks[chunkX, chunkY].gameObject.name = "Chunk " + chunkX + " , " + chunkY;
                float[,] localNoiseMap = new float[chunkWidth, chunkHeight];
                
                for (int y = 0; y < chunkHeight; y++)
                {
                    for (int x = 0; x < chunkWidth; x++)
                    {
                        //Debug.DrawRay(new Vector3(x + (chunkX * chunkWidth), 0, y + (chunkY * chunkHeight)), Vector3.down, Color.red, Mathf.Infinity);
                        localNoiseMap[x, y] = globalNoiseMap[x + (chunkWidth * chunkX), y + (chunkHeight * chunkY)];
                    }
                }
                MapData mapData = GenerateMapData(localNoiseMap);

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

                display.DrawMesh(MeshGen.GenerateTerrainMesh(localNoiseMap, meshHeightMultiplier, meshHeightCurve, 0, true, chunkX, chunkY, chunkWidth, chunkHeight), chunkX, chunkY);
                Bounds meshBounds = display.meshFilter[chunkX, chunkY].sharedMesh.bounds;
                Debug.Log($"Chunk [{chunkX},{chunkY}] Bounds: sizeX={meshBounds.size.x}, sizeZ={meshBounds.size.z}");
            }
        }




    }

    MapData GenerateMapData(float[,] noiseMap)
    {
        textureData.ApplyToMaterial(terrainMat);
        return new MapData(noiseMap);
    }
    private void OnValidate()
    {
        if (chunkWidth < 1)
        {
            chunkWidth = 1;
        }
        if (chunkHeight < 1)
        {
            chunkHeight = 1;
        }
        if (octaves < 1)
        {
            octaves = 1;
        }
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }
    }
}
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

