using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
public class Credits : MonoBehaviour
{
    public GameObject camera;
    public float camHeight;
    public float camSpeed;
    public GameObject fracturedHelisFollowTarget;
    public List<GameObject> fracturedHeliList;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Time.timeScale = camSpeed;
        Vector3 centerPoint = new Vector3();
        for(int i = 0; i < fracturedHeliList.Count; i++)
        {
            centerPoint += fracturedHeliList[i].transform.position;
        }
        centerPoint /= fracturedHeliList.Count;
        camera.transform.position = new Vector3(0, centerPoint.y, -10);
        if(centerPoint.y < -420)
        {
            SceneManager.LoadScene("TerrainGenTest");
        }
    }
}
