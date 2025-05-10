using System.Collections;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.XR;
using Unity.AI.Navigation;
using NUnit.Framework.Constraints;
using UnityEngine.AI;
using System.Threading.Tasks;
using JetBrains.Annotations;



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
    public Vector2 townPosEditor;
    public Vector2 townPos;
    public GameObject player;
    public float drawDistance;
    public float chunkDrawDistance;
    public NavMeshSurface navSurface;
    public List<Helipad> helipads = new List<Helipad>();
    public List<Mission> possibleMissions = new List<Mission>();
    public List<Mission> missionsOnMap = new List<Mission>();
    public List<Mission> assaignedMissions = new List<Mission>();
    public GameObject water;
    public float waterHeight;
    public float waterNoiseHeight;
    public GameObject missionPrefab;
    public GameObject missionVerticalLayoutGroup;
    public Rect playableMapSize;
    public int missionsCompleted;
    public int citizensKilled;
    public int citizensDied;
    public int timesRespawned;
    private bool startedScore;
    public bool titleScreen;
    public GameObject titleScreenUIObj;
    public GameObject gameUIObj;
    public GameObject fireFighterPrefab;
    public Material fireFighterDropOffPointMat;
    public Sprite fireFighterDropOffPointMarker;
    public List<Rect> fireFighterDropOffPoints = new List<Rect>();
    public List<float> fireFighterDropOffPointsHeight = new List<float>();
    public int seed;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Awake()
    {
        Time.timeScale = 1f;
        if(PlayerPrefs.GetInt("ManuallyAssaignedLevel") == 0)
        {
            PlayerPrefs.SetInt("Seed", rnd.Next(0, 1000));
        }

        rnd = new System.Random(seed);
        UnityEngine.Random.InitState(seed);
        mapSize = rnd.Next(10, 25);
        drawDistance = PlayerPrefs.GetFloat("DrawDistance");
        chunkDrawDistance = PlayerPrefs.GetFloat("TileDrawDistance");
        gameUIObj.SetActive(false);
        titleScreen = true;
        Mission citizenPickupMission = new Mission();

        citizenPickupMission.markerSprite = townIcon;
        citizenPickupMission.missionTitle = "Pickup Citizens";
        citizenPickupMission.missionDescription = "Pickup citizens and bring them to a hospital";
        citizenPickupMission.missionTag = "CitizenPickupPoint";
        citizenPickupMission.pointsGainedFromMission = 25;
        citizenPickupMission.afterFire = false;

        possibleMissions.Add(citizenPickupMission);

        Mission FireFighterMission = new Mission();

        FireFighterMission.markerSprite = fireFighterDropOffPointMarker;
        FireFighterMission.missionTitle = "Drop off Fire Fighters";
        FireFighterMission.missionDescription = "Pickup fire fighter from a fire station and bring them to a dropoff point";
        FireFighterMission.missionTag = "FireFighterDropOffPoint";
        FireFighterMission.pointsGainedFromMission = 25;
        FireFighterMission.afterFire = false;

        possibleMissions.Add(FireFighterMission);
        meshHeightMultiplier = rnd.Next(24, 40);
        meshHeightCurve = new AnimationCurve();
        meshHeightCurve.AddKey(0, 0);
        meshHeightCurve.AddKey(1, 1);
        for (int i = 0; i < rnd.Next(1, 3); i++)
        {
            meshHeightCurve.AddKey(UnityEngine.Random.Range(0, 0.90f), UnityEngine.Random.Range(0, 1));
        }
        
        playableMapSize = new Rect(new Vector2(0,0), new Vector2((mapSize) * chunkSize, -(mapSize) * chunkSize));
        DrawRect(playableMapSize, 0, townPickupZoneMat, 1, "mapSize", null);
        waterNoiseHeight = UnityEngine.Random.Range(-.2f, .1f);
        waterHeight = waterNoiseHeight * meshHeightMultiplier;
        water.transform.position = new Vector3(0, waterHeight, 0);
        water.transform.localScale = new Vector3(mapSize * chunkSize, 1, mapSize * chunkSize);
        //Create chunk gameobjects and gameobject component lists
        chunks = new GameObject[mapSize, mapSize];
        display.meshFilter = new MeshFilter[mapSize, mapSize];
        display.MeshRenderer = new MeshRenderer[mapSize, mapSize];
        display.meshCollider = new MeshCollider[mapSize, mapSize];
        int helipadsCount = rnd.Next(2, 4);
        int spawnHelipad = rnd.Next(0, helipadsCount);
        int hospitals = 0;
        int gasStations = 0;
        int ContainerPickupPoints = 0;
        int FireStations = 0;
        List<Helipad> helipadSettings = new List<Helipad>();
        while(hospitals < 1 ||  gasStations < 1 || ContainerPickupPoints < 1 || FireStations < 1)
        {
            for( int i = 0; i < helipadSettings.Count; i++)
            {
                Destroy(helipadSettings[i].helipad);
            }
            helipadSettings = new List<Helipad>();
            hospitals = 0;
            gasStations = 0;
            ContainerPickupPoints = 0;
            FireStations = 0;
            for (int i = 0; i < helipadsCount; i++)
            {

                //Spawn helicopter & helipad
                GameObject helipad = Instantiate(heliPadPrefab, new Vector3(rnd.Next(0, mapSize * chunkSize), waterHeight, rnd.Next(-mapSize * chunkSize, 0)), Quaternion.identity);
                
                Debug.Log(i + " " + helipadsCount);
                Helipad helipadSetting = helipad.GetComponent<Helipad>();
                helipadSetting.helipadNoiseHeight = waterNoiseHeight;
                helipadSetting.position = helipad.transform.position;
                helipadSetting.helipad = helipad;
                if (spawnHelipad == i)
                {
                    helipadSetting.spawnHelipad = true;

                }
                if (rnd.Next(0, 2) == 1)
                {
                    FireStations++;
                    helipadSetting.fireStation = true;
                }
                if (rnd.Next(0, 2) == 1)
                {
                    gasStations++;
                    helipadSetting.gasStation = true;
                }
                if (rnd.Next(0, 2) == 1)
                {
                    hospitals++;
                    helipadSetting.hospital = true;
                }
                if (rnd.Next(0, 2) == 1)
                {
                    ContainerPickupPoints++;
                    helipadSetting.containerPickupPoint = true;
                }

                helipadSettings.Add(helipadSetting);


            }
        }
        Debug.Log("spawnpad " + spawnHelipad + " padcount" + helipadsCount);
        for (int i = 0; i < helipadsCount; i++)
        {
            //Assign compass markers to the helipad
            if (helipadSettings[i].spawnHelipad)
            {
                player = Instantiate(helicopterPrefab, new Vector3(helipadSettings[i].helipad.transform.position.x, .85f, helipadSettings[i].helipad.transform.position.z), Quaternion.identity);
                player.GetComponentInChildren<Helicopter>().TitleScreen();
            }
            Marker helipadCompassMarker = helipadSettings[i].helipad.AddComponent<Marker>();
            helipadCompassMarker.icon = helipadIcon;
            UIController.AddMarker(helipadCompassMarker);
            helipads.Add(helipadSettings[i]); 
        }


        int fireFighterDropOffPointMissions = rnd.Next(0, 2);
        List<float> fireFighterDropOffPointsNoiseHeight = new List<float>();
        for(int i = 0; i < fireFighterDropOffPointMissions; i++)
        {
            Rect fireFighterDropOffPoint = new Rect();
            fireFighterDropOffPoint.x = rnd.Next(chunkSize, (mapSize - 1) * chunkSize);
            fireFighterDropOffPoint.y = rnd.Next(-((mapSize + -1) * chunkSize), -chunkSize);

            fireFighterDropOffPoint.width = 4;
            fireFighterDropOffPoint.height = 4;

            fireFighterDropOffPointsNoiseHeight.Add(0);
            fireFighterDropOffPointsHeight.Add(0);
            fireFighterDropOffPoints.Add(fireFighterDropOffPoint);

            GameObject fireFighterDropoffPointObj = new GameObject();
            fireFighterDropoffPointObj.transform.position = new Vector3(fireFighterDropOffPoint.x, 0, fireFighterDropOffPoint.y);
            Marker fireFighterPickupPointMarker = fireFighterDropoffPointObj.AddComponent<Marker>();
            fireFighterPickupPointMarker.icon = fireFighterDropOffPointMarker;
            UIController.AddMarker(fireFighterPickupPointMarker);
        }

        //Pick number of towns
        int townCount = rnd.Next(3, 6);

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
            townX = rnd.Next(chunkSize, ((mapSize - 1) * (chunkSize)));
            townZ = rnd.Next(-((mapSize + - 1) * (chunkSize )), -chunkSize);

            //Assign width
            townWidth = rnd.Next(6, 10);
            townLength = rnd.Next(6, 10);

            //Assign town position and size
            townArea = new Rect(new Vector2(townX, townZ), new Vector2(townWidth, townLength));
            town.town = townArea;

            //Create town pickup zone
            town.townPickupPoint.x = rnd.Next(Mathf.RoundToInt(town.town.x - 30), Mathf.RoundToInt(town.town.width + town.town.x + 30));
            town.townPickupPoint.y = rnd.Next(Mathf.RoundToInt((town.town.y - 30)), Mathf.RoundToInt(town.town.height + town.town.y + 30));
            town.townPickupPoint.width = 6;
            town.townPickupPoint.height = 6;
            for (int h = 0; h < helipads.Count; h++)
            {
                while (Vector2.Distance(town.townPickupPoint.position, new Vector2(helipads[h].position.x, helipads[h].position.z)) < 15)
                {
                    town.townPickupPoint.x = rnd.Next(Mathf.RoundToInt(town.town.x - 30), Mathf.RoundToInt(town.town.width + town.town.x + 30));
                    town.townPickupPoint.y = rnd.Next(Mathf.RoundToInt((town.town.y - 30)), Mathf.RoundToInt(town.town.height + town.town.y + 30));
                    town.townPickupPoint.width = 6;
                    town.townPickupPoint.height = 6;
                }
            }

            

            //Pick number of citizens
            town.townCitizenCount = rnd.Next(5, 10);

            //Add town marker

            //Add town to list of towns
            towns.Add(town);
        }

        //Create a global noise map for all chunks, making sure to generate an extra bit for the missing bits of chunks (generate an extra 2 chunks)
        float[,] globalNoiseMap = NoiseGeneration.GenerateNoiseMap((mapSize + 1) * chunkSize, (mapSize + 1) * chunkSize, seed, noiseScale, octaves, persistance, lacunarity, new Vector2(0, 0), NoiseGeneration.NormalizeMode.Global);

        //Flatten terrain around structures such as towns

        int vertexIndex = 0;
        for (int y = 0; y < globalNoiseMap.GetLength(1); y++)
        {

            for (int x = 0; x < globalNoiseMap.GetLength(0); x++)
            {
                Vector2 percent = new Vector2((float)x, (float)y);
                //Debug.Log(vertices[vertexIndex]);
                //Debug.DrawRay(vertices[vertexIndex], Vector3.up, Color.red, 10000);
                for (int h = 0; h < helipadsCount; h++)
                {
                    if (new Rect(new Vector2(helipads[h].position.x - 15, helipads[h].position.z - 15), new Vector2(30, 30)).Contains(new Vector2(percent.x, -percent.y)))
                    {
                        //If the townheight has not been set yet, then set the townheight to the terrain height, else set the terrain height to the townheight
                        if (!helipads[h].setHeight)
                        {
                            helipads[h].setHeight = true;
                            if (meshHeightCurve.Evaluate(globalNoiseMap[x, y]) * meshHeightMultiplier > waterHeight)
                            {
                                helipads[h].position = new Vector3(helipads[h].position.x, (meshHeightCurve.Evaluate(globalNoiseMap[x, y]) * meshHeightMultiplier) - (helipads[h].helipad.transform.lossyScale.y), helipads[h].position.z);
                                helipads[h].helipad.transform.position = helipads[h].position;
                                helipads[h].helipadNoiseHeight = globalNoiseMap[x, y];
                                
                                if (helipads[h].spawnHelipad)
                                {
                                    player.transform.position = new Vector3(player.transform.position.x, helipads[h].position.y + 2, player.transform.position.z);
                                    player.GetComponentInChildren<Helicopter>().spawnPoint = player.transform.position;
                                }
                            }
                            else
                            {
                                Debug.Log("Terrain Height " + meshHeightCurve.Evaluate(globalNoiseMap[x, y]) * meshHeightMultiplier + " Water Height " + waterHeight);
                                if (helipads[h].spawnHelipad)
                                {
                                    player.transform.position = new Vector3(player.transform.position.x, helipads[h].position.y + 2, player.transform.position.z);
                                    player.GetComponentInChildren<Helicopter>().spawnPoint = player.transform.position;
                                }
                            }

                        }
                        globalNoiseMap[x, y] = helipads[h].helipadNoiseHeight;
                    }
                }
                for(int i = 0; i < fireFighterDropOffPointMissions;  i++)
                {
                    if (fireFighterDropOffPoints[i].Contains(new Vector2(percent.x, -percent.y)))
                    {
                        //If the townheight has not been set yet, then set the townheight to the terrain height, else set the terrain height to the townheight
                        if (fireFighterDropOffPointsHeight[i] == 0)
                        {
                            if (meshHeightCurve.Evaluate(globalNoiseMap[x, y]) * meshHeightMultiplier <= waterHeight)
                            {
                                fireFighterDropOffPointsHeight[i] = waterHeight;
                                fireFighterDropOffPointsNoiseHeight[i] = waterNoiseHeight;
                            }
                            else
                            {
                                fireFighterDropOffPointsHeight[i] = meshHeightCurve.Evaluate(globalNoiseMap[x, y]) * meshHeightMultiplier;
                                fireFighterDropOffPointsNoiseHeight[i] = globalNoiseMap[x, y];
                            }

                        }
                        else
                        {
                            globalNoiseMap[x, y] = fireFighterDropOffPointsNoiseHeight[i];
                        }
                    }
                }

                for (int t = 0; t < towns.Count; t++)
                {
                    if (new Rect(new Vector2(towns[t].town.x, towns[t].town.y), new Vector2(towns[t].town.width + 1, towns[t].town.height + 1)).Contains(new Vector2(percent.x, -percent.y)))
                    {
                        //If the townheight has not been set yet, then set the townheight to the terrain height, else set the terrain height to the townheight
                        if (towns[t].townHeight == 0)
                        {
                            if (meshHeightCurve.Evaluate(globalNoiseMap[x, y]) * meshHeightMultiplier <= waterHeight)
                            {
                                towns[t].townHeight = waterHeight;
                                towns[t].townNoiseHeight = waterNoiseHeight;
                            }
                            else
                            {
                                towns[t].townHeight = meshHeightCurve.Evaluate(globalNoiseMap[x, y]) * meshHeightMultiplier;
                                towns[t].townNoiseHeight = globalNoiseMap[x, y];
                            }

                        }
                        else
                        {
                            globalNoiseMap[x, y] = towns[t].townNoiseHeight;
                        }
                    }
                    if (new Rect(new Vector2(towns[t].townPickupPoint.x, towns[t].townPickupPoint.y), new Vector2(towns[t].townPickupPoint.width + 8, towns[t].townPickupPoint.height + 8)).Contains(new Vector2(percent.x, -percent.y)))
                    {
                        //If the townpickuppointheight has not been set yet, then set the townpickuppointheight to the terrain height, else set the terrain height to the townpickuppointheight
                        if (towns[t].townPickupPointHeight == 0)
                        {
                            //Debug.Log(-Mathf.RoundToInt(towns[t].townPickupPoint.y));
                            if (globalNoiseMap[x, y] * meshHeightMultiplier <= waterHeight)
                            {
                                towns[t].townPickupPointHeight = waterHeight;
                                towns[t].townPickupPointNoiseHeight = waterNoiseHeight;
                            }
                            else
                            {
                                towns[t].townPickupPointHeight = meshHeightCurve.Evaluate(globalNoiseMap[x, y]) * meshHeightMultiplier;
                                towns[t].townPickupPointNoiseHeight = globalNoiseMap[x, y];
                            }

                        }
                        else
                        {
                            globalNoiseMap[x, y] = towns[t].townPickupPointNoiseHeight;
                        }
                    }
                }
                vertexIndex++;
                
            }
        }

        for(int i = 0; i < fireFighterDropOffPointMissions; i++)
        {
            DrawRect(fireFighterDropOffPoints[i], fireFighterDropOffPointsHeight[i], fireFighterDropOffPointMat, .1f, "fire fighter dropoff point " + i.ToString(), "FireFighterDropOffPoint");
        }
        for (int i = 0; i < towns.Count; i++)
        {
            //Draw town pickup zone
            DrawRect(towns[i].townPickupPoint, towns[i].townPickupPointHeight, townPickupZoneMat, .1f, "town pickup point " + i.ToString(), "CitizenPickupPoint");
        }
        //Spawn chunks and assign verticies
        for (int chunkY = 0; chunkY < mapSize; chunkY++)
        {
            for (int chunkX = 0; chunkX < mapSize; chunkX++)
            {
                await DrawChunk(chunkX, chunkY, globalNoiseMap);
            }
        }
        //Create trees
        //Eventually should be based off of noisemap
        List<GameObject> treesToSpawn = new List<GameObject>();
        for (int i = 0; i < rnd.Next(3000, 6000); i++)
        {
            //Create variables
            float treeX;
            float treeZ;

            //Assign position
            treeX = UnityEngine.Random.Range(0, chunkSize * mapSize);
            treeZ = UnityEngine.Random.Range(0, -chunkSize * mapSize);
            for (int h = 0; h < helipadsCount; h++)
            {
                while (new Rect(new Vector2(helipads[h].position.x - 15, helipads[h].position.z - 15), new Vector2(30, 30)).Contains(new Vector2(treeX, treeZ)))
                {
                    treeX = UnityEngine.Random.Range(0, chunkSize * mapSize);
                    treeZ = UnityEngine.Random.Range(0, -chunkSize * mapSize);
                }
            }
            
            //Check if inside town and spawn tree
            RaycastHit hit;
            if (Physics.Raycast(new Vector3(treeX, 1000, treeZ), Vector3.down, out hit))
            {
                if(hit.point.y >= waterHeight && hit.point.y <= meshHeightMultiplier * 0.3)
                {
                    for (int j = 0; j < towns.Count; j++)
                    {
                        if (!towns[j].town.Contains(new Vector2(treeX, treeZ)) && !towns[j].townPickupPoint.Contains(new Vector2(treeX, treeZ)) && hit.collider.gameObject.name != treePrefab.name)
                        {
                            GameObject tree = new GameObject();
                            tree.transform.position = hit.point;
                            treesToSpawn.Add(tree);
                            break;
                        }
                    }
                }
            }

        }
        for(int i = 0; i < treesToSpawn.Count; i++)
        {
            trees.Add(Instantiate(treePrefab, treesToSpawn[i].transform.position, Quaternion.identity));
        }
        for (int i = 0; i < helipadsCount; i++)
        {
            if (!helipads[i].fireStation)
            {
                helipads[i].fireStationObj.SetActive(false);
            }
            if (!helipads[i].gasStation)
            {
                helipads[i].gasStationObj.SetActive(false);
            }
            if (!helipads[i].hospital)
            {
                helipads[i].hospitalObj.SetActive(false);
            }
            if (!helipads[i].containerPickupPoint)
            {
                helipads[i].containerPickupPointObj.SetActive(false);
            }
        }
        
        //Create fires
        for (int i = 0; i < rnd.Next(3, 7); i++)
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


                int housePrefab = rnd.Next(0, buildingPrefabs.Count);
                //Add house to town list
                townHouses.Add(Instantiate(buildingPrefabs[housePrefab], new Vector3(houseX, towns[i].townHeight + (buildingPrefabs[housePrefab].GetComponent<MeshRenderer>().bounds.size.y / 2), houseZ), Quaternion.identity));


                townHouses[j].transform.Rotate(new Vector3(-90, 0, 0));
                //Add house to burnable objects
                trees.Add(townHouses[j]);
            }

            towns[i].houses = townHouses;

            //Spawn citizens
            for (int j = 0; j < towns[i].townCitizenCount; j++)
            {
                int loops = 0;
                Vector3 citizenPos = new Vector3(UnityEngine.Random.Range(towns[i].townPickupPoint.x, towns[i].townPickupPoint.x + towns[i].townPickupPoint.width + 7), citizenPrefab.transform.lossyScale.y + 1 + towns[i].townPickupPointHeight, UnityEngine.Random.Range(towns[i].townPickupPoint.y, towns[i].townPickupPoint.y + towns[i].townPickupPoint.height + 8));
                while(!new Rect(new Vector2(0,0), new Vector2(mapSize * chunkSize, -mapSize * chunkSize)).Contains(new Vector2(citizenPos.x, citizenPos.z), true))
                {
                    citizenPos = new Vector3(UnityEngine.Random.Range(towns[i].townPickupPoint.x, towns[i].townPickupPoint.x + towns[i].townPickupPoint.width + 7), citizenPrefab.transform.lossyScale.y + 1 + towns[i].townPickupPointHeight, UnityEngine.Random.Range(towns[i].townPickupPoint.y, towns[i].townPickupPoint.y + towns[i].townPickupPoint.height + 8));
                    loops++;
                    if (loops > 10)
                    {
                        Debug.Log("Broke loop"); 
                        break;
                        
                    }
                }
                GameObject citizen = Instantiate(citizenPrefab, citizenPos, Quaternion.identity);
                //Set citizen town index
                
                citizen.GetComponent<Citizen>().townIndex = i;
            }
            
        }
        
        for(int i = 0; i < possibleMissions.Count; i++)
        {
            int amountOfMissionOnMap = GameObject.FindGameObjectsWithTag(possibleMissions[i].missionTag).Length;
            if(amountOfMissionOnMap > 0)
            {
                for (int j = 0; j < rnd.Next(1, amountOfMissionOnMap); j++)
                {
                    GameObject missionGameObj = Instantiate(missionPrefab);
                    Mission mission = missionGameObj.GetComponent<Mission>();

                    missionGameObj.transform.parent = missionVerticalLayoutGroup.transform;
                    mission.missionIconImage.sprite = possibleMissions[i].markerSprite;
                    mission.missionTitleText.text = possibleMissions[i].missionTitle;
                    mission.missionDescriptionText.text = possibleMissions[i].missionDescription;
                    mission.missionTag = possibleMissions[i].missionTag;
                    mission.pointsGainedFromMission = possibleMissions[i].pointsGainedFromMission;


                    if (mission.missionTag == "CitizenPickupPoint")
                    {
                        towns[j].townLinkedMission = mission;
                        GameObject townOBJ = new GameObject();
                        townOBJ.name = "Town " + i;
                        townOBJ.transform.position = new Vector3(towns[j].town.x, 0, towns[j].town.y);
                        Marker townMarker = townOBJ.AddComponent<Marker>();
                        townMarker.icon = townIcon;
                        UIController.AddMarker(townMarker);
                        towns[j].townMarker = townMarker;
                    }


                    mission.missionObj = GameObject.FindGameObjectsWithTag(possibleMissions[i].missionTag)[j];
                    assaignedMissions.Add(mission);
                    Debug.Log(assaignedMissions[j].missionTitle);
                }
            }

            
        }

        navSurface.BuildNavMesh();
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
    public void DrawRect(Rect rect, float height, Material mat, float lineWidth, string rectName, string rectTag)
    {
        GameObject rectOBJ = new GameObject();
        rectOBJ.name = rectName;
        if (rectTag != null)
        {
            rectOBJ.tag = rectTag;
        }
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
    public void StartGame()
    {
        Debug.Log("Started game");
        titleScreenUIObj.SetActive(false);
        gameUIObj.SetActive(true);
        titleScreen = false;
        player.GetComponentInChildren<Helicopter>().GameView();
    }

    public void DebugLog()
    {
        Debug.Log("Logged");
    }
    void Update()
    {
        for (int y = 0; y < chunks.GetLength(1); y++)
        {
            for (int x = 0; x < chunks.GetLength(0); x++)
            {
                if (Vector2.Distance(new Vector2(player.transform.position.x, player.transform.position.z), new Vector2(chunks[x, y].transform.position.x, chunks[x, y].transform.position.z)) < chunkDrawDistance)
                {
                    chunks[x, y].SetActive(true);
                }
                else
                {
                    chunks[x, y].SetActive(false);
                }
            }
        }
        if (titleScreen == false)
        {
            for(int h = 0; h < helipads.Count; h++)
            {
                if (player.GetComponentInParent<HeliCollider>().touchingObj == helipads[h].helipad)
                {
                    if (helipads[h].fireStation && helipads[h].fireFightersDeployed <= 6)
                    {
                        helipads[h].fireFightersDeployed++;
                        Instantiate(fireFighterPrefab, helipads[h].fireStationObj.GetComponentInChildren<fireStationDoor>().gameObject.transform.position, Quaternion.identity);
                    }
                    //Spawn firefighters
                }
            }
            for (int i = 0; i < towns.Count; i++)
            {
                if (towns[i].townCitizenCount == 0 && towns[i].townLinkedMission.isActiveAndEnabled)
                {
                    Debug.Log("Mission done");
                    //towns[i].townMarker.gameObject.SetActive(false);
                    towns[i].townLinkedMission.gameObject.SetActive(false);
                    assaignedMissions.Remove(towns[i].townLinkedMission);
                    missionsCompleted++;
                }
            }

            if (assaignedMissions.Count == 0 && player.GetComponentInChildren<Helicopter>().capacity == 0 && startedScore == false)
            {
                startedScore = true;
                StartCoroutine(UIController.Scoring(missionsCompleted, player.GetComponentInChildren<Helicopter>().citizensKilled, player.GetComponentInChildren<Helicopter>().citizensDiedInFire, timesRespawned, seed));
            }
            /*
            if(townPosEditor != townPos)
            {
                townPos = townPosEditor;
                GenerateMap();
            }
            */
            //Debug Draw the fire areas and update their positions
            for (int i = 0; i < fireAreas.Count; i++)
            {
                fireAreas[i] = new Rect(fireAreas[i].position, new Vector2(fireAreas[i].width + 0.03f, fireAreas[i].height + 0.03f));
                DebugDrawRectCentered(fireAreas[i], 1, Color.red);
            }

            //Debug draw the towns and update the compass markers
            for (int i = 0; i < towns.Count; i++)
            {
                DebugDrawRect(towns[i].town, towns[i].townHeight, Color.green);

                if (towns[i].townCitizenCount == 0)
                {
                    UIController.RemoveMarker(towns[i].townMarker);
                }
            }

            //Check trees
            for (int i = 0; i < trees.Count; i++)
            {
                if (Vector3.Distance(player.transform.position, trees[i].transform.position) > drawDistance)
                {
                    trees[i].SetActive(false);
                }
                else
                {
                    trees[i].SetActive(true);
                }
                for (int f = 0; f < fireAreas.Count; f++)
                {
                    //Check if tree is inside of fire
                    if (RectContains(new Vector2(trees[i].transform.position.x, trees[i].transform.position.z), fireAreas[f]))
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
                if (fires[i] != null)
                {
                    if (Vector3.Distance(player.transform.position, fires[i].transform.position) > drawDistance && !burntTrees.Contains(trees[i]))
                    {
                        fires[i].SetActive(false);
                    }
                    else
                    {
                        fires[i].SetActive(true);
                    }
                }

            }
        }
        
    }

    //Check if a object is inside a rect CORNER ALLIGNED DEPRECATED : USE .Contains INSTEAD
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
    //Check if a rect is inside another rect CORNER ALLIGNED
    public bool RectContainsRect(Rect Input, Rect Rect)
    {
        if (Input.x >= Rect.x && Input.x + Input.width <= Rect.x + Rect.width && Input.y >= Rect.y && Input.y + Input.height <= Rect.y + Rect.height)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public async Task DrawChunk(int chunkX, int chunkY, float[,] globalNoiseMap)
    {
        //Create the chunk gameobject, place it, and name it
        chunks[chunkX, chunkY] = new GameObject();
        chunks[chunkX, chunkY].transform.position = new Vector3(chunkX * (chunkSize), 0, -(chunkY * (chunkSize)));
        chunks[chunkX, chunkY].gameObject.name = "Chunk " + chunkX + " , " + chunkY;

        //Create a localnoisemap making sure to get the extra 1 tile to account for 10 verticies and 9 tiles
        float[,] localNoiseMap = new float[chunkSize + 1, chunkSize + 1];

        //Retrieve chunk from globalnoisemap and set it to localnoisemap (less than or equal to making sure to have the extra 1 tile)
        for (int y = 0; y < localNoiseMap.GetLength(0); y++)
        {
            for (int x = 0; x < localNoiseMap.GetLength(1); x++)
            {
                localNoiseMap[x, y] = globalNoiseMap[x + (chunkSize * chunkX), y + (chunkSize * chunkY)];
            }
        }
        MapData mapData = GenerateMapData(localNoiseMap);
        //Add components

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
        MeshData meshData = MeshGen.GenerateTerrainMesh(localNoiseMap, meshHeightMultiplier, meshHeightCurve, 0, true);
        display.DrawMesh(meshData, chunkX, chunkY);
        

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

