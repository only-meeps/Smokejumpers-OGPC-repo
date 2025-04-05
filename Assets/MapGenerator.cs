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
        navMeshSurface = new NavMeshSurface[mapSize,mapSize];
        chunks = new GameObject[mapSize, mapSize];
        GameObject helipad = Instantiate(heliPadPrefab, new Vector3(UnityEngine.Random.Range(0, mapSize * chunkSize), -1, UnityEngine.Random.Range(0, mapSize * chunkSize)), Quaternion.identity);
        Marker helipadCompassMarker = helipad.AddComponent<Marker>();
        helipadCompassMarker.icon = helipadIcon;
        UIController.AddMarker(helipadCompassMarker);
        Instantiate(helicopterPrefab, new Vector3(helipad.transform.position.x, .85f, helipad.transform.position.z), Quaternion.identity);
        float[,] globalNoiseMap = NoiseGeneration.GenerateNoiseMap(mapSize * chunkSize, mapSize * chunkSize, rnd.Next(0, 100), noiseScale, octaves, persistance, lacunarity, new Vector2(0, 0), NoiseGeneration.NormalizeMode.Global);
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
        for (int chunkY = 0; chunkY < mapSize; chunkY++)
        {
            for (int chunkX = 0; chunkX < mapSize; chunkX++)
            {
                chunks[chunkX, chunkY] = GameObject.CreatePrimitive(PrimitiveType.Plane);
                chunks[chunkX, chunkY].transform.position = new Vector3((chunkX * chunkSize), 0, -(chunkY * chunkSize));
                chunks[chunkX, chunkY].gameObject.name = "Chunk " + chunkX + " , " + chunkY;
                float[,] localNoiseMap = new float[chunkSize, chunkSize];

                for (int y = 0; y < chunkSize; y++)
                {
                    for (int x = 0; x < chunkSize; x++)
                    {
                        //Debug.DrawRay(new Vector3(x + (chunkX * chunkWidth), 0, y + (chunkY * chunkHeight)), Vector3.down, Color.red, Mathf.Infinity);
                        localNoiseMap[x, y] = globalNoiseMap[x + (chunkSize * chunkX), y + (chunkSize * chunkY)];
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

                display.DrawMesh(MeshGen.GenerateTerrainMesh(localNoiseMap, meshHeightMultiplier, meshHeightCurve, 0, true, chunkX, chunkY, chunkSize, chunkSize), chunkX, chunkY);
                Bounds meshBounds = display.meshFilter[chunkX, chunkY].sharedMesh.bounds;
            }
        }
        int reps = rnd.Next(1, 5);
        for (int i = 0; i < reps; i++)
        {
            Town town = new Town();
            Rect townArea = new Rect();
            float townWidth;
            float townLength;
            float townX;
            float townZ;
            townX = UnityEngine.Random.Range(0, mapSize * chunkSize);
            townZ = UnityEngine.Random.Range(0, mapSize * chunkSize);
            while (Vector2.Distance(new Vector2(townX, townZ), new Vector2(helipad.transform.position.x, helipad.transform.position.z)) < 20)
            {
                townX = UnityEngine.Random.Range(0, mapSize * chunkSize);
                townZ = UnityEngine.Random.Range(0, mapSize * chunkSize);
            }
            townWidth = UnityEngine.Random.Range(6, 20);
            townLength = UnityEngine.Random.Range(6, 20);
            while (!RectContainsRect(new Rect(townX, townZ, townWidth, townLength), new Rect(new Vector2(0, 0), new Vector2(chunkSize * mapSize, chunkSize * mapSize))))
            {
                townX = UnityEngine.Random.Range(0, mapSize * chunkSize);
                townZ = UnityEngine.Random.Range(0, mapSize * chunkSize);
                townWidth = UnityEngine.Random.Range(15, 20);
                townLength = UnityEngine.Random.Range(15, 20);
            }
            Debug.Log(new Rect(townX, townZ, townWidth, townLength));
            townArea = new Rect(new Vector2(townX, townZ), new Vector2(townWidth, townLength));
            town.town = townArea;
            town.townPickupPoint.x = UnityEngine.Random.Range(town.town.x - 15, town.town.width + town.town.x + 15);
            town.townPickupPoint.y = UnityEngine.Random.Range(town.town.y - 15, town.town.width + town.town.y + 15);
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

            treeX = UnityEngine.Random.Range(0, chunkSize * mapSize);
            treeZ = UnityEngine.Random.Range(0, chunkSize * mapSize);

            for (int j = 0; j < towns.Count; j++)
            {
                if (!towns[j].town.Contains(new Vector2(treeX, treeZ)) && !towns[j].townPickupPoint.Contains(new Vector2(treeX, treeZ)))
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
                houseX = UnityEngine.Random.Range(towns[i].town.x, towns[i].town.width + towns[i].town.x);
                houseZ = UnityEngine.Random.Range(towns[i].town.y, towns[i].town.height + towns[i].town.y);
                townHouses.Add(Instantiate(buildingPrefabs[rnd.Next(0, buildingPrefabs.Count)], new Vector3(houseX, towns[i].townHeight, houseZ), Quaternion.identity));
                trees.Add(townHouses[j]);
            }

            towns[i].houses = townHouses;
            for (int j = 0; j < towns[i].townCitizenCount; j++)
            {
                GameObject citizen = Instantiate(citizenPrefab, new Vector3(UnityEngine.Random.Range(towns[i].townPickupPoint.x - 10, towns[i].townPickupPoint.x + towns[i].townPickupPoint.width + 10), citizenPrefab.transform.lossyScale.y + 1, UnityEngine.Random.Range(towns[i].townPickupPoint.y - 10, towns[i].townPickupPoint.y + towns[i].townPickupPoint.height + 10)), Quaternion.identity);
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
    MapData GenerateMapData(float[,] noiseMap)
    {
        textureData.ApplyToMaterial(terrainMat);
        return new MapData(noiseMap);
    }
    public void DebugDrawRectCentered(Rect rect, float height, Color color)
    {
        Debug.DrawLine(new Vector3(-(rect.width / 2) + rect.x, height, -(rect.height / 2) + rect.y), new Vector3((rect.width / 2) + rect.x, height, -(rect.height / 2) + rect.y), color);
        Debug.DrawLine(new Vector3((rect.width / 2) + rect.x, height, -(rect.height / 2) + rect.y), new Vector3((rect.width / 2) + rect.x, height, (rect.height / 2) + rect.y), color);
        Debug.DrawLine(new Vector3((rect.width / 2) + rect.x, height, (rect.height / 2) + rect.y), new Vector3(-(rect.width / 2) + rect.x, height, (rect.height / 2) + rect.y), color);
        Debug.DrawLine(new Vector3(-(rect.width / 2) + rect.x, height, (rect.height / 2) + rect.y), new Vector3(-(rect.width / 2) + rect.x, height, -(rect.height / 2) + rect.y), color);
    }
    // Update is called once per frame
    void Update()
    {

        for (int i = 0; i < fireAreas.Count; i++)
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
                DebugDrawRect(new Rect(new Vector2(towns[i].houses[j].transform.position.x, towns[i].houses[j].transform.position.z), new Vector2(1, 1)), 1, Color.yellow);
            }
        }
        for (int i = 0; i < trees.Count; i++)
        {
            for (int f = 0; f < fireAreas.Count; f++)
            {
                if (RectContains(trees[i].transform.position, fireAreas[f]))
                {
                    if (!burningTrees.Contains(trees[i]) && rnd.Next(0, 500) == 2)
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
    }
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

