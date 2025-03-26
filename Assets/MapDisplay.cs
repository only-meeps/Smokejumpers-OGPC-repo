using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class MapDisplay : MonoBehaviour
{
    public Renderer[,] textureRender;
    public MeshFilter[,] meshFilter;
    public MeshRenderer[,] MeshRenderer;
    public MeshCollider[,] meshCollider;
    public MapGenerator mapGenerator;

    public void DrawMesh(MeshData meshData, int chunkX, int chunkY)
    {

        meshCollider[chunkX , chunkY].sharedMesh = meshData.CreateMesh();
        meshFilter[chunkX , chunkY].sharedMesh = meshData.CreateMesh();
    }
}
