using JetBrains.Annotations;
using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public RawImage compass;
    public HeliCollider heli;

    public GameObject iconPrefab;

    List<Marker> markers = new List<Marker>();

    public float compassUnit;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        compassUnit = compass.rectTransform.rect.width / 360f;
        heli = GameObject.FindFirstObjectByType<HeliCollider>();
    }

    // Update is called once per frame
    void Update()
    {
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
        markers.Remove(marker);
    }

    Vector2 GetPosOnCompass(Marker marker)
    {
        Vector2 playerPos = new Vector2(heli.transform.position.x, heli.transform.position.z);
        Vector2 playerFwd = new Vector2(heli.transform.forward.x, heli.transform.forward.z);

        float angle = Vector2.SignedAngle(marker.position - playerPos, playerFwd);

        return new Vector2(compassUnit * angle, 0f);
    }
} 
