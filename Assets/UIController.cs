using JetBrains.Annotations;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

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
    public GameObject settingsObj;
    public Slider helicopterFXSlider;
    public GameObject soundFXObj;
    public GameObject graphicsObj;
    public GameObject scoringUI;
    public GameObject gameUI;
    public GameObject highScoreText;

    public Slider tileDrawDistance;
    public Slider treeDrawDistance;
    public Slider shadowQuality;
    public Slider shadowDrawDistance;

    public TMP_Text levelText;

    public GameObject pauseUI;
    public GameObject loadingScreen;

    public TMP_Dropdown screenRes;

    public GameObject levelSelectScreen;
    public int levelNumber;

    List<Marker> markers = new List<Marker>();

    public float compassUnit;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        loadingScreen.SetActive(false);
        levelNumber = PlayerPrefs.GetInt("Seed");
        levelText.text = PlayerPrefs.GetInt("Seed").ToString();
        scoringUI.SetActive(false);
        helicopterFXSlider.value = PlayerPrefs.GetFloat("HeliFX");
        shadowDrawDistance.value = PlayerPrefs.GetFloat("ShadowDrawDistance");
        shadowQuality.value = PlayerPrefs.GetFloat("ShadowResolution");
        screenRes.value = PlayerPrefs.GetInt("ScreenRes");
        treeDrawDistance.value = PlayerPrefs.GetFloat("DrawDistance");
        tileDrawDistance.value = PlayerPrefs.GetFloat("TileDrawDistance");
        if(PlayerPrefs.GetInt("FirstTime") == 0 || PlayerPrefs.GetInt("FirstTime") == 1)
        {
            PlayerPrefs.SetFloat("FirstTime", 2);
            PlayerPrefs.SetFloat("HeliFX", 100);
            PlayerPrefs.SetFloat("ShadowDrawDistance", 50);
            PlayerPrefs.SetFloat("ShadowResolution", 1);
            PlayerPrefs.SetInt("ScreenRes", 2);
            PlayerPrefs.SetFloat("DrawDistance", 50);
            PlayerPrefs.SetFloat("TileDrawDistance", 200);
        }
        helicopter = GameObject.FindObjectsByType<Helicopter>(FindObjectsSortMode.None)[0];
        helicopter.pauseUI = pauseUI;
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
    public void levelNumberUp()
    {
        if(levelNumber <= Int32.MaxValue)
        {
            levelNumber++;
            levelText.text = levelNumber.ToString();
        }

    }
    public void BugReport()
    {
        Application.OpenURL("https://docs.google.com/forms/d/e/1FAIpQLSdvjgxbda6eEQ3YU5IOB73XdZUm6Xmi6hVMK9SrB4BHKuQRdg/viewform?usp=dialog");
    }
    public void levelNumberDown()
    {
        if(levelNumber >= 0)
        {
            levelNumber--;
            levelText.text = levelNumber.ToString();
        }
    }
    public void SelectLevelNumber()
    {
        loadingScreen.SetActive(true);
        PlayerPrefs.SetInt("ManuallyAssaignedLevel", 1);
        PlayerPrefs.SetInt("Seed", levelNumber);
        SceneManager.LoadScene("TerrainGenTest");
    }
    public void levelSelect()
    {
        Debug.Log("Level select");
        if (levelSelectScreen.activeSelf)
        {
            levelSelectScreen.SetActive(false);
        }
        else
        {
            levelSelectScreen.SetActive(true);
        }
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

    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void ChangeGraphicsSettings()
    {
        PlayerPrefs.SetFloat("ShadowDrawDistance", shadowDrawDistance.value);
        PlayerPrefs.SetFloat("ShadowResolution", shadowQuality.value);
        QualitySettings.shadowDistance = shadowDrawDistance.value;
        if(shadowQuality.value == 0 )
        {
            QualitySettings.shadowResolution = ShadowResolution.Low;
        }
        else if(shadowQuality.value == 1 ) 
        {
            QualitySettings.shadowResolution = ShadowResolution.Medium;
        }
        else if(shadowQuality.value == 2)
        {
            QualitySettings.shadowResolution = ShadowResolution.High;
        }
        else if(shadowQuality.value == 3)
        {
            QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
        }
        PlayerPrefs.SetFloat("DrawDistance", treeDrawDistance.value);
        PlayerPrefs.SetFloat("TileDrawDistance", tileDrawDistance.value);
        if(screenRes.value == 0)
        {
            Screen.SetResolution(1280, 720, true);
        }
        else if(screenRes.value == 1)
        {
            Screen.SetResolution(1920, 1080, true);
        }
        else if(screenRes.value == 2)
        {
            Screen.SetResolution(2560, 1440, true);
        }
        else if(screenRes.value == 3)
        {
            Screen.SetResolution(3840, 2160, true);
        }
        PlayerPrefs.SetInt("ScreenRes", screenRes.value);
    }

    public void OpenGraphicsSettings()
    {
        if (!graphicsObj.activeSelf)
        {
            graphicsObj.SetActive(true);
            soundFXObj.SetActive(false);
        }
    }

    public void OpenSoundSettings()
    {
        if (!soundFXObj.activeSelf)
        {
            graphicsObj.SetActive(false);
            soundFXObj.SetActive(true);
        }
    }
    public void AddMarker (Marker marker)
    {
        GameObject newMarker = Instantiate(iconPrefab, compass.transform);
        marker.image = newMarker.GetComponent<Image>();

        marker.image.sprite = marker.icon;
        markers.Add(marker);
    }

    public void ChangeVolumeSettings()
    {
        PlayerPrefs.SetFloat("HeliFX", helicopterFXSlider.value);
    }
    public void RemoveMarker (Marker marker)
    {
        if(marker != null && marker.GetComponent<Image>() != null)
        {
            marker.GetComponent<Image>().sprite = null;
            markers.Remove(marker);
        }

    }

    public void Settings()
    {
        if(settingsObj.activeSelf)
        {
            settingsObj.SetActive(false);
        }
        else
        {
            settingsObj.SetActive(true);
        }
    }
     
    public void Quit()
    {
        Application.Quit();
    }

    public void Credits()
    {
        SceneManager.LoadScene("Credits");
    }
    public IEnumerator Scoring(int missionsCompleted, int citizensKilled, int citizensDiedToFire, int timesRespawned, int seed)
    {
        scoringUI.SetActive(true);
        gameUI.SetActive(false);
        if(PlayerPrefs.GetInt("Level " + seed.ToString()) > (missionsCompleted * 200) + (citizensKilled * 200) + (citizensDiedToFire * 100) + (timesRespawned * 300))
        {
            highScoreText.SetActive(true);
            PlayerPrefs.SetInt("Level " + seed.ToString(), (missionsCompleted * 200) + (citizensKilled * 200) + (citizensDiedToFire * 100) + (timesRespawned * 300));
        }
        else
        {
            highScoreText.SetActive(false);
        }

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
