using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public class CreateTown : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject housePrefab;
    public GameObject roadPrefab;

    [Header("Town Area")]
    public Rect townArea;

    [Header("Layout Settings")]
    public float houseSpacing = 0.5f;
    public float roadWidth = 2f; // Assuming a consistent road width for simplicity
    public float rowSpacing = 1f;
    public List<string> rowPattern = new List<string> { "house", "road" };

    private System.Random random = new System.Random();

    public Quaternion instatiationRotation;
    public GameObject[,] gameObjects;

    void Start()
    {
        float spaceFilledX = 0;
        while(spaceFilledX < townArea.width)
        {
            float spaceFilledY = 0;
            while(spaceFilledY < townArea.height)
            {
                spaceFilledY += houseSpacing;
                Debug.Log(spaceFilledX + "," + spaceFilledY);
                Instantiate(housePrefab, new Vector3(spaceFilledX, 0, spaceFilledY), new Quaternion(-90, 0,0,0));
            }
            spaceFilledX += houseSpacing;

        }
    }
    public void DebugDrawRect(Rect rect, float height, Color color)
    {
        Debug.DrawLine(new Vector3(rect.x, height, rect.y), new Vector3(rect.width + rect.x, height, rect.y), color);
        Debug.DrawLine(new Vector3(rect.width + rect.x, height, rect.y), new Vector3(rect.width + rect.x, height, rect.height + rect.y), color);
        Debug.DrawLine(new Vector3(rect.width + rect.x, height, rect.height + rect.y), new Vector3(rect.x, height, rect.height + rect.y), color);
        Debug.DrawLine(new Vector3(rect.x, height, rect.height + rect.y), new Vector3(rect.x, height, rect.y), color);
    }

}