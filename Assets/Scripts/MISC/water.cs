using UnityEngine;
using UnityEngine.TerrainUtils;
using UnityEngine.SceneManagement;

public class water : MonoBehaviour
{

    public MeshFilter meshFilter;
    public Vector2 Manualsize;
    public Material waterMat;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Start()
    {
        if(SceneManager.GetActiveScene().name == "WaterTest")
        {
            GenerateWater(Manualsize);
        }
    }
    public void GenerateWater(Vector2 size)
    {
        meshFilter = GetComponent<MeshFilter>();
        float[,] waterPlane = new float[Mathf.RoundToInt(size.x) + 1, Mathf.RoundToInt(size.y) + 1 ];
        for(int y = 0; y < waterPlane.GetLength(1); y++)
        {
            for(int x = 0; x < waterPlane.GetLength(0); x++)
            {
                waterPlane[x, y] = 1;
            }
        }
        AnimationCurve anmCurve = new AnimationCurve();
        anmCurve.AddKey(0, 0);
        anmCurve.AddKey(1, 1);
        MapData mapData = new MapData(waterPlane);
        MeshData meshData = MeshGen.GenerateTerrainMesh(waterPlane, 1, anmCurve, 0, true);
        Mesh mesh = meshData.CreateMesh();
        meshFilter.sharedMesh = mesh;
        GetComponent<MeshRenderer>().sharedMaterial = waterMat;
    }
}
