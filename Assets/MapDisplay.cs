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
        Mesh mesh = meshData.CreateMesh(); // Create the mesh once
        meshCollider[chunkX, chunkY].sharedMesh = mesh;
        meshCollider[chunkX, chunkY].sharedMesh.RecalculateBounds();
        meshFilter[chunkX, chunkY].sharedMesh = mesh;
    }
}
