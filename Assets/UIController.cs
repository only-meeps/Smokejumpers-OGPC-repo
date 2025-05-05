using JetBrains.Annotations;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public RawImage compass;
    public HeliCollider heli;
    public Slider fuelEfficencyDisplay;
    public Slider fuelDisplay;
    public Slider capacityDisplay;
    public GameObject iconPrefab;
    public Helicopter helicopter;
    public List<GameObject> scoringObjects;
    public TMP_Text mainScore;
    public TMP_Text mainScoreText;
    public Image mainScoreBar;

    public TMP_Text missionsText;
    public TMP_Text missionsScore;
    public Image missionsBar;

    public TMP_Text citizensKilledText;
    public TMP_Text citizensKilledScore;
    public Image citizensKilledBar;

    public TMP_Text citizensDiedText;
    public TMP_Text citizensDiedScore;
    public Image citizensDiedBar;

    public TMP_Text timesRespawnedText;
    public TMP_Text respawnedScore;
    public Image respawnedBar;

    List<Marker> markers = new List<Marker>();

    public float compassUnit;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        helicopter = GameObject.FindObjectsByType<Helicopter>(FindObjectsSortMode.None)[0];
        
        for(int i = 0; i < scoringObjects.Count; i++)
        {
            scoringObjects[i].SetActive(false);
        }

        compassUnit = compass.rectTransform.rect.width / 360f;
        heli = GameObject.FindFirstObjectByType<HeliCollider>();
        fuelDisplay.maxValue = helicopter.fuel;
        capacityDisplay.maxValue = helicopter.maxCapacity;
        fuelEfficencyDisplay.maxValue = 4f;
    }

    // Update is called once per frame
    void Update()
    {
        fuelDisplay.value = helicopter.fuel;
        capacityDisplay.value = helicopter.capacity;
        fuelEfficencyDisplay.value = helicopter.fuelEfficency;

        compass.uvRect = new Rect(heli.transform.localEulerAngles.y / 360f, 0f, 1f, 1f);

        foreach(Marker marker in markers)
        {
            marker.image.rectTransform.anchoredPosition = GetPosOnCompass(marker);
        }
    }

    public void AddMarker (Marker marker)
    {
        GameObject newMarker = Instantiate(iconPrefab, compass.transform);
        marker.image = newMarker.GetComponent<Image>();

        marker.image.sprite = marker.icon;
        markers.Add(marker);
    }

    public void RemoveMarker (Marker marker)
    {
        if(marker != null && marker.GetComponent<Image>() != null)
        {
            marker.GetComponent<Image>().sprite = null;
            markers.Remove(marker);
        }

    }

    public IEnumerator Scoring(int missionsCompleted, int citizensKilled, int citizensDiedToFire, int timesRespawned)
    {
        for (int i = 0; i < scoringObjects.Count; i++)
        {
            scoringObjects[i].SetActive(true);
        }
        mainScore.color = Color.clear;
        mainScore.text = ((missionsCompleted * 200) + (citizensKilled * 200) + (citizensDiedToFire * 100) + (timesRespawned * 300)).ToString();

        mainScoreText.color = Color.clear;

        mainScoreBar.color = Color.clear;

        missionsText.color = Color.clear;
        missionsText.text = "Missions Completed : " + missionsCompleted.ToString();

        missionsBar.color = Color.clear;

        missionsScore.color = Color.clear;
        missionsScore.text = "+ " +(missionsCompleted * 200).ToString();

        citizensKilledText.color = Color.clear;
        citizensKilledText.text = "Citizens Killed " + citizensKilled.ToString();

        citizensKilledBar.color = Color.clear;

        citizensKilledScore.color = Color.clear;
        citizensKilledScore.text = "- " + (citizensKilled * 200).ToString();

        citizensDiedText.color = Color.clear;
        citizensDiedText.text = "Citizens Died To Fire : " + citizensDiedToFire.ToString();

        citizensDiedBar.color = Color.clear;

        citizensDiedScore.color = Color.clear;
        citizensDiedScore.text = "- " + (citizensDiedToFire * 100).ToString();


        timesRespawnedText.color = Color.clear;
        timesRespawnedText.text = "Citizens Died To Fire : " + timesRespawned.ToString();

        respawnedBar.color = Color.clear;

        respawnedScore.color = Color.clear;
        respawnedScore.text = "- " + (timesRespawned * 300).ToString();


        for (int i = 0; i < 100; i++)
        {
            if(int.Parse(mainScore.text) < 0)
            {
                mainScore.color = new Color(1, 0, 0, (float)i/100);
            }
            else
            {
                mainScore.color = new Color(0,1,0,(float)i/100);
            }

            mainScoreBar.color = new Color(1, 1, 1, (float)i / 100);
            mainScoreText.color = new Color(1,1,1,(float)i/100);
            yield return new WaitForSeconds(0.01f);

        }
        yield return new WaitForSeconds(1);
        for (int i = 0; i < 100; i++)
        {
            missionsText.color = new Color(1, 1,1, (float)i/100);
            missionsScore.color = new Color(0, 1, 0, (float)i / 100);
            missionsBar.color = new Color(1, 1, 1, (float)i / 100);
            Debug.Log(new Color(1, 1, 1, (float)i / 100));
            yield return new WaitForSeconds(0.01f);
        }
        yield return new WaitForSeconds(1);
        for (int i = 0; i < 100; i++)
        {
            citizensKilledText.color = new Color(1, 1, 1,(float) i / 100);
            citizensKilledScore.color = new Color(1, 0, 0, (float)i / 100);
            citizensKilledBar.color = new Color(1, 1, 1, (float)i / 100);
            yield return new WaitForSeconds(0.01f);
        }
        yield return new WaitForSeconds(1);
        for (int i = 0; i < 100; i++)
        {
            citizensDiedText.color = new Color(1, 1, 1, (float)i / 100);
            citizensDiedScore.color = new Color(1, 0, 0,(float)i / 100);
            citizensDiedBar.color = new Color(1, 1, 1, (float)i / 100);
            yield return new WaitForSeconds(0.01f);
        }

        yield return new WaitForSeconds(1);
        for (int i = 0; i < 100; i++)
        {
            timesRespawnedText.color = new Color(1, 1, 1, (float)i / 100);
            respawnedScore.color = new Color(1, 0, 0, (float)i / 100);
            respawnedBar.color = new Color(1, 1, 1, (float)i / 100);
            yield return new WaitForSeconds(0.01f);
        }
        Time.timeScale = 0;
    }

    Vector2 GetPosOnCompass(Marker marker)
    {
        Vector2 playerPos = new Vector2(heli.transform.position.x, heli.transform.position.z);
        Vector2 playerFwd = new Vector2(heli.transform.forward.x, heli.transform.forward.z);

        float angle = Vector2.SignedAngle(marker.position - playerPos, playerFwd);

        return new Vector2(compassUnit * angle, 0f);
    }
} 
