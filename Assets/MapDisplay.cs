using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Unity.AI.Navigation;

public class MapDisplay : MonoBehaviour
{
    public MeshFilter[,] meshFilter;
    public MeshRenderer[,] MeshRenderer;
    public MeshCollider[,] meshCollider;
    public NavMeshSurface[,] navMeshSurfaces;
    public MapGenerator mapGenerator;

    public void DrawMesh(MeshData meshData, int chunkX, int chunkY)
    {
        Mesh mesh = meshData.CreateMesh(); // Create the mesh once
        Debug.Log(mesh.isReadable);
        meshCollider[chunkX, chunkY].sharedMesh = mesh;
        meshCollider[chunkX, chunkY].sharedMesh.RecalculateBounds();
        meshFilter[chunkX, chunkY].sharedMesh = mesh;
        //navMeshSurfaces[chunkX, chunkY].BuildNavMesh();
    }
}
