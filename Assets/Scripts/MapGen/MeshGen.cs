using UnityEngine;

using System.Collections;
using NUnit.Framework;



public static class MeshGen

{



    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve _heightCurve, int levelOfDetail, bool useFlatShading)

    {

        AnimationCurve heightCurve = new AnimationCurve(_heightCurve.keys);



        int meshSimplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;


        int borderedSize = heightMap.GetLength(0);

        int meshSize = borderedSize; // We want the mesh to cover the full heightmap size

        int meshSizeUnsimplified = borderedSize;// For calculating world space



        float topLeftX = 0;
        float topLeftZ = 0; // Assuming your Z is going negative



        int verticesPerLine = meshSize + 1; // Number of vertices for 'meshSize' squares



        MeshData meshData = new MeshData(verticesPerLine, useFlatShading);



        int[,] vertexIndicesMap = new int[borderedSize, borderedSize];

        int vertexIndexCounter = 0;



        for (int y = 0; y < borderedSize; y += meshSimplificationIncrement)
        {

            for (int x = 0; x < borderedSize; x += meshSimplificationIncrement)
            {

                vertexIndicesMap[x, y] = vertexIndexCounter;

                vertexIndexCounter++;

            }

        }



        for (int y = 0; y < borderedSize ; y ++)

        {

            for (int x = 0; x < borderedSize; x ++)

            {

                int vertexIndex = vertexIndicesMap[x, y];

                Vector2 percent = new Vector2((float)x, (float)y);

                float height = heightCurve.Evaluate(heightMap[x, y]) * heightMultiplier;

                Vector3 vertexPosition = new Vector3(topLeftX + percent.x, height, topLeftZ - percent.y);

                meshData.AddVertex(vertexPosition, percent, vertexIndex);



                if (x < borderedSize && y < borderedSize)
                {
                    int a = vertexIndicesMap[x, y];
                    int b = vertexIndicesMap[Mathf.Min(x + 1, borderedSize - 1), y];
                    int c = vertexIndicesMap[x, Mathf.Min(y + 1, borderedSize - 1)];
                    int d = vertexIndicesMap[Mathf.Min(x + 1, borderedSize - 1), Mathf.Min(y + 1, borderedSize - 1)];
                    meshData.AddTriangle(a, d, c);
                    meshData.AddTriangle(d, a, b);
                }

            }

        }



        meshData.ProcessMesh();

        return meshData;

    }

}

public class MeshData
{

    Vector3[] vertices;

    int[] triangles;

    Vector2[] uvs;

    Vector3[] bakedNormals;



    Vector3[] borderVertices;

    int[] borderTriangles;



    int triangleIndex;

    int borderTriangleIndex;



    bool useFlatShading;



    public MeshData(int verticesPerLine, bool useFlatShading)

    {

        this.useFlatShading = useFlatShading;



        vertices = new Vector3[verticesPerLine * verticesPerLine];

        uvs = new Vector2[verticesPerLine * verticesPerLine];

        triangles = new int[(verticesPerLine - 1) * (verticesPerLine - 1) * 6];



        borderVertices = new Vector3[verticesPerLine * 4 + 4];

        borderTriangles = new int[24 * verticesPerLine];

    }

    public Vector3[] ReturnVertices()
    {
        return vertices;
    } 



    public void AddVertex(Vector3 vertexPosition, Vector2 uv, int vertexIndex)

    {

        if (vertexIndex < 0)
        {

            borderVertices[-vertexIndex - 1] = vertexPosition;

        }

        else

        {

            vertices[vertexIndex] = vertexPosition;

            uvs[vertexIndex] = uv;

        }

    }



    public void AddTriangle(int a, int b, int c)

    {

        if (a < 0 || b < 0 || c < 0)

        {

            borderTriangles[borderTriangleIndex] = a;

            borderTriangles[borderTriangleIndex + 1] = b;

            borderTriangles[borderTriangleIndex + 2] = c;

            borderTriangleIndex += 3;

        }

        else

        {

            triangles[triangleIndex] = a;

            triangles[triangleIndex + 1] = b;

            triangles[triangleIndex + 2] = c;

            triangleIndex += 3;

        }

    }



    Vector3[] CalculateNormals()

    {



        Vector3[] vertexNormals = new Vector3[vertices.Length];

        int triangleCount = triangles.Length / 3;

        for (int i = 0; i < triangleCount; i++)

        {

            int normalTriangleIndex = i * 3;

            int vertexIndexA = triangles[normalTriangleIndex];

            int vertexIndexB = triangles[normalTriangleIndex + 1];

            int vertexIndexC = triangles[normalTriangleIndex + 2];



            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);

            vertexNormals[vertexIndexA] += triangleNormal;

            vertexNormals[vertexIndexB] += triangleNormal;

            vertexNormals[vertexIndexC] += triangleNormal;

        }



        int borderTriangleCount = borderTriangles.Length / 3;

        for (int i = 0; i < borderTriangleCount; i++)

        {

            int normalTriangleIndex = i * 3;

            int vertexIndexA = borderTriangles[normalTriangleIndex];

            int vertexIndexB = borderTriangles[normalTriangleIndex + 1];

            int vertexIndexC = borderTriangles[normalTriangleIndex + 2];



            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);



            if (vertexIndexA >= 0)

            {

                vertexNormals[vertexIndexA] += triangleNormal;

            }

            if (vertexIndexB >= 0)

            {

                vertexNormals[vertexIndexB] += triangleNormal;

            }

            if (vertexIndexC >= 0)

            {

                vertexNormals[vertexIndexC] += triangleNormal;

            }

        }





        for (int i = 0; i < vertexNormals.Length; i++)

        {

            vertexNormals[i].Normalize();

        }



        return vertexNormals;



    }



    Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC)

    {

        Vector3 pointA = (indexA < 0) ? borderVertices[-indexA - 1] : vertices[indexA];

        Vector3 pointB = (indexB < 0) ? borderVertices[-indexB - 1] : vertices[indexB];

        Vector3 pointC = (indexC < 0) ? borderVertices[-indexC - 1] : vertices[indexC];



        Vector3 sideAB = pointB - pointA;

        Vector3 sideAC = pointC - pointA;

        return Vector3.Cross(sideAB, sideAC).normalized;

    }



    public void ProcessMesh()

    {

        if (useFlatShading)

        {

            FlatShading();

        }

        else

        {

            BakeNormals();

        }

    }



    void BakeNormals()

    {

        bakedNormals = CalculateNormals();

    }



    void FlatShading()

    {

        Vector3[] flatShadedVertices = new Vector3[triangles.Length];

        Vector2[] flatShadedUvs = new Vector2[triangles.Length];



        for (int i = 0; i < triangles.Length; i++)

        {

            flatShadedVertices[i] = vertices[triangles[i]];

            flatShadedUvs[i] = uvs[triangles[i]];

            triangles[i] = i;

        }



        vertices = flatShadedVertices;

        uvs = flatShadedUvs;

    }



    public Mesh CreateMesh()

    {

        Mesh mesh = new Mesh();

        mesh.vertices = vertices;

        mesh.triangles = triangles;

        mesh.uv = uvs;

        if (useFlatShading)

        {

            mesh.RecalculateNormals();

        }

        else

        {

            mesh.normals = bakedNormals;

        }

        return mesh;

    }





}