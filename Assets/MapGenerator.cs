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

public class MapGenerator : MonoBehaviour 
{
    public enum DrawMode { NoiseMap, ColorMap, Mesh};
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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        chunks = new GameObject[chunksX ,chunksY];
        terrainMat = new Material(terrainMat);
        DangerZone = new Rect(new Vector2(0,0), new Vector2(chunkWidth, chunkHeight));
        rnd = new System.Random(noiseData.seed);
        UnityEngine.Random.InitState(noiseData.seed);
        CreateMap();
    }
    private void Start()
    {
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
        if(townPosEditor != townPos)
        { 
            for(int i = 0; i < GameObject.FindGameObjectsWithTag("Terrain").Length; i++)
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
        for (int i = 0; i < fireAreas.Count; i++)
        {
            fireAreas[i] = new Rect(new Vector2(fireAreas[i].x, fireAreas[i].y), new Vector2(fireAreas[i].width + 0.001f, fireAreas[i].width + 0.001f));
            DrawRect(fireAreas[i], 0, Color.red);
        }
        
        

        for (int i = 0; i < towns.Count; i++)
        {
            DrawRect(towns[i].town, towns[i].townHeight, Color.green);
            for (int j = 0; j < towns[i].houses.Count; j++)
            {
                DrawRect(new Rect(new Vector2(towns[i].houses[j].transform.position.x, towns[i].houses[j].transform.position.z), new Vector2(1,1)), towns[i].townHeight, Color.yellow);
            }
        }
        for (int i = 0; i < trees.Count; i++)
        {
            for(int f = 0; f < fireAreas.Count; f++)
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

    public void SpawnTrees(int tilePosX, int tilePosY)
    {
        int treesCount = rnd.Next(50, 200);
        int[] treesX = new int[treesCount];
        int[] treesZ = new int[treesCount];
        for (int i = 0; i < treesCount; i++)
        {
            treesX[i] = rnd.Next(0, chunkWidth);
            treesZ[i] = rnd.Next(0, chunkHeight);
        }
        for(int y = 0; y < chunkHeight; y++)
        {
            for(int x = 0; x < chunkWidth; x++)
            {
                for (int i = 0; i < treesCount; i++)
                {
                    if (x == treesX[i] && y == treesZ[i])
                    {
                        Vector3 potentialTreePos = new Vector3(x - (chunkWidth / 2f) + tilePosX,  20 * meshHeightMultiplier, y - (chunkHeight / 2f) + tilePosY);

                        RaycastHit hit;
                        if (Physics.Raycast(potentialTreePos, Vector3.down, out hit, Mathf.Infinity))
                        {
                            if (hit.collider.tag == "Terrain")
                            {
                                Vector3 treePos = new Vector3(hit.point.x, hit.point.y, hit.point.z);
                                Debug.Log("Tree " + i + " is at x position " + treePos.x + ", z position " + treePos.z + " and a height of " + treePos.y);
                                Quaternion quaternion = new Quaternion(0, 0, 0, 0);
                                GameObject tree = Instantiate(treePrefabs[rnd.Next(0,treePrefabs.Count)], treePos, quaternion);
                                trees.Add(tree);
                                tree.gameObject.name = "Tree " + i;
                                
                            }
                        }
                    }
                }
            }
        }
    }
    /*
    public void CreateMap()
    {
        MapDisplay display = FindObjectOfType<MapDisplay>();
        display.textureRender = new Renderer[chunksX,chunksY];
        display.meshFilter = new MeshFilter[chunksX,chunksY];
        display.MeshRenderer = new MeshRenderer[chunksX,chunksY];
        display.meshCollider = new MeshCollider[chunksX,chunksY];
        int townsAmount = 1;
        for (int i = 0; i < townsAmount; i++)
        {
            Town town = new Town();
            Rect townArea = new Rect();

            /*
            townArea.x = UnityEngine.Random.Range(0, (chunkWidth * chunksX));
            townArea.y = UnityEngine.Random.Range(0, (chunkHeight * chunksY));
            townArea.width = UnityEngine.Random.Range(6, 20);
            townArea.height = UnityEngine.Random.Range(6, 20);
            
            townArea.position = townPos;
            townArea.width = 6;
            townArea.height = 6;
            int houses = rnd.Next(1, 5);
            List<GameObject> townHouses = new List<GameObject>();

            /*
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
        
        int fires = rnd.Next(1, 5);
        for(int i = 0; i < fires; i++)
        {
            fireAreas.Add(new Rect(new Vector2(rnd.Next(0,chunksX), rnd.Next(0,chunksY)), new Vector2(0,0)));
        }
        int treesCount = rnd.Next(100, 500);
        
        for(int i = 0; i < treesCount; i++)
        {
            float treeX = UnityEngine.Random.Range(0, (chunkWidth * chunksX));
            float treeY = UnityEngine.Random.Range(0, (chunkWidth * chunksY));

            GameObject tree = treePrefabs[rnd.Next(0, treePrefabs.Count)];
            tree.transform.position = new Vector3(treeX, 0, treeY);
            trees.Add(tree);
        }
        
        for (int chunkY = 0; chunkY < chunksY; chunkY++)
        {
            for(int chunkX = 0; chunkX < chunksX; chunkX++)
            {
                GameObject primitive = GameObject.CreatePrimitive(PrimitiveType.Plane);
                chunks[chunkX, chunkY] = primitive;
                chunks[chunkX, chunkY].transform.position = new Vector3(chunkX * (chunkWidth) + (chunkWidth / 2), 0, chunkY * (chunkHeight) + (chunkHeight / 2));
                chunks[chunkX, chunkY].gameObject.name = " Chunk " + chunkX + " " + chunkY;
                chunks[chunkX, chunkY].gameObject.tag = "Terrain";
                float[,] noiseMap = NoiseGeneration.GenerateNoiseMap(chunkWidth, chunkHeight, seed, noiseScale, octaves, persistance, lacunarity, new Vector2((chunkX * (chunkWidth)), (chunkY * (chunkHeight))), noiseData.normalizeMode);
                for (int noiseY = 0; noiseY < chunkHeight; noiseY++)
                {
                    for (int noiseX = 0; noiseX < chunkWidth; noiseX++)
                    {
                        int globalNoiseX = noiseX + (chunkX * chunkWidth);
                        int globalNoiseY = noiseY + (chunkY * chunkHeight);
                        //Debug.Log("noise Pos " + globalNoiseX + " " + globalNoiseY);
                        
                        for (int t = 0; t < towns.Count; t++)
                        {
                            if (towns[t].town.Contains(new Vector2(globalNoiseX, chunkHeight - globalNoiseY)))
                            {
                                Debug.Log("Found Rect" + towns[t].town + " on chunk " + chunkX + " , " + chunkY + " at noise position " + noiseX + " , " + noiseY + " world position " + globalNoiseX + " , " + globalNoiseY);
                                if (towns[t].townHeight == 0)
                                { 
                                    towns[t].townNoiseHeight = noiseMap[noiseX, noiseY];
                                    towns[t].townHeight = meshHeightCurve.Evaluate(noiseMap[noiseX, noiseY]) * meshHeightMultiplier;
                                    Debug.Log("townheight of town " + t + " set to " + towns[t].townHeight);
                                }
                                else
                                {
                                    Debug.Log("Terrain height set to " + towns[t].townHeight);
                                    noiseMap[noiseX, noiseY] = towns[t].townNoiseHeight;
                                    
                                }

                            }
                            

                        }
                    }
                    MapData mapData = GenerateMapData(noiseMap);

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

                    display.DrawMesh(MeshGen.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, 0, true), chunkX, chunkY);
                }
            }
        }
        
    }
           */

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
        float[,] globalNoiseMap = NoiseGeneration.GenerateNoiseMap(chunksX * chunkWidth, chunksY * chunkHeight, seed, noiseScale, octaves, persistance, lacunarity, new Vector2(0,0), noiseData.normalizeMode);


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
            for(int chunkX = 0; chunkX < chunksX; chunkX++)
            {
                chunks[chunkX, chunkY] = GameObject.CreatePrimitive(PrimitiveType.Plane);
                chunks[chunkX, chunkY].transform.position = new Vector3((chunkX * chunkWidth), 0, -(chunkY * chunkHeight));
                chunks[chunkX, chunkY].gameObject.name = "Chunk " + chunkX + " , " + chunkY;
                float[,] localNoiseMap = new float[chunkWidth, chunkHeight];
                for(int y = 0; y < chunkHeight; y++)
                {
                    for(int x = 0; x < chunkWidth; x++)
                    {
                        localNoiseMap[x,y] = globalNoiseMap[x + (chunkWidth * chunkX), y + (chunkHeight * chunkY)];
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

                display.DrawMesh(MeshGen.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, meshHeightCurve, 0, true), chunkX, chunkY);
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
        if(chunkWidth < 1)
        {
            chunkWidth = 1;
        }
        if(chunkHeight < 1)
        {
            chunkHeight = 1;
        }
        if (octaves < 1)
        {
            octaves = 1;
        }
        if(lacunarity < 1)
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

